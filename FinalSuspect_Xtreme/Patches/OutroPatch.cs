using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using FinalSuspect_Xtreme.Modules;

using FinalSuspect_Xtreme.Patches;
using FinalSuspect_Xtreme.Templates;
using UnityEngine;
using static FinalSuspect_Xtreme.Translator;
using static Il2CppSystem.Globalization.CultureInfo;

namespace FinalSuspect_Xtreme;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
class EndGamePatch
{
    public static Dictionary<byte, string> SummaryText = new();
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        SummaryText = new();
        foreach (var id in GamePlayerData.AllGamePlayerData.Keys)
            SummaryText[id] = Utils.SummaryTexts(id);
    }
}
[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
class SetEverythingUpPatch
{
    private static TextMeshPro roleSummary;
    private static SimpleButton showHideButton;
    static bool DidHumansWin;
    public static void Prefix()
    {
        DidHumansWin = GameManager.Instance.DidHumansWin(EndGameResult.CachedGameOverReason);
    }
    public static void Postfix(EndGameManager __instance)
    {

        var showInitially = Main.ShowResults.Value;

        //#######################################
        //          ==勝利陣営表示==
        //#######################################
        Logger.Info("胜利阵营显示", "SetEverythingUpPatch");
        string CustomWinnerColor = "#ffffff";
        __instance.WinText.gameObject.SetActive(!showInitially);

        //ShowResult:
        Logger.Info("最终结果显示", "SetEverythingUpPatch");
        showHideButton = new SimpleButton(
           __instance.transform,
           "ShowHideResultsButton",
           new(-4.5f, 2.6f, -14f),  // 比 BackgroundLayer(z = -13) 更靠前
           new(209, 190, 0, byte.MaxValue),
           new(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue),
           () =>
           {
               var setToActive = !roleSummary.gameObject.activeSelf;
               roleSummary.gameObject.SetActive(setToActive);
               Main.ShowResults.Value = setToActive;
               __instance.WinText.gameObject.SetActive(!setToActive);
               showHideButton.Label.text = GetString(setToActive ? "HideResults" : "ShowResults");
           },
           GetString(showInitially ? "HideResults" : "ShowResults"))
        {
            Scale = new(1.5f, 0.5f),
            FontSize = 2f,
        };

        StringBuilder sb = new($"{GetString("RoleSummaryText")}");
        sb.Append(DidHumansWin ? GetString("CrewsWin") : GetString("ImpsWin"));



        CustomWinnerColor = !DidHumansWin ? "#FF1919" : "#8CFFFF";



        foreach (var kvp in GamePlayerData.AllGamePlayerData.Where(x => x.Value.IsImpostor != DidHumansWin))
        {
            var id = kvp.Key;
            var data = kvp.Value;
            sb.Append($"\n<color={CustomWinnerColor}>★</color> ").Append(EndGamePatch.SummaryText[id]);
        }
        foreach (var kvp in GamePlayerData.AllGamePlayerData.Where(x => x.Value.IsImpostor == DidHumansWin))
        {
            var id = kvp.Key;
            var data = kvp.Value;
            sb.Append($"\n　 ").Append(EndGamePatch.SummaryText[id]);

        }
        Logger.Info("判断胜利结束", "SetEverythingUpPatch");


        roleSummary = TMPTemplate.Create(
                "RoleSummaryText",
                sb.ToString(),
                Color.white,
                1.25f,
                TextAlignmentOptions.TopLeft,
                setActive: showInitially,
                parent: showHideButton.Button.transform);
        roleSummary.transform.localPosition = new(1.7f, -0.4f, -1f);
        roleSummary.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////Utils.ApplySuffix();
    }
}
