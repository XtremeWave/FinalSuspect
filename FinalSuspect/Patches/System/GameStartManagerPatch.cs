using System;
using AmongUs.Data;
using FinalSuspect.Attributes;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Resources;
using InnerNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class GameStartManagerUpdatePatch
{
    public static void Prefix(GameStartManager __instance)
    {
        __instance.MinPlayers = 1;

        /*Scrapped
        if (CreateOptionsPickerPatch.SetDleks && AmongUsClient.Instance.AmHost)
        {
            if (IsNormalGame)
                Main.NormalOptions.MapId = 3;

            else if (IsHideNSeek)
                Main.HideNSeekOptions.MapId = 3;
        }*/
    }
}

public static class GameStartManagerPatch
{
    private static float timer = 600f;
    private static Vector3 GameStartTextlocalPosition;
    private static TextMeshPro timerText;
    private static PassiveButton cancelButton;
    private static TextMeshPro warningText;
    private static TextMeshPro HideName;
    public static GameStartManager Instance;

    [GameModuleInitializer]
    public static void Init()
    {
        DestroyableSingleton<GameStartManager>.Instance.transform.gameObject.ForEachChild(
            (Action<GameObject>)HideAllBtns);
    }

    private static void HideAllBtns(GameObject obj)
    {
        obj.SetActive(false);
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public class GameStartManagerStartPatch
    {
        public static void Postfix(GameStartManager __instance)
        {
            Instance = __instance;
            __instance.MinPlayers = 1;

            __instance.GameRoomNameCode.text = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            timer = 600f;

            HideName = Object.Instantiate(__instance.GameRoomNameCode, __instance.GameRoomNameCode.transform.parent);
            HideName.gameObject.SetActive(true);
            HideName.name = "HideName";
            HideName.color =
                ColorUtility.TryParseHtmlString(Main.HideColor.Value, out var color) ? color :
                ColorUtility.TryParseHtmlString(ColorHelper.FSColorHex, out var modColor) ? modColor : HideName.color;
            HideName.text = Main.HideName.Value;

            warningText = Object.Instantiate(__instance.GameStartText, __instance.transform);
            warningText.name = "WarningText";
            warningText.transform.localPosition = new Vector3(0f, 0f - __instance.transform.localPosition.y, -1f);
            warningText.gameObject.SetActive(false);

            timerText = Object.Instantiate(__instance.PlayerCounter,
                AmongUsClient.Instance.AmHost
                    ? __instance.StartButton.transform.parent
                    : __instance.StartButtonClient.transform.parent);
            timerText.fontSize = 6.2f;
            timerText.autoSizeTextContainer = true;
            timerText.name = "Timer";
            timerText.DestroyChildren();
            timerText.DestroySubMeshObjects();
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.outlineColor = Color.black;
            timerText.outlineWidth = 0.40f;
            timerText.hideFlags = HideFlags.None;
            timerText.transform.localPosition += new Vector3(-0.55f, -0.4f, 0f);
            timerText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            timerText.gameObject.SetActive(AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame &&
                                           IsVanillaServer);

            cancelButton = Object.Instantiate(__instance.StartButton, __instance.transform);
            var cancelLabel = cancelButton.GetComponentInChildren<TextMeshPro>();
            cancelLabel.DestroyTranslator();
            cancelLabel.text = GetString("Cancel");
            var cancelButtonInactiveRenderer = cancelButton.inactiveSprites.GetComponent<SpriteRenderer>();
            cancelButtonInactiveRenderer.color = new Color(0.8f, 0f, 0f, 1f);
            var cancelButtonActiveRenderer = cancelButton.activeSprites.GetComponent<SpriteRenderer>();
            cancelButtonActiveRenderer.color = Color.red;
            var cancelButtonInactiveShine = cancelButton.inactiveSprites.transform.Find("Shine");
            if (cancelButtonInactiveShine) cancelButtonInactiveShine.gameObject.SetActive(false);

            cancelButton.activeTextColor = cancelButton.inactiveTextColor = Color.white;
            GameStartTextlocalPosition = __instance.GameStartText.transform.localPosition;
            cancelButton.OnClick = new Button.ButtonClickedEvent();
            cancelButton.OnClick.AddListener((Action)(__instance.ResetStartState));
            cancelButton.gameObject.SetActive(false);

            if (!AmongUsClient.Instance.AmHost || (!VersionChecker.isBroken &&
                                                   (!VersionChecker.hasUpdate || !VersionChecker.forceUpdate) &&
                                                   VersionChecker.IsSupported)) return;
            __instance.HostPrivateButton.inactiveTextColor = Palette.DisabledClear;
            __instance.HostPrivateButton.activeTextColor = Palette.DisabledClear;
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public class GameStartManagerUpdatePatch
    {
        private static int updateTimer;
        public static float exitTimer = -1f;

        public static bool Prefix(GameStartManager __instance)
        {
            if (IsInGame) return false;
            // Lobby code
            if (DataManager.Settings.Gameplay.StreamerMode)
            {
                __instance.GameRoomNameCode.color = new Color(__instance.GameRoomNameCode.color.r,
                    __instance.GameRoomNameCode.color.g, __instance.GameRoomNameCode.color.b, 0);
                HideName.enabled = !IsLocalGame;
            }
            else
            {
                __instance.GameRoomNameCode.color = new Color(__instance.GameRoomNameCode.color.r,
                    __instance.GameRoomNameCode.color.g, __instance.GameRoomNameCode.color.b, 255);
                HideName.enabled = false;
            }

            if (!Main.AutoStartGame.Value || !AmongUsClient.Instance.AmHost) return true;
            updateTimer++;
            if (updateTimer < 50) return true;
            updateTimer = 0;
            var maxPlayers = GameManager.Instance.LogicOptions.MaxPlayers;
            if (GameData.Instance.PlayerCount < maxPlayers - 1 || IsCountDown) return true;
            GameStartManager.Instance.startState = GameStartManager.StartingStates.Countdown;
            GameStartManager.Instance.countDownTimer = 10;
            return true;
        }

        public static void Postfix(GameStartManager __instance)
        {
            if (!AmongUsClient.Instance) return;
            if (AmongUsClient.Instance.AmHost)
            {
                /*bool canStartGame = true;
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
                        mismatchedPlayerNameList.Add(StringHelper.ColorString(Palette.PlayerColors[client.ColorId], client.Character.Data.PlayerName));
                    }
                }
                if (!canStartGame)
                {
                    __instance.StartButton.gameObject.SetActive(false);
                    warningMessage = StringHelper.ColorString(Color.red, string.Format(GetString("Warning.MismatchedVersion"), string.Join(" ", mismatchedPlayerNameList), $"<color={ColorHelper.ModColor}>{Main.ModName}</color>"));
                }*/
                cancelButton.gameObject.SetActive(__instance.startState == GameStartManager.StartingStates.Countdown);
                __instance.StartButton.gameObject.SetActive(!cancelButton.gameObject.active);
            }

            /*if (MatchVersions(0, true) || Main.VersionCheat.Value)
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
                    warningMessage = StringHelper.ColorString(Color.red, string.Format(GetString("Warning.AutoExitAtMismatchedVersion"), $"<color={ColorHelper.ModColor}>{Main.ModName}</color>", Math.Round(5 - exitTimer).ToString()));
            }*/
            var warningMessage = "";
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
                __instance.GameStartText.transform.localPosition = new Vector3(
                    __instance.GameStartText.transform.localPosition.x, 2f,
                    __instance.GameStartText.transform.localPosition.z);
            else
                __instance.GameStartText.transform.localPosition = GameStartTextlocalPosition;

            timerText.text = "";
            // Lobby timer
            if (!GameData.Instance || AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame ||
                !IsVanillaServer || !AmongUsClient.Instance.AmHost) return;

            timer = Mathf.Max(0f, timer -= Time.deltaTime);
            var minutes = (int)timer / 60;
            var seconds = (int)timer % 60;
            var countDown = $"{minutes:00}:{seconds:00}";
            if (timer <= 60) countDown = StringHelper.ColorString(Color.red, countDown);
            timerText.text = countDown;
        }

        /*private static bool MatchVersions(byte playerId, bool acceptVanilla = false)
        {
            if (!FinalGameData.PlayerVersion.playerVersion.TryGetValue(playerId, out var version)) return acceptVanilla;
            return Main.ForkId == version.forkId
                   && Main.version.CompareTo(version.version) == 0
                   && version.tag == $"{Main.GitCommit}({Main.GitBranch})";
        }*/
    }
}

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
public static class HiddenTextPatch
{
    public static void Postfix(TextBoxTMP __instance)
    {
        if (__instance.name == "GameIdText") __instance.outputText.text = new string('*', __instance.text.Length);
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.ResetStartState))]
internal class ResetStartStatePatch
{
    public static void Prefix(GameStartManager __instance)
    {
        if (IsCountDown) SoundManager.Instance.StopSound(__instance.gameStartSound);
    }
}