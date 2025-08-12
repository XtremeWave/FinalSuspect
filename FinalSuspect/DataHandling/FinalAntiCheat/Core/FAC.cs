using System;
using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Patches.Game_Vanilla;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Core;

public static class FAC
{
    private static int _deNum;
    public static long LastHandleCheater = -1;
    public static readonly List<RpcHandlers> Handlers = [];

    static FAC()
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
                     .Where(t => typeof(IRpcHandler).IsAssignableFrom(t) && !t.IsAbstract))
        {
            var handler = (IRpcHandler)Activator.CreateInstance(type);
            if (handler == null) continue;
            var rpcTypes = handler.TargetRpcs;

            var activehandler = new RpcHandlers(rpcTypes);
            activehandler.Handlers.Add(handler);

            Handlers.Add(activehandler);
        }
    }

    public static void Init_FAC()
    {
        _deNum = 0;
    }

    public static void WarnHost(int denum = 1)
    {
        _deNum += denum;
        if (!ErrorText.Instance) return;
        ErrorText.Instance.CheatDetected = _deNum > 3;
        ErrorText.Instance.SBDetected = _deNum > 10;
        if (ErrorText.Instance.CheatDetected)
            ErrorText.Instance.AddError(ErrorText.Instance.SBDetected ? ErrorCode.SBDetected : ErrorCode.CheatDetected);
        else
            ErrorText.Instance.Clear();
    }

    public static bool ReceiveRpc(PlayerControl pc, byte callId, MessageReader reader, out bool notify,
        out string reason, out bool ban)
    {
        notify = true;
        reason = "Hacking";
        ban = false;

        if (!Main.EnableFAC.Value || !pc || reader == null || pc.AmOwner)
            return false;

        try
        {
            if (pc.GetCheatData().HandleIncomingRpc(callId))
            {
                NotificationPopperPatch.NotificationPop(AmongUsClient.Instance.AmHost
                    ? string.Format(GetString("CheatDetected.Overload"), pc.GetColoredName())
                    : string.Format(GetString("CheatDetected.Overload_NotHost"), pc.GetColoredName()));
                ban = true;
                notify = false;
                return true;
            }

            var sr = MessageReader.Get(reader);

            foreach (var handler in Handlers.Where(handlers => handlers.TargetRpcs.Contains(callId))
                         .SelectMany(handlers => handlers.Handlers))
            {
                if (!Enum.IsDefined(typeof(RpcCalls), callId))
                {
                    notify = false;
                    if (handler.HandleInvalidRPC(pc, sr, ref notify, ref reason, ref ban))
                    {
                        ban = true;
                        if (reason == "Hacking")
                            reason = GetString("Unknown");
                        NotificationPopperPatch.NotificationPop(
                            string.Format(GetString("CheatDetected.UseCheat"),
                                pc.GetColoredName(),
                                reason));
                        return true;
                    }

                    if (reason == "Hacking")
                        reason = GetString("Unknown");
                    NotificationPopperPatch.NotificationPop(
                        string.Format(GetString("CheatDetected.MayUseCheat"),
                            pc.GetColoredName(),
                            reason));
                    return false;
                }

                if (handler.HandleAll(pc, sr, ref notify, ref reason, ref ban))
                    return true;

                if (IsLobby && handler.HandleLobby(pc, sr, ref notify, ref reason, ref ban))
                {
                    if (AmongUsClient.Instance.AmHost) return true;
                    NotificationPopperPatch.NotificationPop(GetString("Warning.RoomBroken"));
                    notify = false;
                    return true;
                }

                return (IsInGame && handler.HandleGame_All(pc, sr, ref notify, ref reason, ref ban))
                       || (IsInTask && handler.HandleGame_InTask(pc, sr, ref notify, ref reason, ref ban))
                       || (IsInMeeting && handler.HandleGame_InMeeting(pc, sr, ref notify, ref reason, ref ban))
                       || (IsHideNSeek && handler.HandleHideNSeek(pc, sr, ref notify, ref reason, ref ban));
            }
        }
        catch (Exception e)
        {
            Fatal(e.ToString(), "FAC");
        }

        WarnHost(-1);
        return false;
    }

    public static void HandleCheat(PlayerControl pc, string text)
    {
        NotificationPopperPatch.NotificationPop(string.Format(text, pc.GetColoredName()));
    }

    public static void Dispose(byte id)
    {
        foreach (var handler in Handlers) handler.Handlers.Do(x => x.Dispose(id));
    }
}