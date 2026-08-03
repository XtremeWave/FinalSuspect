using System;
using AmongUs.Data;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Plugin.UI;
using FinalSuspect.Modules.Resources;
using FinalSuspect.Templates;
using TMPro;
using UnityEngine.UI;
using static FinalSuspect.Modules.Core.Plugin.UI.MainMenu;
using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.System;

[HarmonyPatch]
public class MainMenuManagerPatch
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OpenGameModeMenu))]
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OpenAccountMenu))]
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OpenCredits))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    public static void ShowRightPanel()
    {
        ShowingPanel = true;
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Open))]
    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.Show))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    public static void HideRightPanel()
    {
        try
        {
            ShowingPanel = false;
            AccountManager.Instance?.transform.FindChild("AccountTab/AccountWindow")?.gameObject.SetActive(false);
        }
        catch
        {
            /* ignored */
        }
    }

    public static void ShowRightPanelImmediately()
    {
        ShowingPanel = true;
        RightPanel.transform.localPosition = RightPanelOp;
        Instance.OpenGameModeMenu();
    }

    [HarmonyPatch(typeof(SignInStatusComponent), nameof(SignInStatusComponent.SetOnline))]
    [HarmonyPostfix]
    public static void SetOnline_Postfix()
    {
        _ = new LateTask(() => { isOnline = true; }, 0.1f, "Set Online Status");
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
    [HarmonyPostfix]
    public static void MainMenuManager_LateUpdate(MainMenuManager __instance)
    {
        CustomPopup.Update();
        OVersionShower.transform.FindChild("Text_TMP").gameObject.GetComponent<RectTransform>()
            .sizeDelta = new Vector2(3.9f, 0.2359f);
        if (!GameObject.Find("MainUI")) ShowingPanel = false;
        CreditTextCredential.gameObject.SetActive(!ShowingPanel && Active);
        if (RightPanel)
        {
            var pos1 = RightPanel.transform.localPosition;
            var pos3 = new Vector3(
                RightPanelOp.x * GetResolutionOffset(),
                RightPanelOp.y, RightPanelOp.z);
            var lerp1 = Vector3.Lerp(pos1, ShowingPanel ? pos3 : RightPanelOp + new Vector3(20f, 0f, 0f),
                Time.deltaTime * (ShowingPanel ? 3f : 2f));
            if (ShowingPanel
                    ? RightPanel.transform.localPosition.x > pos3.x + 0.03f
                    : RightPanel.transform.localPosition.x < RightPanelOp.x + 29f
               ) RightPanel.transform.localPosition = lerp1;
        }

        if (ShowedBak || !isOnline) return;
        if (!BackgroundTexture || !BackgroundTexture.active) return;
        var pos2 = BackgroundTexture.transform.position;
        var lerp2 = Vector3.Lerp(pos2, new Vector3(pos2.x, 7.1f, pos2.z), Time.deltaTime * 1.4f);
        BackgroundTexture.transform.position = lerp2;
        if (pos2.y > 7f) ShowedBak = true;
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    public static void Start_Postfix(MainMenuManager __instance)
    {
        Instance = __instance;
        SimpleButton.SetBase(__instance.quitButton);

        var row = 1;
        var col = 0;

        var inviteLinkName = IsChineseUser ? "QQ群" : "Discord";
        var inviteLinkUrl = IsChineseUser ? QQInviteUrl : DiscordInviteUrl;

        if (!InviteButton) InviteButton = CreatButton(inviteLinkName, () => { OpenUrl(inviteLinkUrl); });
        InviteButton.gameObject.SetActive(true);
        InviteButton.name = "FinalSuspect Extra Link Button";

        if (WebsiteButton == null) WebsiteButton = CreatButton(GetString("Website"), () => OpenUrl(WebsiteUrl));
        WebsiteButton.gameObject.SetActive(true);
        WebsiteButton.name = "FinalSuspect Website Button";

        if (!GithubButton) GithubButton = CreatButton("Github", () => OpenUrl(GithubRepoUrl));
        GithubButton.gameObject.SetActive(true);
        GithubButton.name = "FinalSuspect Github Button";

        var bugLinkName = IsChineseUser ? "Bug 反馈" : "Bug Report";
        var bugLinkUrl = IsChineseUser ? BugReportUrl_Gitee : BugReportUrl_Github;
        if (!BugReportButton) BugReportButton = CreatButton(bugLinkName, () => { OpenUrl(bugLinkUrl); });
        BugReportButton.gameObject.SetActive(true);
        BugReportButton.name = "FinalSuspect Bug Report Button";
        PlayButton = __instance.playButton.gameObject;

        if (!UpdateButton)
        {
            UpdateButton = Object.Instantiate(PlayButton, PlayButton.transform.parent);
            UpdateButton.name = "FinalSuspect Update Button";
            UpdateButton.transform.localPosition = PlayButton.transform.localPosition - new Vector3(0f, 0f, 3f);
            var passiveButton = UpdateButton.GetComponent<PassiveButton>();
            passiveButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new Color(0.49f, 0.34f, 0.62f, 0.8f);
            passiveButton.activeSprites.GetComponent<SpriteRenderer>().color = new Color(0.49f, 0.34f, 0.62f, 1f);
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener((Action)(() =>
            {
                PlayButton.SetActive(true);
                UpdateButton.SetActive(false);
                if (DebugModeManager.IsDebugMode && Input.GetKey(KeyCode.LeftShift)) return;
                if (VersionChecker.CanUpdate)
                    ModUpdater.StartUpdate();
                else
                    CustomPopup.Show(GetString("UpdateRemind.BySelf_Title"), GetString("UpdateRemind.BySelf_Text"),
                        [(GetString(StringNames.Okay), null)]);
            }));
            UpdateButton.transform.transform.FindChild("FontPlacer").GetChild(0).gameObject.DestroyTranslator();
        }

        Application.targetFrameRate = ConfigManager.UnlockFPS.Value ? 165 : 60;
        return;

        void OpenUrl(string url)
        {
#if Android
            OpenURLAndroid(url);
#elif Windows
            Application.OpenURL(url);
#endif
        }

        GameObject CreatButton(string text, Action action)
        {
            col++;
            if (col > 2)
            {
                col = 1;
                row++;
            }

            var template = col == 1 ? __instance.creditsButton.gameObject : __instance.quitButton.gameObject;
            var button = Object.Instantiate(template, template.transform.parent);
            Object.Destroy(button.GetComponent<ConditionalHide>());
            button.SetActive(true);
            button.transform.transform.FindChild("FontPlacer").GetChild(0).gameObject.DestroyTranslator();
            var buttonText = button.transform.FindChild("FontPlacer").GetChild(0).GetComponent<TextMeshPro>();
            buttonText.text = text;
            var passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener(action);
            var aspectPosition = button.GetComponent<AspectPosition>();
#if Android
            var yPosition = col == 1 ? 0.5f - 0.08f * row : 0.5f - 0.08f * (row - 1);
#else
            var yPosition = 0.5f - 0.08f * row;
#endif

            aspectPosition.anchorPoint = new Vector2(
                col == 1 ? 0.415f : 0.583f,
                yPosition
            );
            var scale = button.transform.localScale;
            button.transform.localScale = new Vector3(scale.x * GetResolutionOffset(), button.transform.localScale.y);
            MainMenuCustomButtons.Add(button);
            return button;
        }
    }
#if Android
    private static void OpenURLAndroid(string url)
    {
        try
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            // 创建 Intent
            var intentClass = new AndroidJavaClass("android.content.Intent");
            var intentObject =
                new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW");

            // 创建 URI
            var uriClass = new AndroidJavaClass("android.net.Uri");
            var uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", url);

            // 设置 Intent 的数据
            intentObject.Call<AndroidJavaObject>("setData", uriObject);

            // 设置标志确保在新任务中打开
            const int FLAG_ACTIVITY_NEW_TASK = 0x10000000;
            intentObject.Call<AndroidJavaObject>("setFlags", FLAG_ACTIVITY_NEW_TASK);

            // 启动 Activity
            currentActivity.Call("startActivity", intentObject);

            // 释放资源（虽然不是必须的，但推荐）
            unityPlayer.Dispose();
            currentActivity.Dispose();
            intentClass.Dispose();
            intentObject.Dispose();
            uriClass.Dispose();
            uriObject.Dispose();
        }
        catch
        {
            // 降级到Application.OpenURL
            Application.OpenURL(url);
        }
    }

#endif
}