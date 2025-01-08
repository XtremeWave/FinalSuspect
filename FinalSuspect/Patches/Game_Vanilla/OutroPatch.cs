using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.Data;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Patches.System;
using FinalSuspect.Templates;
using InnerNet;
using TMPro;
using UnityEngine;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
class AmongUsClientEndGamePatch
{
    public static Dictionary<byte, string> SummaryText = new();
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        SummaryText = new();
        foreach (var id in XtremePlayerData.AllPlayerData.Keys)
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
        var WinnerTextObject = Object.Instantiate(__instance.WinText.gameObject);
        WinnerTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.5f, __instance.WinText.transform.position.z);
        WinnerTextObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        var WinnerText = WinnerTextObject.GetComponent<TextMeshPro>(); // WinTextと同じ型のコンポーネントを取得
        WinnerText.fontSizeMin = 3f;

        string CustomWinnerColor = DidHumansWin ? "#8CFFFF" : "#FF1919";
        __instance.BackgroundBar.material.color = __instance.WinText.color = WinnerText.color = DidHumansWin ? Palette.CrewmateBlue : Palette.ImpostorRed;
        __instance.WinText.text = DidHumansWin ? GetString("CrewmatesWin") : GetString("ImpostorsWin");
        WinnerText.text = DidHumansWin ? GetString("CrewmatesWinBlurb") : GetString("ImpostorsWinBlurb");

        __instance.WinText.gameObject.SetActive(!showInitially);
        WinnerTextObject.SetActive(!showInitially);

        //ShowResult:
        showHideButton = 
        new SimpleButton(
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
               WinnerTextObject.SetActive(!setToActive);
               showHideButton.Label.text = GetString(setToActive ? "HideResults" : "ShowResults");
           },
           GetString(showInitially ? "HideResults" : "ShowResults"))
        {
            Scale = new(1.5f, 0.5f),
            FontSize = 2f,
        };

        StringBuilder sb = new($"{GetString("RoleSummaryText")}");
        sb.Append(DidHumansWin ? GetString("CrewsWin") : GetString("ImpsWin"));
        var gamecode =  StringHelper.ColorString(
            ColorHelper.ModColor32, 
            !DataManager.Settings.Gameplay.StreamerMode? GameCode.IntToGameName(AmongUsClient.Instance.GameId) : new string('*', GameCode.IntToGameName(AmongUsClient.Instance.GameId).Length));
        sb.Append("\n"+ (XtremeGameData.GameStates.IsOnlineGame ? PingTrackerUpdatePatch.ServerName : GetString("Local")) +"  "+gamecode);
        sb.Append("\n" + GetString("HideSummaryTextToShowWinText"));

        foreach (var kvp in XtremePlayerData.AllPlayerData.Where(x => x.Value.IsImpostor != DidHumansWin))
        {
            var id = kvp.Key;
            var data = kvp.Value;
            sb.Append($"\n<color={CustomWinnerColor}>★</color> ").Append(AmongUsClientEndGamePatch.SummaryText[id]);
        }
        foreach (var kvp in XtremePlayerData.AllPlayerData.Where(x => x.Value.IsImpostor == DidHumansWin))
        {
            var id = kvp.Key;
            sb.Append("\n\u3000 ").Append(AmongUsClientEndGamePatch.SummaryText[id]);
        }

        HudManagerPatch.LastResultText = sb.ToString().Replace("\n" + GetString("HideSummaryTextToShowWinText"), "");
        HudManagerPatch.Init();
        roleSummary = TMPTemplate.Create(
            "RoleSummaryText",
            sb.ToString(),
            Color.white,
            1.25f,
            TextAlignmentOptions.TopLeft,
            setActive: showInitially,
            parent: showHideButton.Button.transform);
        roleSummary.transform.localPosition = new Vector3(1.7f, -0.4f, -1f);
        roleSummary.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        roleSummary.fontStyle = FontStyles.Bold;
        roleSummary.SetOutlineColor(Color.black);
        roleSummary.SetOutlineThickness(0.15f);
 

        XtremePlayerData.AllPlayerData.Values.ToArray().Do(data => data.Dispose());


    }
    
}