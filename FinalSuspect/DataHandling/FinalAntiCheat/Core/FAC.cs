using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Patches.Game_Vanilla;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Core;

/// <summary>
///     Final Anti-Cheat（FAC）反作弊引擎。
///     通过反射注册 RPC 处理器，使用处理器管道处理传入的 RPC，并管理作弊检测警告。
/// </summary>
public static class FAC
{
    #region Constants

    /// <summary>将玩家标记为疑似作弊者所需的检测计数阈值。</summary>
    private const int CheatWarningThreshold = 3;

    /// <summary>将作弊状态提升为严重（SB）级别所需的检测计数阈值。</summary>
    private const int SevereCheatThreshold = 10;

    /// <summary>当具体作弊类型尚未确定时使用的默认原因字符串。</summary>
    private const string DefaultReason = "Hacking";

    #endregion

    #region Fields

    private static int _detectionCount;

    /// <summary>
    ///     最近一次作弊处理操作的时间戳（Unix 秒）。
    ///     用于限制连续作弊通知/踢出操作的频率。
    ///     初始值 -1 表示尚未执行任何操作。
    /// </summary>
    public static long LastHandleCheater = -1;

    /// <summary>
    ///     O(1) 查找表，将每个 RPC 调用 ID 映射到处理该 RPC 的 <see cref="IRpcHandler" /> 实例列表。
    ///     在静态构造函数中通过反射一次性填充。
    /// </summary>
    public static readonly Dictionary<byte, List<IRpcHandler>> Handlers = new();

    #endregion

    #region Initialization

