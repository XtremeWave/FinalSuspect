using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using FinalSuspect_Xtreme.Modules;
using UnityEngine;
using static FinalSuspect_Xtreme.Translator;
using Object = UnityEngine.Object;
using FinalSuspect_Xtreme.Patches;
using FinalSuspect_Xtreme.Modules.Managers.ResourcesManager;


namespace FinalSuspect_Xtreme;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class GameStartManagerUpdatePatch
{
    public static void Prefix(GameStartManager __instance)
    {
        __instance.MinPlayers = 1;

        if (CreateOptionsPickerPatch.SetDleks)
        {
            if (XtremeGameData.GameStates.IsNormalGame)
                Main.NormalOptions.MapId = 3;

            else if (XtremeGameData.GameStates.IsHideNSeek)
                Main.HideNSeekOptions.MapId = 3;
        }
    }
}
public class GameStartManagerPatch
{
    private static float timer = 600f;
    private static Vector3 GameStartTextlocalPosition;
    private static TextMeshPro timerText;
    private static PassiveButton cancelButton;
    private static TextMeshPro warningText;
    public static TextMeshPro HideName;
    public static TextMeshPro GameCountdown;

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public class GameStartManagerStartPatch
    {
        public static void Postfix(GameStartManager __instance)
        {
            __instance.MinPlayers = 1;

            __instance.GameRoomNameCode.text = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            timer = 600f;

            HideName = Object.Instantiate(__instance.GameRoomNameCode, __instance.GameRoomNameCode.transform);
            HideName.gameObject.SetActive(true);
            HideName.name = "HideName";
            HideName.color =
                ColorUtility.TryParseHtmlString(Main.HideColor.Value, out var color) ? color :
                ColorUtility.TryParseHtmlString(ColorHelper.ModColor, out var modColor) ? modColor : HideName.color;
            HideName.text = Main.HideName.Value;
            Logger.Info("HideName instantiated and configured", "test");

            warningText = Object.Instantiate(__instance.GameStartText, __instance.transform);
            warningText.name = "WarningText";
            warningText.transform.localPosition = new(0f, 0f - __instance.transform.localPosition.y, -1f);
            warningText.gameObject.SetActive(false);
            Logger.Info("WarningText instantiated and configured", "test");

            if (AmongUsClient.Instance.AmHost)
            {
                timerText = Object.Instantiate(__instance.PlayerCounter, __instance.StartButton.transform.parent);
            }
            else
            {
                timerText = Object.Instantiate(__instance.PlayerCounter, __instance.StartButtonClient.transform.parent);
            }
            timerText.fontSize = 6.2f;
            timerText.autoSizeTextContainer = true;
            timerText.name = "Timer";
            timerText.DestroyChildren();
            timerText.DestroySubMeshObjects();
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.outlineColor = Color.black;
            timerText.outlineWidth = 0.40f;
            timerText.hideFlags = HideFlags.None;
            //timerText.transform.localPosition += new Vector3(-0.5f, -2.6f, 0f);
            timerText.transform.localPosition += new Vector3(-0.55f,  -0.4f, 0f);
            timerText.transform.localScale = new(0.7f, 0.7f, 1f);
            timerText.gameObject.SetActive(AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame && XtremeGameData.GameStates.IsVanillaServer);

            cancelButton = Object.Instantiate(__instance.StartButton, __instance.transform);
            var cancelLabel = cancelButton.GetComponentInChildren<TextMeshPro>();
            cancelLabel.DestroyTranslator();
            cancelLabel.text = GetString("Cancel");
            var cancelButtonInactiveRenderer = cancelButton.inactiveSprites.GetComponent<SpriteRenderer>();
            cancelButtonInactiveRenderer.color = new(0.8f, 0f, 0f, 1f);
            var cancelButtonActiveRenderer = cancelButton.activeSprites.GetComponent<SpriteRenderer>();
            cancelButtonActiveRenderer.color = Color.red;
            var cancelButtonInactiveShine = cancelButton.inactiveSprites.transform.Find("Shine");
            if (cancelButtonInactiveShine)
            {
                cancelButtonInactiveShine.gameObject.SetActive(false);
            }
            cancelButton.activeTextColor = cancelButton.inactiveTextColor = Color.white;
            //cancelButton.transform.localPosition = new(2f, 0.13f, 0f);
            GameStartTextlocalPosition = __instance.GameStartText.transform.localPosition;
            cancelButton.OnClick = new();
            cancelButton.OnClick.AddListener((Action)(() =>
            {
                __instance.ResetStartState();
            }));
            cancelButton.gameObject.SetActive(false);

            if (AmongUsClient.Instance.AmHost && (VersionChecker.isBroken || (VersionChecker.hasUpdate && VersionChecker.forceUpdate)  || !VersionChecker.IsSupported))
            {
                __instance.HostPrivateButton.inactiveTextColor = Palette.DisabledClear;
                __instance.HostPrivateButton.activeTextColor = Palette.DisabledClear;
            }


        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public class GameStartManagerUpdatePatch
    {
        private static int updateTimer = 0;
        public static float exitTimer = -1f;
        public static void Prefix(GameStartManager __instance)
        {
            // Lobby code
            if (DataManager.Settings.Gameplay.StreamerMode)
            {
                __instance.GameRoomNameCode.color = new(__instance.GameRoomNameCode.color.r, __instance.GameRoomNameCode.color.g, __instance.GameRoomNameCode.color.b, 0);
                HideName.enabled = false;
            }
            else
            {
                __instance.GameRoomNameCode.color = new(__instance.GameRoomNameCode.color.r, __instance.GameRoomNameCode.color.g, __instance.GameRoomNameCode.color.b, 255);
                HideName.enabled = false;
            }

            if (Main.AutoStartGame.Value)
            {
                updateTimer++;
                if (updateTimer >= 50)
                {
                    updateTimer = 0;
                    var maxPlayers = GameManager.Instance.LogicOptions.MaxPlayers;
                    if (GameData.Instance.PlayerCount >= maxPlayers - 1 && !XtremeGameData.GameStates.IsCountDown)
                    {
                        GameStartManager.Instance.startState = GameStartManager.StartingStates.Countdown;
                        GameStartManager.Instance.countDownTimer = 10;

                    }
                }
            }
        }
        public static void Postfix(GameStartManager __instance)
        {
            if (!AmongUsClient.Instance) return;
            string warningMessage = "";
            if (AmongUsClient.Instance.AmHost)
            {
                bool canStartGame = true;
                List<string> mismatchedPlayerNameList = new();
                foreach (var client in AmongUsClient.Instance.allClients.ToArray())
                {
                    if (client.Character == null) continue;
                    var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                    if (dummyComponent != null && dummyComponent.enabled)
                        continue;
                    if (!MatchVersions(client.Character.PlayerId, true))
                    {
                        canStartGame = false;
                        mismatchedPlayerNameList.Add(Utils.ColorString(Palette.PlayerColors[client.ColorId], client.Character.Data.PlayerName));
                    }
                }
                if (!canStartGame)
                {
                    __instance.StartButton.gameObject.SetActive(false);
                    warningMessage = Utils.ColorString(Color.red, string.Format(GetString("Warning.MismatchedVersion"), string.Join(" ", mismatchedPlayerNameList), $"<color={ColorHelper.ModColor}>{Main.ModName}</color>"));
                }
                cancelButton.gameObject.SetActive(__instance.startState == GameStartManager.StartingStates.Countdown);
                __instance.StartButton.gameObject.SetActive(!cancelButton.gameObject.active);
            }
            else
            {
                if (MatchVersions(0, true) || Main.VersionCheat.Value)
                    exitTimer = 0;
                else
                {
                    exitTimer += Time.deltaTime;
                    if (exitTimer >= 5)
                    {
                        exitTimer = 0;
                        AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
                        SceneChanger.ChangeScene("MainMenu");
                    }
                    if (exitTimer != 0)
                        warningMessage = Utils.ColorString(Color.red, string.Format(GetString("Warning.AutoExitAtMismatchedVersion"), $"<color={ColorHelper.ModColor}>{Main.ModName}</color>", Math.Round(5 - exitTimer).ToString()));
                }
            }
            if (warningMessage == "")
            {
                warningText.gameObject.SetActive(false);
            }
            else
            {
                warningText.text = warningMessage;
                warningText.gameObject.SetActive(true);
            }
            if (AmongUsClient.Instance.AmHost)
            {
                __instance.GameStartText.transform.localPosition = new Vector3(__instance.GameStartText.transform.localPosition.x, 2f, __instance.GameStartText.transform.localPosition.z);
            }
            else
            {
                __instance.GameStartText.transform.localPosition = GameStartTextlocalPosition;
            }
            timerText.text = "";
            // Lobby timer
            if (!GameData.Instance || AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame || !XtremeGameData.GameStates.IsVanillaServer || !AmongUsClient.Instance.AmHost) return;

            timer = Mathf.Max(0f, timer -= Time.deltaTime);
            int minutes = (int)timer / 60;
            int seconds = (int)timer % 60;
            string countDown = $"{minutes:00}:{seconds:00}";
            if (timer <= 60) countDown = Utils.ColorString(Color.red, countDown);
            timerText.text = countDown;
        }
        private static bool MatchVersions(byte playerId, bool acceptVanilla = false)
        {
            if (!XtremeGameData.PlayerVersion.playerVersion.TryGetValue(playerId, out var version)) return acceptVanilla;
            return Main.ForkId == version.forkId
                && Main.version.CompareTo(version.version) == 0
                && version.tag == $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})";
        }
    }
}

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
public static class HiddenTextPatch
{
    private static void Postfix(TextBoxTMP __instance)
    {
        if (__instance.name == "GameIdText") __instance.outputText.text = new string('*', __instance.text.Length);
    }
}
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.ResetStartState))]
class ResetStartStatePatch
{
    public static void Prefix(GameStartManager __instance)
    {
        if (XtremeGameData.GameStates.IsCountDown)
        {
            SoundManager.Instance.StopSound(__instance.gameStartSound);


        }
    }
}