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
    private static float _timer = 600f;
    private static Vector3 _gameStartTextlocalPosition;
    private static TextMeshPro _timerText;
    private static PassiveButton _cancelButton;
    private static PassiveButton _skipButton;
    private static TextMeshPro _warningText;
    private static TextMeshPro _hideName;

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
            __instance.MinPlayers = 1;

            __instance.GameRoomNameCode.text = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            _timer = 600f;

            _hideName = Object.Instantiate(__instance.GameRoomNameCode, __instance.GameRoomNameCode.transform.parent);
            _hideName.gameObject.SetActive(true);
            _hideName.name = "HideName";
            _hideName.color =
                ColorUtility.TryParseHtmlString(ConfigManager.HideColor.Value, out var color) ? color :
                ColorUtility.TryParseHtmlString(ColorHelper.FSColorHex, out var modColor) ? modColor : _hideName.color;
            _hideName.text = ConfigManager.HideName.Value;

            _warningText = Object.Instantiate(__instance.GameStartText, __instance.transform);
            _warningText.name = "WarningText";
            _warningText.transform.localPosition = new Vector3(0f, 0f - __instance.transform.localPosition.y, -1f);
            _warningText.gameObject.SetActive(false);

            _timerText = Object.Instantiate(__instance.PlayerCounter,
                AmongUsClient.Instance.AmHost
                    ? __instance.StartButton.transform.parent
                    : __instance.StartButtonClient.transform.parent);
            _timerText.fontSize = 6.2f;
            _timerText.autoSizeTextContainer = true;
            _timerText.name = "Timer";
            _timerText.DestroyChildren();
            _timerText.DestroySubMeshObjects();
            _timerText.alignment = TextAlignmentOptions.Center;
            _timerText.outlineColor = Color.black;
            _timerText.outlineWidth = 0.40f;
            _timerText.hideFlags = HideFlags.None;
            _timerText.transform.localPosition += new Vector3(-0.55f, -0.4f, 0f);
            _timerText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            _timerText.gameObject.SetActive(AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame &&
                                            IsVanillaServer);

            _cancelButton = Object.Instantiate(__instance.StartButton, __instance.transform);
            var cancelLabel = _cancelButton.GetComponentInChildren<TextMeshPro>();
            cancelLabel.DestroyTranslator();
            cancelLabel.text = GetString("Cancel");
            var cancelButtonInactiveRenderer = _cancelButton.inactiveSprites.GetComponent<SpriteRenderer>();
            cancelButtonInactiveRenderer.color = new Color(0.8f, 0f, 0f, 1f);
            var cancelButtonActiveRenderer = _cancelButton.activeSprites.GetComponent<SpriteRenderer>();
            cancelButtonActiveRenderer.color = Color.red;
            var cancelButtonInactiveShine = _cancelButton.inactiveSprites.transform.Find("Shine");
            if (cancelButtonInactiveShine) cancelButtonInactiveShine.gameObject.SetActive(false);

            _cancelButton.activeTextColor = _cancelButton.inactiveTextColor = Color.white;
            _gameStartTextlocalPosition = __instance.GameStartText.transform.localPosition;
            _cancelButton.OnClick = new Button.ButtonClickedEvent();
            _cancelButton.OnClick.AddListener((Action)(__instance.ResetStartState));
            _cancelButton.gameObject.SetActive(false);

            _skipButton = Object.Instantiate(__instance.StartButton, __instance.transform);
            var skipLabel = _skipButton.GetComponentInChildren<TextMeshPro>();
            skipLabel.DestroyTranslator();
            _skipButton.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            _skipButton.transform.localPosition = new Vector3(2f, 0.13f, 0f);
            skipLabel.text = GetString("Skip");
            var skipButtonInactiveRenderer = _skipButton.inactiveSprites.GetComponent<SpriteRenderer>();
            skipButtonInactiveRenderer.color = new Color(0f, 0.6f, 0.6f, 1f);
            var skipButtonActiveRenderer = _skipButton.activeSprites.GetComponent<SpriteRenderer>();
            skipButtonActiveRenderer.color = new Color(0f, 0.6f, 0.6f, 1f);
            var skipButtonInactiveShine = _skipButton.inactiveSprites.transform.Find("Shine");
            if (skipButtonInactiveShine) skipButtonInactiveShine.gameObject.SetActive(false);

            _skipButton.activeTextColor = _skipButton.inactiveTextColor = Color.white;

            _skipButton.OnClick = new Button.ButtonClickedEvent();
            _skipButton.OnClick.AddListener(new Action(() =>
            {
                GameStartManager.Instance.countDownTimer = 0;
                SoundManager.Instance.StopSound(GameStartManager.Instance.gameStartSound);
            }));
            _skipButton.gameObject.SetActive(false);

            if (!AmongUsClient.Instance.AmHost || (!VersionChecker.IsBroken &&
                                                   (!VersionChecker.HasUpdate || !VersionChecker.ForceUpdate) &&
                                                   VersionChecker.IsSupported)) return;
            __instance.HostPrivateButton.inactiveTextColor = Palette.DisabledClear;
            __instance.HostPrivateButton.activeTextColor = Palette.DisabledClear;
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public class GameStartManagerUpdatePatch
    {
        private static int _updateTimer;

        public static bool Prefix(GameStartManager __instance)
        {
            if (IsInGame) return false;
            // Lobby code
            if (DataManager.Settings.Gameplay.StreamerMode)
            {
                __instance.GameRoomNameCode.color = new Color(__instance.GameRoomNameCode.color.r,
                    __instance.GameRoomNameCode.color.g, __instance.GameRoomNameCode.color.b, 0);
                _hideName.enabled = !IsLocalGame;
            }
            else
            {
                __instance.GameRoomNameCode.color = new Color(__instance.GameRoomNameCode.color.r,
                    __instance.GameRoomNameCode.color.g, __instance.GameRoomNameCode.color.b, 255);
                _hideName.enabled = false;
            }

            if (!ConfigManager.AutoStartGame.Value
                || !AmongUsClient.Instance.AmHost
                || GameStartManager.Instance.startState == GameStartManager.StartingStates.Starting
                || IsInitGame) return true;
            _updateTimer++;
            if (_updateTimer < 50) return true;
            _updateTimer = 0;
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
                _cancelButton.gameObject.SetActive(__instance.startState == GameStartManager.StartingStates.Countdown);
                _skipButton.gameObject.SetActive(__instance.startState == GameStartManager.StartingStates.Countdown);
                __instance.StartButton.gameObject.SetActive(!_cancelButton.gameObject.active);
            }

            if (AmongUsClient.Instance.AmHost)
                __instance.GameStartText.transform.localPosition = new Vector3(
                    __instance.GameStartText.transform.localPosition.x, 2f,
                    __instance.GameStartText.transform.localPosition.z);
            else
                __instance.GameStartText.transform.localPosition = _gameStartTextlocalPosition;

            _timerText.text = "";
            // Lobby timer
            if (!GameData.Instance || AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame ||
                !IsVanillaServer || !AmongUsClient.Instance.AmHost) return;

            _timer = Mathf.Max(0f, _timer -= Time.deltaTime);
            var minutes = (int)_timer / 60;
            var seconds = (int)_timer % 60;
            var countDown = $"{minutes:00}:{seconds:00}";
            if (_timer <= 60) countDown = StringHelper.ColorString(Color.red, countDown);
            _timerText.text = countDown;
        }
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