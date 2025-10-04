using System;
using System.Text;
using AmongUs.Data;
using AmongUs.GameOptions;
using FinalSuspect.Attributes;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Patches.System;
using FinalSuspect.Templates;
using InnerNet;
using TMPro;
using Object = UnityEngine.Object;

namespace FinalSuspect.Modules.Core.Game.UI;

public static class LastResult
{
    public static TextMeshPro roleSummary;
    public static SimpleButton LastResultButton;
    public static bool DidHumansWin;
    private static SpriteRenderer backgroundRenderer;

    public static string LastResultText;
    public static string LastRoomCode;
    public static string LastServer;
    public static string LastGameData;
    public static string LastGameResult;
    public static Color LastLocalPlayerRoleColor;


    public static Dictionary<byte, string> SummaryText = new();

    [GameModuleInitializer]
    public static void OnInitialization()
    {
        DestroyAll();
        LastResultText = LastGameData = LastGameResult = LastRoomCode = LastServer = "";
    }

    public static void DestroyAll()
    {
        try
        {
            Object.Destroy(LastResultButton.Button.gameObject);
            Object.Destroy(roleSummary.gameObject);
            Object.Destroy(backgroundRenderer.gameObject);
        }
        catch
        {
            /* ignored */
        }

        LastResultButton = null;
        roleSummary = null;
        backgroundRenderer = null;
    }

    public static void UpdateResult(HudManager __instance)
    {
        if (IsFreePlay || (!IsInGame && GetLineCount(LastResultText) < 6))
            return;
        var showInitially = ConfigManager.ShowResults.Value;

        LastResultButton ??=
            new SimpleButton(
                __instance.transform,
                "Show Hide Last Result Button",
                IsInGame
                    ? new Vector3(0.2f * GetResolutionOffset(), 2.685f, -14f)
                    : new Vector3(-4.5f * GetResolutionOffset(), 2.6f, -14f), // 比 BackgroundLayer(z = -13) 更靠前
                new Color32(209, 190, 255, byte.MaxValue),
                new Color32(208, 222, 255, byte.MaxValue),
                () =>
                {
                    var setToActive = !roleSummary.gameObject.activeSelf;
                    roleSummary.gameObject.SetActive(setToActive);
                    ConfigManager.ShowResults.Value = setToActive;
                    LastResultButton.Label.text =
                        GetString(setToActive ? "Summary.HideResults" : "Summary.ShowResults");
                },
                GetString(showInitially ? "Summary.HideResults" : "Summary.ShowResults"))
            {
                Scale = new Vector2(1.5f, 0.5f),
                FontSize = 2f
            };

        LastResultButton.Button.gameObject.SetActive(true);

        StringBuilder sb = new($"{GetString("Summary.Text")}{LastGameResult}");
        if (IsInGame)
        {
            LastRoomCode = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            LastServer = IsOnlineGame
                ? PingTrackerUpdatePatch.ServerName
                : GetString("Local");
        }

        var gamecode = StringHelper.ColorString(
            ColorHelper.FSColor,
            DataManager.Settings.Gameplay.StreamerMode
                ? new string('*', LastRoomCode.Length)
                : LastRoomCode);
        sb.Append("\n" + LastServer + "  " + gamecode);
        if (IsInGame)
        {
            StringBuilder sb2 = new();
            foreach (var data in FinalPlayerData.AllPlayerData)
                sb2.Append("\n\u3000 ").Append(SummaryTexts(data.PlayerId));

            LastGameData = sb2.ToString();
        }

        sb.Append(LastGameData);
        LastResultText = sb.ToString();
        if (!roleSummary)
        {
            roleSummary = TMPTemplate.Create(
                "RoleSummaryText", LastResultText,
                Color.white,
                1.25f,
                TextAlignmentOptions.TopLeft,
                showInitially,
                LastResultButton.Button.transform);
            roleSummary.transform.localPosition =
                new Vector3(IsInGame ? 0f : 1.7f, -0.4f, -1f);
            roleSummary.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            roleSummary.fontStyle = FontStyles.Bold;
            roleSummary.SetOutlineColor(Color.black);
            roleSummary.SetOutlineThickness(0.15f);

            var backgroundObject = new GameObject("RoleSummaryBackground");
            backgroundObject.transform.SetParent(roleSummary.transform);
            backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = LoadSprite("LastResult-BG.png", 200f);
            backgroundRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        LastResultButton.Button.transform.localPosition =
            IsInGame ? new Vector3(0.2f, 2.685f, -14f) : new Vector3(-4.5f, 2.6f, -1f);
        if (IsInGame)
        {
            LastResultButton.Button.gameObject.SetActive
            (PlayerControl.LocalPlayer.GetRoleType() is RoleTypes.CrewmateGhost or RoleTypes.ImpostorGhost &&
             !IsInMeeting);
        }
        else
            LastResultButton.Button.gameObject.SetActive(true);

        roleSummary.text = LastResultText;
        AdjustBackgroundSize();
    }

    private static void AdjustBackgroundSize()
    {
        if (!roleSummary || !backgroundRenderer) return;
        var textBounds = roleSummary.textBounds;

        var backgroundSprite = backgroundRenderer.sprite;
        if (!backgroundSprite) return;
        var scaleX = (textBounds.size.x + 0.4f) / backgroundSprite.bounds.size.x;
        var scaleY = (textBounds.size.y + 0.5f) / backgroundSprite.bounds.size.y;

        backgroundRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        backgroundRenderer.transform.localPosition = new Vector3(textBounds.center.x, textBounds.center.y, 2f);
    }

    private static int GetLineCount(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;
        var lines = text.Split(["\r\n", "\n"], StringSplitOptions.None);
        return lines.Length;
    }
}