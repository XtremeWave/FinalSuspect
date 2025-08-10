using System;
using FinalSuspect.Modules.Features;
using UnityEngine;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
internal class ControllerManagerUpdatePatch
{
    private static readonly (int, int)[] resolutions =
        [(480, 270), (640, 360), (800, 450), (1280, 720), (1600, 900), (1920, 1080)];

    private static int _resolutionIndex;
    public static bool ShowSettingsPanel = true;
    public static bool ShowHudUI = true;

    public static void Postfix()
    {
        //职业介绍
        if (IsInGame && (IsCanMove || IsInMeeting) && Input.GetKey(KeyCode.F1))
        {
            if (!InGameRoleInfoMenu.Showing)
                InGameRoleInfoMenu.SetRoleInfoRef(PlayerControl.LocalPlayer);
            InGameRoleInfoMenu.Show();
        }
        else
        {
            InGameRoleInfoMenu.Hide();
        }

        if (Input.GetKeyDown(KeyCode.F2) && IsInGame)
            ShowSettingsPanel = !ShowSettingsPanel;

        //更改分辨率
        if (Input.GetKeyDown(KeyCode.F11))
        {
            _resolutionIndex++;
            if (_resolutionIndex >= resolutions.Length) _resolutionIndex = 0;
            ResolutionManager.SetResolution(resolutions[_resolutionIndex].Item1, resolutions[_resolutionIndex].Item2,
                Screen.fullScreen);
        }

        //更改是否全屏
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ResolutionManager.SetResolution(resolutions[_resolutionIndex].Item1, resolutions[_resolutionIndex].Item2,
                !Screen.fullScreen);
        }

        //重新加载自定义翻译
        if (GetKeysDown(KeyCode.F5, KeyCode.T))
        {
            Info("加载自定义翻译文件", "KeyCommand");
            LoadLangs();
            SendInGame("Reloaded Custom Translation File");
        }

        if (GetKeysDown(KeyCode.F5, KeyCode.X))
        {
            Info("导出自定义翻译文件", "KeyCommand");
            ExportCustomTranslation();
            SendInGame("Exported Custom Translation File");
        }

        //日志文件转储
        if (GetKeysDown(KeyCode.F1, KeyCode.LeftControl))
        {
            Info("输出日志", "KeyCommand");
            DumpLog();
        }

        if (GetKeysDown(KeyCode.F1, KeyCode.RightControl))
        {
            Info("输出日志", "KeyCommand");
            DumpLog();
        }

        //打开游戏目录
        if (GetKeysDown(KeyCode.F10)) OpenDirectory(Environment.CurrentDirectory);

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (IsNotJoined)
            {
                VersionShowerStartPatch.ModLogo.SetActive(ModMainMenuManager.Active);
                VersionShowerStartPatch.AuthorLogo.SetActive(ModMainMenuManager.Active);
                ModMainMenuManager.Active = !ModMainMenuManager.Active;
                ModMainMenuManager.Instance.mainMenuUI.SetActive(ModMainMenuManager.Active);
                VersionShowerStartPatch.CreditTextCredential.gameObject.SetActive(ModMainMenuManager.Active);
                VersionShowerStartPatch.VisitText.gameObject.SetActive(ModMainMenuManager.Active);
                DestroyableSingleton<AccountTab>.Instance.gameObject.SetActive(ModMainMenuManager.Active);
                ModMainMenuManager.ModStamp.SetActive(ModMainMenuManager.Active);
            }
        }

        //-- 下面是主机专用的命令--//
        if (!AmongUsClient.Instance.AmHost) return;

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && IsCountDown)
        {
            Info("倒计时修改为0", "KeyCommand");
            GameStartManager.Instance.countDownTimer = 0;
            SoundManager.Instance.StopSound(GameStartManager.Instance.gameStartSound);
        }

        //倒计时取消
        if (Input.GetKeyDown(KeyCode.C) && IsCountDown)
        {
            Info("重置倒计时", "KeyCommand");
            GameStartManager.Instance.ResetStartState();
        }

        //切换日志是否也在游戏中输出
        if (GetKeysDown(KeyCode.F2, KeyCode.LeftControl))
        {
            isAlsoInGame = !isAlsoInGame;
            SendInGame($"游戏中输出日志：{isAlsoInGame}");
        }

        if (GetKeysDown(KeyCode.F2, KeyCode.RightControl))
        {
            isAlsoInGame = !isAlsoInGame;
            SendInGame($"游戏中输出日志：{isAlsoInGame}");
        }

        if (!DebugModeManager.IsDebugMode) return;

        //实名投票
        if (GetKeysDown(KeyCode.Return, KeyCode.V, KeyCode.LeftShift) && IsInMeeting && !IsOnlineGame)
            MeetingHud.Instance.RpcClearVote(AmongUsClient.Instance.ClientId);

        //打开飞艇所有的门
        if (GetKeysDown(KeyCode.Return, KeyCode.D, KeyCode.LeftShift) && IsInGame)
        {
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, 79);
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, 80);
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, 81);
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, 82);
        }

        //将击杀冷却设定为0秒
        if (GetKeysDown(KeyCode.Return, KeyCode.K, KeyCode.LeftShift) && IsInGame)
            PlayerControl.LocalPlayer.Data.Object.SetKillTimer(0f);

        //开场动画测试
        if (Input.GetKeyDown(KeyCode.G))
        {
            HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black));
            HudManager.Instance.StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoShowIntro());
        }

        //获取现在的坐标
        if (Input.GetKeyDown(KeyCode.I))
            Info(PlayerControl.LocalPlayer.GetTruePosition().ToString(), "GetLocalPlayerPos");
    }

    private static bool GetKeysDown(params KeyCode[] keys)
    {
        if (keys.Any(Input.GetKeyDown) && keys.All(Input.GetKey))
        {
            Info($"快捷键：{keys.Where(Input.GetKeyDown).First()} in [{string.Join(",", keys)}]", "GetKeysDown");
            return true;
        }

        return false;
    }

    // private static bool ORGetKeysDown(params KeyCode[] keys) => keys.Any(Input.GetKeyDown);
}