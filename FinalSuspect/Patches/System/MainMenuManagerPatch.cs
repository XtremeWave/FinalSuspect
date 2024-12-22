using HarmonyLib;
using System;
using TMPro;
using System.IO;
using FinalSuspect.Templates;
using UnityEngine;
using static FinalSuspect.Translator;
using Object = UnityEngine.Object;
using FinalSuspect.Modules.Managers;
using FinalSuspect.Modules.Managers.ResourcesManager;

namespace FinalSuspect;

[HarmonyPatch]
public class MainMenuManagerPatch
{
    public static MainMenuManager Instance { get; private set; }

    public static GameObject InviteButton;
    public static GameObject GithubButton;
    public static GameObject WebsiteButton;
    public static GameObject UpdateButton;
    public static GameObject PlayButton;

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OpenGameModeMenu))]
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OpenAccountMenu))]
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OpenCredits))]
    [HarmonyPrefix, HarmonyPriority(Priority.Last)]
    public static void ShowRightPanel() => ShowingPanel = true;

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Open))]
    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.Show))]
    [HarmonyPrefix, HarmonyPriority(Priority.Last)]
    public static void HideRightPanel()
    {
        ShowingPanel = false;
        AccountManager.Instance?.transform?.FindChild("AccountTab/AccountWindow")?.gameObject?.SetActive(false);
    }

    public static void ShowRightPanelImmediately()
    {
        ShowingPanel = true;
        TitleLogoPatch.RightPanel.transform.localPosition = TitleLogoPatch.RightPanelOp;
        Instance.OpenGameModeMenu();
    }

    private static bool isOnline = false;
    public static bool ShowedBak = false;
    public static bool ShowingPanel = false;
    [HarmonyPatch(typeof(SignInStatusComponent), nameof(SignInStatusComponent.SetOnline)), HarmonyPostfix]
    public static void SetOnline_Postfix() { _ = new LateTask(() => { isOnline = true; }, 0.1f, "Set Online Status"); }
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate)), HarmonyPostfix]
    public static void MainMenuManager_LateUpdate()
    {
        CustomPopup.Update();

        if (GameObject.Find("MainUI") == null) ShowingPanel = false;
        VersionShowerStartPatch.CreditTextCredential.gameObject.SetActive(!ShowingPanel && MainMenuButtonHoverAnimation.Active);

        if (TitleLogoPatch.RightPanel != null)
        {
            var pos1 = TitleLogoPatch.RightPanel.transform.localPosition;
            Vector3 lerp1 = Vector3.Lerp(pos1, TitleLogoPatch.RightPanelOp + new Vector3((ShowingPanel ? 0f : 10f), 0f, 0f), Time.deltaTime * (ShowingPanel ? 3f : 2f));
            if (ShowingPanel
                ? TitleLogoPatch.RightPanel.transform.localPosition.x > TitleLogoPatch.RightPanelOp.x + 0.03f
                : TitleLogoPatch.RightPanel.transform.localPosition.x < TitleLogoPatch.RightPanelOp.x + 9f
                ) TitleLogoPatch.RightPanel.transform.localPosition = lerp1;
        }

        if (ShowedBak || !isOnline) return;
        var bak = GameObject.Find("BackgroundTexture");
        if (bak == null || !bak.active) return;
        var pos2 = bak.transform.position;
        Vector3 lerp2 = Vector3.Lerp(pos2, new Vector3(pos2.x, 7.1f, pos2.z), Time.deltaTime * 1.4f);
        bak.transform.position = lerp2;
        if (pos2.y > 7f) ShowedBak = true;

    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPostfix]
    public static void Start_Postfix(MainMenuManager __instance)
    {
        Instance = __instance;

        SimpleButton.SetBase(__instance.quitButton);

        int row = 1; int col = 0;
        GameObject CreatButton(string text, Action action)
        {
            col++; if (col > 2) { col = 1; row++; }
            var template = col == 1 ? __instance.creditsButton.gameObject : __instance.quitButton.gameObject;
            var button = Object.Instantiate(template, template.transform.parent);
            button.transform.transform.FindChild("FontPlacer").GetChild(0).gameObject.DestroyTranslator();
            var buttonText = button.transform.FindChild("FontPlacer").GetChild(0).GetComponent<TextMeshPro>();
            buttonText.text = text;
            PassiveButton passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener(action);
            AspectPosition aspectPosition = button.GetComponent<AspectPosition>();
            aspectPosition.anchorPoint = new Vector2(col == 1 ? 0.415f : 0.583f, 0.5f - 0.08f * row);
            return button;
        }



        var extraLinkName = IsChineseUser ? "QQ群" : "Discord";
        var extraLinkUrl = IsChineseUser ? Main.QQInviteUrl : Main.DiscordInviteUrl;

        if (InviteButton == null) InviteButton = CreatButton(extraLinkName, () => { Application.OpenURL(extraLinkUrl); });
        InviteButton.gameObject.SetActive(true);
        InviteButton.name = "FinalSuspect Extra Link Button";

        if (WebsiteButton == null) WebsiteButton = CreatButton(GetString("Website"), () => Application.OpenURL(Main.WebsiteUrl));
        WebsiteButton.gameObject.SetActive(true);
        WebsiteButton.name = "FinalSuspect Website Button";

        if (GithubButton == null) GithubButton = CreatButton("Github", () => Application.OpenURL(Main.GithubRepoUrl));
        GithubButton.gameObject.SetActive(true);
        GithubButton.name = "FinalSuspect Github Button";
        PlayButton = __instance.playButton.gameObject;

        if (UpdateButton == null)
        {
            UpdateButton = Object.Instantiate(PlayButton, PlayButton.transform.parent);
            UpdateButton.name = "FinalSuspect Update Button";
            UpdateButton.transform.localPosition = PlayButton.transform.localPosition - new Vector3(0f, 0f, 3f);
            var passiveButton = UpdateButton.GetComponent<PassiveButton>();
            passiveButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new Color(0.49f, 0.34f, 0.62f, 0.8f);
            passiveButton.activeSprites.GetComponent<SpriteRenderer>().color = new Color(0.49f, 0.34f, 0.62f, 1f);
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((Action)(() =>
            {
                PlayButton.SetActive(true);
                UpdateButton.SetActive(false);
                if (!DebugModeManager.AmDebugger || !Input.GetKey(KeyCode.LeftShift))
                {
                    if (VersionChecker.CanUpdate)
                    {
                        ModUpdater.StartUpdate();
                    }
                    else
                    {
                        CustomPopup.Show(GetString("UpdateBySelfTitle"), GetString("UpdateBySelfText"), new() { (GetString(StringNames.Okay), null) });
                    }
                }
            }));
            UpdateButton.transform.transform.FindChild("FontPlacer").GetChild(0).gameObject.DestroyTranslator();
        }
        Application.targetFrameRate = Main.UnlockFPS.Value ? 165 : 60;
    }
}