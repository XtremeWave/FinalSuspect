using FinalSuspect.Attributes;
using FinalSuspect.DataHandling.FinalGameData;
using FinalSuspect.Modules.Features.DisplayedRoleTag;
using TMPro;
using static FinalSuspect.Modules.Core.Game.UI.ModGameManager;

namespace FinalSuspect.Modules.Core.Game.UI;

public static class InGameInfoPane
{
    private static GameObject _switchPaneButton;
    public static LobbyInfoPane Instance;

    [GameModuleInitializer]
    public static void OnInitialization()
    {
        var lobbyPane = Instance;
        var aspect = lobbyPane.transform.FindChild("AspectSize");
        SetupInfoPaneUI(aspect);
        AdjustInfoPane(lobbyPane);
        CreateHidePaneButton(aspect);
    }

    private static void SetupInfoPaneUI(Transform aspect)
    {
        var trans = aspect.FindChild("GameSettingsButtons");

        trans.FindChild("Host Buttons").gameObject.SetActive(false);
        trans.FindChild("Client Buttons").gameObject.SetActive(true);

        var header = trans.FindChild("ButtonSettingsHeader").gameObject;
        var headerTransform = header.transform;
        headerTransform.localPosition =
            new Vector3(-0.282f, headerTransform.localPosition.y, headerTransform.localPosition.z);

        var tmp = header.GetComponent<TextMeshPro>();
        tmp.text += $" - {GetString("PressF2ToHidePane")}";

        var rect = header.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(3f, rect.sizeDelta.y);
    }

    private static void AdjustInfoPane(LobbyInfoPane lobbyPane)
    {
        var aspectPosition = lobbyPane.gameObject.GetComponent<AspectPosition>();
        aspectPosition.DistanceFromEdge += Vector3.forward * -60;

        lobbyPane.gameObject.transform.localScale *= 0.8f;
    }

    private static void CreateHidePaneButton(Transform aspect)
    {
        if (_switchPaneButton != null)
        {
            Object.Destroy(_switchPaneButton);
            _switchPaneButton = null;
        }


        var buttonTemplate = aspect.FindChild("GameCodeSection").FindChild("CopyGameCodeButton").gameObject;
        _switchPaneButton = CreateFunctionalButton(
            buttonTemplate,
            HudManager.Instance.gameObject.transform,
            "Show Hide Panel Button",
            new Vector3(-0.2f, -0.5f, 1f),
            "eye.png",
            new Color(0.8f, 0.8f, 1f, 0.3f),
            new Vector3(0.22f, 1.85f, -80f),
            () =>
            {
                Test(1);
                ConfigManager.ShowInfoPanel.Value = !ConfigManager.ShowInfoPanel.Value;
                Test(ConfigManager.ShowInfoPanel.Value);
            });
    }

    public static void CheckForHotkey()
    {
        if (!Input.GetKeyDown(KeyCode.F2) || !IsInGame) return;
        Test(0);
        ConfigManager.ShowInfoPanel.Value = !ConfigManager.ShowInfoPanel.Value;
        Test(ConfigManager.ShowInfoPanel.Value);
    }

    public static void SetShowInfoPanel()
    {
        var showInfo = ConfigManager.ShowInfoPanel.Value;
        var map = MapBehaviour.Instance != null && (MapBehaviour.Instance?.gameObject.activeSelf ?? false);
        var notShowPane =
            (DestroyableSingleton<HudManager>.Instance?.Chat?.IsOpenOrOpening ?? false)
            || map
            || !showInfo
            || !FinalGameData.IntroDestroyed
            || (DisplayerRoleTagHelper.selectionUI?.active ?? false);
        if (IsFreePlay || !IsInGame) return;
        if ((Instance?.gameObject.activeSelf ?? true) && notShowPane)
        {
            Test(0);
            Instance?.DeactivatePane();
        }

        Instance?.gameObject.SetActive(!notShowPane);
    }
}