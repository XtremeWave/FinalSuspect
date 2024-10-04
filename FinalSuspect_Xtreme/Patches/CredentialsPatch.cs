using HarmonyLib;
using System.Collections.Generic;
using System.Text;
using TMPro;
using FinalSuspect_Xtreme.Templates;
using UnityEngine;

using static FinalSuspect_Xtreme.Translator;
using System.Linq;
using FinalSuspect_Xtreme.Modules.Managers.ResourcesManager;

namespace FinalSuspect_Xtreme;

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
internal class PingTrackerUpdatePatch
{
    private static float deltaTime;
    public static string ServerName = "";
    private static TextMeshPro CreditTextCredential = null;
    private static AspectPosition CreditTextCredentialAspectPos = null;
    private static void Postfix(PingTracker __instance)
    {
        if (CreditTextCredential == null)
        {
            var uselessPingTracker = Object.Instantiate(__instance, __instance.transform.parent);
            CreditTextCredential = uselessPingTracker.GetComponent<TextMeshPro>();
            Object.Destroy(uselessPingTracker);
            CreditTextCredential.alignment = TextAlignmentOptions.TopRight;
            CreditTextCredential.color = new(1f, 1f, 1f, 0.7f);
            CreditTextCredential.rectTransform.pivot = new(1f, 1f);  // 中心を右上角に設定
            CreditTextCredentialAspectPos = CreditTextCredential.GetComponent<AspectPosition>();
            CreditTextCredentialAspectPos.Alignment = AspectPosition.EdgeAlignments.RightTop;
        }
        if (CreditTextCredentialAspectPos)
        {
            CreditTextCredentialAspectPos.DistanceFromEdge = 
                DestroyableSingleton<HudManager>.InstanceExists && DestroyableSingleton<HudManager>.Instance.Chat.chatButton.gameObject.active 
                ? new(2.5f, 0f, -800f)
                        : new(1.8f, 0f, -800f);
        }
        StringBuilder sb = new();
        
        sb.Append(Main.CredentialsText);

        CreditTextCredential.text = sb.ToString();
        if ((GameSettingMenu.Instance?.gameObject?.active ?? false) || XtremeGameData.GameStates.IsMeeting)
            CreditTextCredential.text = "";

        var ping = AmongUsClient.Instance.Ping;
        string color = "#ff4500";
        if (ping < 50) color = "#44dfcc";
        else if (ping < 100) color = "#7bc690";
        else if (ping < 200) color = "#f3920e";
        else if (ping < 400) color = "#ff146e";

        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = Mathf.Ceil(1.0f / deltaTime);


        __instance.text.alignment = TextAlignmentOptions.TopGeoAligned;
        __instance.text.text = 
            $"<color={color}>{GetString("Ping")}:{ping} <size=60%>ms</size></color>" + "  " 
            + $"<color=#00a4ff>{GetString("FrameRate")}:{fps} <size=60%>FPS</size></color>" +
            $"{"    <color=#FFDCB1>◈</color>" + (XtremeGameData.GameStates.IsOnlineGame ? ServerName : GetString("Local"))}";

        //__instance.text.transform.localPosition = 
        //    new Vector3(
        //        __instance.text.transform.localPosition.x, 
        //    XtremeGameData.GameStates.IsInGame? __instance.text.transform.localPosition.y + 0.2f: __instance.text.transform.localPosition.y  - 0.2f, 
        //    __instance.text.transform.localPosition.z);


    }
}
[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
internal class VersionShowerStartPatch
{
    public static GameObject OVersionShower;
    private static TextMeshPro VisitText;
    private static TextMeshPro CreditTextCredential = null;


    private static void Postfix(VersionShower __instance)
    {
        TMPTemplate.SetBase(__instance.text);

        Main.CredentialsText = $"\r\n<size=120%>" +
            $"<color={ColorHelper.TeamColor}>==</color> <color={ColorHelper.ModColor}>{Main.ModName}</color> <color={ColorHelper.TeamColor}>==</color>"
            + "</size>";
        Main.CredentialsText += $"\r\n <color=#fffcbe> By </color><color=#cdfffd>XtremeWave</color></size>";
        Main.CredentialsText += $"\r\n<color=#C8FF78>v{Main.DisplayedVersion}</color>";

#if DEBUG
        Main.CredentialsText += $"\r\n<color={ColorHelper.ModColor}>{ThisAssembly.Git.Branch}</color> - {ThisAssembly.Git.Commit}";
#endif

#if RELEASE
        string additionalCredentials = GetString("TextBelowVersionText");
        if (additionalCredentials != null && additionalCredentials != "*" && additionalCredentials != "")
        {
            Main.CredentialsText += $"\r\n{additionalCredentials}";
        }
#endif

        ErrorText.Create(__instance.text);
        if (Main.hasArgumentException && ErrorText.Instance != null)
            ErrorText.Instance.AddError(ErrorCode.Main_DictionaryError);

        if ((OVersionShower = GameObject.Find("VersionShower")) != null && VisitText == null)
        {
            VisitText = Object.Instantiate(__instance.text);
            VisitText.name = "FinalSuspect_Xtreme VisitText";
            VisitText.alignment = TextAlignmentOptions.Left;
            VisitText.text = VersionChecker.visit > 0
                ? string.Format(GetString("FinalSuspect_XtremeVisitorCount"), ColorHelper.ModColor)
                : GetString("ConnectToFinalSuspect_XtremeServerFailed");
            VisitText.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            VisitText.transform.localPosition = new Vector3(-3.92f, -2.9f, 0f);
            VisitText.enabled = GameObject.Find("FinalSuspect_Xtreme Background") != null;

            __instance.text.alignment = TextAlignmentOptions.Left;
            OVersionShower.transform.localPosition = new Vector3(-4.92f, -3.3f, 0f);

            var ap1 = OVersionShower.GetComponent<AspectPosition>();
            if (ap1 != null) Object.Destroy(ap1);
            var ap2 = VisitText.GetComponent<AspectPosition>();
            if (ap2 != null) Object.Destroy(ap2);
        };

        if ((OVersionShower = GameObject.Find("VersionShower")) != null && CreditTextCredential == null)
        {
            string credentialsText =  string.Format(GetString("MainMenuCredential"), $"<color={ColorHelper.TeamColor}>XtremeWave</color>");
            credentialsText += "\n";
            string versionText = $"<color={ColorHelper.ModColor}>FSX</color> - <color=#C8FF78>v{Main.DisplayedVersion}</color>";

#if DEBUG
        versionText = $"<color={ColorHelper.ModColor}>{ThisAssembly.Git.Branch}</color> - {ThisAssembly.Git.Commit}";
#endif

            credentialsText += versionText;


            CreditTextCredential = Object.Instantiate(__instance.text);
            CreditTextCredential.name = "FinalSuspect_Xtreme CreditTex";
            CreditTextCredential.alignment = TextAlignmentOptions.Right;
            CreditTextCredential.text = credentialsText;
            CreditTextCredential.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            CreditTextCredential.transform.localPosition = new Vector3(0.3f, -2.6f, 0f);
            // 查找并获取 "VisitText" 的 TMP 文本对象

            CreditTextCredential.enabled = GameObject.Find("FinalSuspect_Xtreme Background") != null;

            var ap1 = OVersionShower.GetComponent<AspectPosition>();
            if (ap1 != null) Object.Destroy(ap1);
            var ap2 = CreditTextCredential.GetComponent<AspectPosition>();
            if (ap2 != null) Object.Destroy(ap2);
        }


    }
}
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPriority(Priority.First)]
internal class TitleLogoPatch
{
    public static GameObject ModStamp;
    public static GameObject FinalSuspect_Xtreme_Background;
    public static GameObject Ambience;
    public static GameObject Starfield;
    public static GameObject LeftPanel;
    public static GameObject RightPanel;
    public static GameObject CloseRightButton;
    public static GameObject Tint;
    public static GameObject Sizer;
    public static GameObject AULogo;
    public static GameObject BottomButtonBounds;

