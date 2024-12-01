using HarmonyLib;
using Hazel;
using System.Linq;
using FinalSuspect.Modules;

using UnityEngine;
using static FinalSuspect.Translator;

namespace FinalSuspect;

[HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
internal class ControllerManagerUpdatePatch
{
    private static readonly (int, int)[] resolutions = { (480, 270), (640, 360), (800, 450), (1280, 720), (1600, 900), (1920, 1080) };
    private static int resolutionIndex = 0;
    private static bool FullScreen = true;
    public static void Postfix(ControllerManager __instance)
    {


        //切换自定义设置的页面

        //职业介绍
        if (XtremeGameData.GameStates.IsInGame && (XtremeGameData.GameStates.IsCanMove || XtremeGameData.GameStates.IsMeeting))
        {
            if (Input.GetKey(KeyCode.F1))
            {
                if (!InGameRoleInfoMenu.Showing)
                    InGameRoleInfoMenu.SetRoleInfoRef(PlayerControl.LocalPlayer);
                InGameRoleInfoMenu.Show();
            }
            else InGameRoleInfoMenu.Hide();
        }
        else InGameRoleInfoMenu.Hide();

        //更改分辨率
        if (Input.GetKeyDown(KeyCode.F11))
        {
            resolutionIndex++;
            if (resolutionIndex >= resolutions.Length) resolutionIndex = 0;
            ResolutionManager.SetResolution(resolutions[resolutionIndex].Item1, resolutions[resolutionIndex].Item2, false);
        }
        //重新加载自定义翻译
        if (GetKeysDown(KeyCode.F5, KeyCode.T))
        {
            Logger.Info("加载自定义翻译文件", "KeyCommand");
            Translator.LoadLangs();
            Logger.SendInGame("Reloaded Custom Translation File");
        }
        if (GetKeysDown(KeyCode.F5, KeyCode.X))
        {
            Logger.Info("导出自定义翻译文件", "KeyCommand");
            Translator.ExportCustomTranslation();
            Logger.SendInGame("Exported Custom Translation File");
        }
        //日志文件转储
        if (GetKeysDown(KeyCode.F1, KeyCode.LeftControl))
        {
            Logger.Info("输出日志", "KeyCommand");
            Utils.DumpLog();
        }
        if (GetKeysDown(KeyCode.F1, KeyCode.RightControl))
        {
            Logger.Info("输出日志", "KeyCommand");
            Utils.DumpLog();
        }
        //打开游戏目录
        if (GetKeysDown(KeyCode.F10))
        {
            Utils.OpenDirectory(System.Environment.CurrentDirectory);
        }

        //-- 下面是主机专用的命令--//
        if (!AmongUsClient.Instance.AmHost) return;

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && XtremeGameData.GameStates.IsCountDown)
        {
            Logger.Info("倒计时修改为0", "KeyCommand");
            GameStartManager.Instance.countDownTimer = 0;
        }


        //倒计时取消
        if (Input.GetKeyDown(KeyCode.C) && XtremeGameData.GameStates.IsCountDown)
        {
            Logger.Info("重置倒计时", "KeyCommand");
            GameStartManager.Instance.ResetStartState();
        }

        //切换日志是否也在游戏中输出
        if (GetKeysDown(KeyCode.F2, KeyCode.LeftControl))
        {
            Logger.isAlsoInGame = !Logger.isAlsoInGame;
            Logger.SendInGame($"游戏中输出日志：{Logger.isAlsoInGame}");
        }
        if (GetKeysDown(KeyCode.F2, KeyCode.RightControl))
        {
            Logger.isAlsoInGame = !Logger.isAlsoInGame;
            Logger.SendInGame($"游戏中输出日志：{Logger.isAlsoInGame}");
        }

    }

    private static bool GetKeysDown(params KeyCode[] keys)
    {
        if (keys.Any(Input.GetKeyDown) && keys.All(Input.GetKey))
        {
            Logger.Info($"快捷键：{keys.Where(Input.GetKeyDown).First()} in [{string.Join(",", keys)}]", "GetKeysDown");
            return true;
        }
        return false;
    }

    private static bool ORGetKeysDown(params KeyCode[] keys) => keys.Any(k => Input.GetKeyDown(k));
}
