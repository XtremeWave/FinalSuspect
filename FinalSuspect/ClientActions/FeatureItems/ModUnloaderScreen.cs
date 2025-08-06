using System;
using FinalSuspect.Patches.System;
using InnerNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FinalSuspect.ClientActions.FeatureItems;

public static class ModUnloaderScreen
{
    public static SpriteRenderer Popup { get; set; }
    private static TextMeshPro WarnText { get; set; }
    private static ToggleButtonBehaviour CancelButton { get; set; }
    private static ToggleButtonBehaviour UnloadButton { get; set; }

    public static void Init(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        Popup = Object.Instantiate(optionsMenuBehaviour.Background, ClientFeatureItem.CustomBackground.transform);
        Popup.name = "UnloadModPopup";
        Popup.transform.localPosition = new Vector3(0f, 0f, -8f);
        Popup.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        Popup.gameObject.SetActive(false);

        WarnText = Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement.Text, Popup.transform);
        WarnText.name = "Warning";
        WarnText.transform.localPosition = new Vector3(0f, 1f, -1f);
        WarnText.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
        WarnText.gameObject.SetActive(true);

        CancelButton = Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement, Popup.transform);
        CancelButton.name = "Cancel";
        CancelButton.transform.localPosition = new Vector3(-1.2f, -1f, -2f);
        CancelButton.Text.text = GetString("Cancel");
        var cancelPassiveButton = CancelButton.GetComponent<PassiveButton>();
        cancelPassiveButton.OnClick = new Button.ButtonClickedEvent();
        cancelPassiveButton.OnClick.AddListener((Action)Hide);
        CancelButton.gameObject.SetActive(true);

        UnloadButton = Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement, Popup.transform);
        UnloadButton.name = "Unload";
        UnloadButton.transform.localPosition = new Vector3(1.2f, -1f, -2f);
        UnloadButton.Background.color = UnloadButton.Text.color = Color.red;
        UnloadButton.Text.text = GetString("Unload");
        var unloadPassiveButton = UnloadButton.GetComponent<PassiveButton>();
        unloadPassiveButton.OnClick = new Button.ButtonClickedEvent();
        unloadPassiveButton.OnClick.AddListener(new Action(() =>
        {
            ClientActionItem.CustomBackground.gameObject.SetActive(false);
            ClientActionItem.ModOptionsButton.gameObject.SetActive(false);
            ClientFeatureItem.CustomBackground.gameObject.SetActive(false);
            ClientFeatureItem.ModOptionsButton.gameObject.SetActive(false);

            try
            {
                MainMenuManagerPatch.ShowRightPanelImmediately();
            }
            catch
            {
                /* ignored */
            }

            _ = new LateTask(() =>
            {
                Info("模组将要禁用", nameof(ModUnloaderScreen));
                Harmony.UnpatchAll();
                Main.Instance.Unload();
            }, 1f);
        }));
    }

    public static void Show()
    {
        if (!Popup) return;
        Popup.gameObject.SetActive(true);

        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
        {
            WarnText.text = GetString("Tip.CannotUnloadDuringGame");
            UnloadButton.gameObject.SetActive(false);
        }
        else
        {
            WarnText.text = GetString("Tip.UnloadWarning");
            UnloadButton.gameObject.SetActive(true);
        }
    }

    public static void Hide()
    {
        Popup?.gameObject.SetActive(false);
    }
}