    static FAC()
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
                     .Where(t => typeof(IRpcHandler).IsAssignableFrom(t) && !t.IsAbstract))
        {
            var handler = (IRpcHandler)Activator.CreateInstance(type);
            if (handler == null) continue;

            foreach (var rpcId in handler.TargetRpcs)
            {
                if (!Handlers.TryGetValue(rpcId, out var list))
                {
                    list = new List<IRpcHandler>();
                    Handlers[rpcId] = list;
                }

                list.Add(handler);
            }
        }
    }

    /// <summary>
    ///     将内部检测计数器重置为零。
    ///     应在加入新的大厅/游戏会话时调用。
    /// </summary>
    public static void Init_FAC()
    {
        _detectionCount = 0;
    }

    #endregion

    #region Public API

    /// <summary>
    ///     增加（或减少）检测警告计数器，并相应更新主机的错误显示。
    /// </summary>
    /// <param name="count">
    ///     要累加到计数器的值。正值表示检测到作弊；
    ///     负值（如 -1）用于在处理完非作弊 RPC 后递减计数器。
    /// </param>
    public static void WarnHost(int count = 1)
    {
        _detectionCount += count;
        if (!ErrorText.Instance) return;

        ErrorText.Instance.cheatDetected = _detectionCount > CheatWarningThreshold;
        ErrorText.Instance.SBDetected = _detectionCount > SevereCheatThreshold;

        if (ErrorText.Instance.cheatDetected)
            ErrorText.Instance.AddError(
                ErrorText.Instance.SBDetected ? ErrorCode.SBDetected : ErrorCode.CheatDetected);
        else
            ErrorText.Instance.Clear();
    }

    /// <summary>
    ///     RPC 主处理管道。对每个来自非主机玩家的传入 RPC 进行调用。
    ///     先通过过载检测，再将 RPC 分发到匹配的 <see cref="IRpcHandler" /> 实例。
    /// </summary>
    /// <param name="pc">发送该 RPC 的玩家。</param>
    /// <param name="callId">RPC 调用标识符（从 <see cref="RpcCalls" /> 转换而来）。</param>
    /// <param name="reader">包含 RPC 负载的 Hazel 消息读取器。</param>
    /// <param name="notify">
    ///     输出参数：是否应将此 RPC 转发给其他客户端。
    ///     当 RPC 被拦截时设为 <c>false</c>。
    /// </param>
    /// <param name="reason">
    ///     输出参数：可读的作弊分类原因。
    ///     初始值为 <see cref="DefaultReason" />，可能由处理器更新。
    /// </param>
    /// <param name="ban">输出参数：是否应封禁发送者。</param>
    /// <returns>
    ///     若 RPC 已被处理（检测到作弊并已处理）则返回 <c>true</c>；
    ///     若 RPC 通过了所有检查应正常转发则返回 <c>false</c>。
    /// </returns>
    public static bool ReceiveRpc(PlayerControl pc, byte callId, MessageReader reader,
        out bool notify, out string reason, out bool ban)
    {
        notify = true;
        reason = DefaultReason;
        ban = false;

        if (!ConfigManager.EnableFAC.Value || !pc || reader == null || pc.AmOwner)
            return false;

        try
        {
            // 阶段 1：过载检测（RPC 洪泛攻击）
            if (pc.GetCheatData().HandleIncomingRpc(callId))
            {
                NotifyOverloadDetected(pc);
                ban = true;
                notify = false;
                return true;
            }

            // 阶段 2：分发到已注册的处理器
            if (!Handlers.TryGetValue(callId, out var matchingHandlers))
            {
                WarnHost(-1);
                return false;
            }

            var sr = MessageReader.Get(reader);

            foreach (var handler in matchingHandlers)
            {
                if (ProcessHandler(pc, callId, sr, handler, ref notify, ref reason, ref ban))
                    return true;
            }
        }
        catch (Exception e)
        {
            Fatal(e.ToString(), "FAC");
        }

        WarnHost(-1);
        return false;
    }

    /// <summary>
    ///     为指定玩家显示作弊检测通知弹窗。
    /// </summary>
    /// <param name="pc">疑似作弊者。</param>
    /// <param name="text">格式化字符串（必须包含单个 {0} 占位符用于彩色玩家名）。</param>
    public static void HandleCheat(PlayerControl pc, string text)
    {
        NotificationPopperPatch.NotificationPop(string.Format(text, pc.GetColoredName()));
    }

    /// <summary>
    ///     清理所有已注册处理器中为已离开游戏的玩家保存的每玩家状态。
    /// </summary>
    /// <param name="playerId">断开连接的玩家 ID。</param>
    public static void Dispose(byte playerId)
    {
        foreach (var list in Handlers.Values)
        {
            foreach (var handler in list)
                handler.Dispose(playerId);
        }
    }

    #endregion

    #region Private Helpers

    /// <summary>显示 RPC 过载通知，根据是否为主机选择对应的本地化字符串。</summary>
    private static void NotifyOverloadDetected(PlayerControl pc)
    {
        var key = AmHost
            ? CheatDetected.Overload
            : CheatDetected.Overload_NotHost;
        NotificationPopperPatch.NotificationPop(
            string.Format(GetString(key), pc.GetColoredName()));
    }

    /// <summary>
    ///     针对传入的 RPC 处理单个 <see cref="IRpcHandler" />。
    ///     根据游戏阶段分发到对应的处理器方法。
    /// </summary>
    private static bool ProcessHandler(PlayerControl pc, byte callId, MessageReader sr,
        IRpcHandler handler, ref bool notify, ref string reason, ref bool ban)
    {
        // 无效（非原版）RPC —— 路由到 HandleInvalidRPC
        if (!Enum.IsDefined(typeof(RpcCalls), callId))
            return ProcessInvalidRpc(pc, sr, handler, ref notify, ref reason, ref ban);

        // HandleAll：在所有游戏阶段无条件检查
        if (handler.HandleAll(pc, sr, ref notify, ref reason, ref ban))
            return true;

        // HandleLobby：仅在大厅中检查
        if (IsLobby && handler.HandleLobby(pc, sr, ref notify, ref reason, ref ban))
        {
            if (AmHost) return true;
            NotificationPopperPatch.NotificationPop(GetString("Warning.RoomBroken"));
            notify = false;
            return true;
        }

        // 游戏阶段特定处理器（按最具体优先的顺序检查）
        return (IsInGame && handler.HandleGame_All(pc, sr, ref notify, ref reason, ref ban))
               || (IsInTask && handler.HandleGame_InTask(pc, sr, ref notify, ref reason, ref ban))
               || (IsInMeeting && handler.HandleGame_InMeeting(pc, sr, ref notify, ref reason, ref ban));
    }

    /// <summary>
    ///     通过处理器的 <see cref="IRpcHandler.HandleInvalidRPC" /> 方法处理无效（非原版）RPC。
    ///     检测到已知作弊 RPC 时通知玩家，或对潜在作弊发出警告。
    /// </summary>
    private static bool ProcessInvalidRpc(PlayerControl pc, MessageReader sr,
        IRpcHandler handler, ref bool notify, ref string reason, ref bool ban)
    {
        notify = false;

        if (handler.HandleInvalidRPC(pc, sr, ref notify, ref reason, ref ban))
        {
            ban = true;
            reason = reason == DefaultReason ? GetString("Unknown") : reason;
            NotificationPopperPatch.NotificationPop(
                string.Format(GetString(CheatDetected.UseCheat),
                    pc.GetColoredName(), reason));
            return true;
        }

        reason = reason == DefaultReason ? GetString("Unknown") : reason;
        NotificationPopperPatch.NotificationPop(
            string.Format(GetString(CheatDetected.MayUseCheat),
                pc.GetColoredName(), reason));
        return false;
    }

    #endregion
}
public enum CheatDetected
{
    SetName,
    SetName_NotHost,
    Overload,
    Overload_NotHost,
    Cheater,
    Cheater_NotHost,
    SendQuickChat,
    SendQuickChat_NotHost,
    InvalidSlothRPC,
    InvalidSlothRPC_NotHost,
    UseCheat,
    MayUseCheat,
    HighLevel,
    LowLevel,
}