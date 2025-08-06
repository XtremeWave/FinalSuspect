using System.Text;
using AmongUs.Data;
using FinalSuspect.DataHandling.FinalGameData;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Templates;
using TMPro;
using UnityEngine;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
internal class AmongUsClientEndGamePatch
{
    public static Dictionary<byte, string> SummaryText = new();

    public static void Postfix()
    {
        FinalGameData.LastLocalPlayerRoleColor = PlayerControl.LocalPlayer.GetRoleColor();
        SummaryText = new Dictionary<byte, string>();
        foreach (var data in FinalPlayerData.AllPlayerData)
            SummaryText[data.PlayerId] = SummaryTexts(data.PlayerId);
    }
}

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
internal class SetEverythingUpPatch
{
    private static TextMeshPro roleSummary;
    private static SimpleButton showHideButton;
    private static bool DidHumansWin;

    public static void Prefix()
    {
        DidHumansWin = GameManager.Instance.DidHumansWin(EndGameResult.CachedGameOverReason);
    }

    public static void Postfix(EndGameManager __instance)
    {
        var showInitially = Main.ShowResults.Value;

        var WinnerTextObject = Object.Instantiate(__instance.WinText.gameObject);
        WinnerTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x,
            __instance.WinText.transform.position.y - 0.5f, __instance.WinText.transform.position.z);
        WinnerTextObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        var winnerText = WinnerTextObject.GetComponent<TextMeshPro>();
        winnerText.fontSizeMin = 3f;

        var winnerColor = DidHumansWin ? "#8CFFFF" : "#FF1919";
        __instance.BackgroundBar.material.color = __instance.WinText.color =
            winnerText.color = DidHumansWin ? Palette.CrewmateBlue : Palette.ImpostorRed;
        __instance.WinText.text = DidHumansWin ? GetString("Outro.Crews_Win") : GetString("Outro.Imps_Win");
        winnerText.text = DidHumansWin ? GetString("Outro.Crews_WinBlurb") : GetString("Outro.Imps_WinBlurb");

        __instance.WinText.gameObject.SetActive(!showInitially);
        WinnerTextObject.SetActive(!showInitially);

        showHideButton =
            new SimpleButton(
                __instance.transform,
                "ShowHideResultsButton",
                new Vector3(-4.5f * GetResolutionOffset(), 2.6f, -14f), // 比 BackgroundLayer(z = -13) 更靠前
                FinalGameData.LastLocalPlayerRoleColor,
                FinalGameData.LastLocalPlayerRoleColor.ShadeColor(0.1f),
                () =>
                {
                    var setToActive = !roleSummary.gameObject.activeSelf;
                    roleSummary.gameObject.SetActive(setToActive);
                    Main.ShowResults.Value = setToActive;
                    __instance.WinText.gameObject.SetActive(!setToActive);
                    WinnerTextObject.SetActive(!setToActive);
                    showHideButton.Label.text = GetString(setToActive ? "Summary.HideResults" : "Summary.ShowResults");
                },
                GetString(showInitially ? "Summary.HideResults" : "Summary.ShowResults"))
            {
                Scale = new Vector2(1.5f, 0.5f),
                FontSize = 2f
            };
        var lastGameResult = DidHumansWin ? GetString("Summary.CrewsWin") : GetString("Summary.ImpsWin");
        FinalGameData.LastGameResult = lastGameResult;
        StringBuilder sb = new($"{GetString("Summary.Text")}{lastGameResult}");
        var gameCode = StringHelper.ColorString(
            ColorHelper.FSColor,
            DataManager.Settings.Gameplay.StreamerMode
                ? new string('*', FinalGameData.LastRoomCode.Length)
                : FinalGameData.LastRoomCode);
        sb.Append("\n" + FinalGameData.LastServer + "  " + gameCode);
        sb.Append("\n" + GetString("Tip.HideSummaryTextToShowWinText"));

        StringBuilder sb2 = new();
        foreach (var data in FinalPlayerData.AllPlayerData.Where(x => x.IsImpostor != DidHumansWin))
            sb2.Append($"\n<color={winnerColor}>★</color> ")
                .Append(AmongUsClientEndGamePatch.SummaryText[data.PlayerId]);

        foreach (var data in FinalPlayerData.AllPlayerData.Where(x => x.IsImpostor == DidHumansWin))
            sb2.Append("\n\u3000 ").Append(AmongUsClientEndGamePatch.SummaryText[data.PlayerId]);

        FinalGameData.LastGameData = sb2.ToString();
        sb.Append(sb2);
        HudManagerPatch.Init();
        roleSummary = TMPTemplate.Create(
            "RoleSummaryText",
            sb.ToString(),
            Color.white,
            1.25f,
            TextAlignmentOptions.TopLeft,
            showInitially,
            showHideButton.Button.transform);
        roleSummary.transform.localPosition = new Vector3(1.7f, -0.4f, -1f);
        roleSummary.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        roleSummary.fontStyle = FontStyles.Bold;
        roleSummary.SetOutlineColor(Color.black);
        roleSummary.SetOutlineThickness(0.15f);

        FinalPlayerData.DisposeAll();
    }
}