    public static Vector3 RightPanelOp;

    private static void Postfix(MainMenuManager __instance)
    {
        GameObject.Find("BackgroundTexture")?.SetActive(!MainMenuManagerPatch.ShowedBak);

        Color shade = new(0f, 0f, 0f, 0f);
        var standardActiveSprite = __instance.newsButton.activeSprites.GetComponent<SpriteRenderer>().sprite;
        var minorActiveSprite = __instance.quitButton.activeSprites.GetComponent<SpriteRenderer>().sprite;

        var friendsButton = AwakeFriendCodeUIPatch.FriendsButton.GetComponent<PassiveButton>();
        Dictionary<List<PassiveButton>, (Sprite, Color, Color, Color, Color)> mainButtons = new()
        {
            {new List<PassiveButton>() {__instance.playButton, __instance.inventoryButton, __instance.shopButton},
                (standardActiveSprite, new(0.5216f, 1f, 0.9490f, 0.8f), shade, Color.white, Color.white) },
            {new List<PassiveButton>() {__instance.newsButton, __instance.myAccountButton, __instance.settingsButton},
                (minorActiveSprite, new( 0.5216f, 0.7765f, 1f, 0.8f), shade, Color.white, Color.white) },
            {new List<PassiveButton>() {__instance.creditsButton, __instance.quitButton},
                (minorActiveSprite, new(0.7294f, 0.6353f, 1.0f, 0.8f), shade, Color.white, Color.white) },
            {new List<PassiveButton>() {friendsButton },
                (minorActiveSprite, new(0.0235f, 0f, 0.8f, 0.8f), shade, Color.white, Color.white) },
        };

        void FormatButtonColor(PassiveButton button, Sprite borderType, Color inActiveColor, Color activeColor, Color inActiveTextColor, Color activeTextColor)
        {
            button.activeSprites.transform.FindChild("Shine")?.gameObject?.SetActive(false);
            button.inactiveSprites.transform.FindChild("Shine")?.gameObject?.SetActive(false);
            var activeRenderer = button.activeSprites.GetComponent<SpriteRenderer>();
            var inActiveRenderer = button.inactiveSprites.GetComponent<SpriteRenderer>();
            activeRenderer.sprite = minorActiveSprite;
            inActiveRenderer.sprite = minorActiveSprite;
            activeRenderer.color = activeColor.a == 0f ?
                new Color(inActiveColor.r, inActiveColor.g, inActiveColor.b, 1f) : activeColor;
            inActiveRenderer.color = inActiveColor;
            button.activeTextColor = activeTextColor;
            button.inactiveTextColor = inActiveTextColor;
        }

        foreach (var kvp in mainButtons)
        {
            kvp.Key.Do(button =>
            {
                FormatButtonColor(button, kvp.Value.Item1, kvp.Value.Item2, kvp.Value.Item3, kvp.Value.Item4, kvp.Value.Item5);
            });
        
        }
        try
        {
            mainButtons?.Keys?.Flatten()?.DoIf(x => x != null, x => x.buttonText.color = Color.white);
        }
        catch { }


        if (!(ModStamp = GameObject.Find("ModStamp"))) return;
        ModStamp.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        FinalSuspect_Xtreme_Background = new GameObject("FinalSuspect_Xtreme Background");
        FinalSuspect_Xtreme_Background.transform.position = new Vector3(0, 0, 520f);
        var bgRenderer = FinalSuspect_Xtreme_Background.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = Utils.LoadSprite("FinalSuspect_Xtreme-BG.jpg", 179f);

        if (!(Ambience = GameObject.Find("Ambience"))) return;
        if (!(Starfield = Ambience.transform.FindChild("starfield").gameObject)) return;
        StarGen starGen = Starfield.GetComponent<StarGen>();
        starGen.SetDirection(new Vector2(0, -2));
        Starfield.transform.SetParent(FinalSuspect_Xtreme_Background.transform);
        Object.Destroy(Ambience);

        if (!(LeftPanel = GameObject.Find("LeftPanel"))) return;
        LeftPanel.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        static void ResetParent(GameObject obj) => obj.transform.SetParent(LeftPanel.transform.parent);
        LeftPanel.ForEachChild((Il2CppSystem.Action<GameObject>)ResetParent);
        LeftPanel.SetActive(false);

        GameObject.Find("Divider")?.SetActive(false);

        if (!(RightPanel = GameObject.Find("RightPanel"))) return;
        var rpap = RightPanel.GetComponent<AspectPosition>();
        if (rpap) Object.Destroy(rpap);
        RightPanelOp = RightPanel.transform.localPosition;
        RightPanel.transform.localPosition = RightPanelOp + new Vector3(10f, 0f, 0f);
        RightPanel.GetComponent<SpriteRenderer>().color = new(1f, 0.78f, 0.9f, 1f);

        CloseRightButton = new GameObject("CloseRightPanelButton");
        CloseRightButton.transform.SetParent(RightPanel.transform);
        CloseRightButton.transform.localPosition = new Vector3(-4.78f, 1.3f, 1f);
        CloseRightButton.transform.localScale = new(1f, 1f, 1f);
        CloseRightButton.AddComponent<BoxCollider2D>().size = new(0.6f, 1.5f);
        var closeRightSpriteRenderer = CloseRightButton.AddComponent<SpriteRenderer>();
        closeRightSpriteRenderer.sprite = Utils.LoadSprite("RightPanelCloseButton.png", 100f);
        closeRightSpriteRenderer.color = new(1f, 0.78f, 0.9f, 1f);
        var closeRightPassiveButton = CloseRightButton.AddComponent<PassiveButton>();
        closeRightPassiveButton.OnClick = new();
        closeRightPassiveButton.OnClick.AddListener((System.Action)MainMenuManagerPatch.HideRightPanel);
        closeRightPassiveButton.OnMouseOut = new();
        closeRightPassiveButton.OnMouseOut.AddListener((System.Action)(() => closeRightSpriteRenderer.color = new(1f, 0.78f, 0.9f, 1f)));
        closeRightPassiveButton.OnMouseOver = new();
        closeRightPassiveButton.OnMouseOver.AddListener((System.Action)(() => closeRightSpriteRenderer.color = new(1f, 0.68f, 0.99f, 1f)));

        Tint = __instance.screenTint.gameObject;
        var ttap = Tint.GetComponent<AspectPosition>();
        if (ttap) Object.Destroy(ttap);
        Tint.transform.SetParent(RightPanel.transform);
        Tint.transform.localPosition = new Vector3(-0.0824f, 0.0513f, Tint.transform.localPosition.z);
        Tint.transform.localScale = new Vector3(1f, 1f, 1f);

        var creditsScreen = __instance.creditsScreen;
        if (creditsScreen)
        {
            var csto = creditsScreen.GetComponent<TransitionOpen>();
            if (csto) Object.Destroy(csto);
            var closeButton = creditsScreen.transform.FindChild("CloseButton");
            closeButton?.gameObject.SetActive(false);
        }

        if (!(Sizer = GameObject.Find("Sizer"))) return;
        if (!(AULogo = GameObject.Find("LOGO-AU"))) return;
        Sizer.transform.localPosition += new Vector3(0f, 0.12f, 0f);
        AULogo.transform.localScale = new Vector3(0.66f, 0.67f, 1f);
        AULogo.transform.position += new Vector3(0f, 0.1f, 0f);
        var logoRenderer = AULogo.GetComponent<SpriteRenderer>();
        logoRenderer.sprite = Utils.LoadSprite("FinalSuspect_Xtreme-Logo.png");

        if (!(BottomButtonBounds = GameObject.Find("BottomButtonBounds"))) return;
        BottomButtonBounds.transform.localPosition -= new Vector3(0f, 0.1f, 0f);

    }


}
[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
internal class ModManagerLateUpdatePatch
{
    public static void Prefix(ModManager __instance)
    {
        __instance.ShowModStamp();

        LateTask.Update(Time.deltaTime);
    }
    public static void Postfix(ModManager __instance)
    {
        var offset_y = HudManager.InstanceExists ? 1.6f : 0.9f;
        __instance.ModStamp.transform.position = AspectPosition.ComputeWorldPosition(
            __instance.localCamera, AspectPosition.EdgeAlignments.RightTop,
            new Vector3(0.4f, offset_y, __instance.localCamera.nearClipPlane + 0.1f));
    }
}
[HarmonyPatch(typeof(CreditsScreenPopUp))]
internal class CreditsScreenPopUpPatch
{
    [HarmonyPatch(nameof(CreditsScreenPopUp.OnEnable))]
    public static void Postfix(CreditsScreenPopUp __instance)
    {
        __instance.BackButton.transform.parent.FindChild("Background").gameObject.SetActive(false);
    }
}
