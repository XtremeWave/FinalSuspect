using System.Text;
using FinalSuspect.ClientActions;
using FinalSuspect.ClientActions.FeatureItems.MainMenuStyle;
using FinalSuspect.ClientActions.FeatureItems.MyMusic;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Resources;
using FinalSuspect.Patches.Game_Vanilla;
using FinalSuspect.Templates;
using Il2CppSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static FinalSuspect.Modules.Core.Plugin.ModMainMenuManager;
using ColorHelper = FinalSuspect.Helpers.ColorHelper;
using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
internal class PingTrackerUpdatePatch
{
    private static float _deltaTime;
    public static string ServerName = "";
    private static TextMeshPro _creditTextCredential;
    private static AspectPosition _creditTextCredentialAspectPos;

    public static void Postfix(PingTracker __instance)
    {
        if (!_creditTextCredential)
        {
            var uselessPingTracker = Object.Instantiate(__instance, __instance.transform.parent);
            _creditTextCredential = uselessPingTracker.GetComponent<TextMeshPro>();
            Object.Destroy(uselessPingTracker);
            _creditTextCredential.alignment = TextAlignmentOptions.TopRight;
            _creditTextCredential.color = new Color(1f, 1f, 1f, 0.7f);
            _creditTextCredential.rectTransform.pivot = new Vector2(1f, 1f); // 将中心点设定在右上角
            _creditTextCredentialAspectPos = _creditTextCredential.GetComponent<AspectPosition>();
            _creditTextCredentialAspectPos.Alignment = AspectPosition.EdgeAlignments.RightTop;
        }

        if (_creditTextCredentialAspectPos)
            _creditTextCredentialAspectPos.DistanceFromEdge =
                DestroyableSingleton<HudManager>.InstanceExists &&
                DestroyableSingleton<HudManager>.Instance.Chat.chatButton.gameObject.active
                    ? new Vector3(2.5f, 0f, -800f)
                    : new Vector3(1.8f, 0f, -800f);

        StringBuilder sb = new();

        sb.Append(Main.CredentialsText);

        _creditTextCredential.text = sb.ToString();
        if (
            (GameSettingMenu.Instance?.gameObject.active ?? false)
            || IsInMeeting
            || (FriendsListUI.Instance?.gameObject.active ?? false)
            || ((HudManagerPatch.showHideButton?.Button?.gameObject.active ?? false) && Main.ShowResults.Value))
            _creditTextCredential.text = "";

        var ping = AmongUsClient.Instance.Ping;
        var color = ping switch
        {
            < 50 => "#44dfcc",
            < 100 => "#7bc690",
            < 200 => "#f3920e",
            < 400 => "#ff146e",
            _ => "#ff4500"
        };

        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        var fps = Mathf.Ceil(1.0f / _deltaTime);

        __instance.text.alignment = TextAlignmentOptions.TopGeoAligned;
        __instance.text.text =
            $"<color={color}>{GetString("Ping")}:{ping} <size=60%>ms</size></color>" + "  "
            + $"<color=#00a4ff>{GetString("FrameRate")}:{fps} <size=60%>FPS</size></color>" +
            $"{"    <color=#FFDCB1>◈</color>" + (IsOnlineGame ? ServerName : GetString("Local"))}";
    }
}

[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
public class VersionShowerStartPatch
{
    public static GameObject OVersionShower;
    public static TextMeshPro VisitText;
    public static TextMeshPro CreditTextCredential;
    public static GameObject ModLogo;
    public static GameObject AuthorLogo;

    private static VersionShower _instance;

    public static void Postfix(VersionShower __instance)
    {
        TMPTemplate.SetBase(__instance.text);

        Main.CredentialsText =
            $"\r\n<size=120%>" +
            $"<color={ColorHelper.AuthorColorHex}>==</color> " +
            $"<color={ColorHelper.FSColorHex}>{Main.ModName}</color> " +
            $"<color={ColorHelper.AuthorColorHex}>==</color>"
            + "</size>";
        Main.CredentialsText += "\r\n <color=#fffcbe> By </color><color=#cdfffd>Slok</color></size>";
        Main.CredentialsText += $"\r\n<color=#C8FF78>v{Main.DisplayedVersion}</color>";

#if !DEBUG
        var additionalCredentials = GetString("TextBelowVersionText");
        if (additionalCredentials != null && additionalCredentials != "*" && additionalCredentials != "")
        {
            Main.CredentialsText += $"\r\n{additionalCredentials}";
        }
#endif
#if !RELEASE
        Main.CredentialsText += $"\r\n<color={ColorHelper.FSColorHex}>{Main.GitBranch}</color> - {Main.GitCommit}";
#endif

        if (Main.IsAprilFools)
        {
            Main.CredentialsText =
                $"\r\n<size=120%>" +
                $"<color=#fffcbe>==</color> " +
                $"<color=#C791F5>Feline Susspekt</color> " +
                $"<color=#fffcbe>==</color>"
                + "</size>";
            Main.CredentialsText += "\r\n <color=#cdffdd> By </color><color=#fffcbe>XtremeWives</color></size>";
            Main.CredentialsText += "\r\n <color=#ff0000>4.1.Never Gonna Give You Up</color>";
        }

        ErrorText.Create(__instance.text);
        if (Main.hasArgumentException && ErrorText.Instance)
            ErrorText.Instance.AddError(ErrorCode.Main_DictionaryError);

        if ((OVersionShower = GameObject.Find("VersionShower")) && !VisitText) CreateVisitText(__instance);

        if ((OVersionShower = GameObject.Find("VersionShower")) && !CreditTextCredential)
        {
            var credentialsText = string.Format(GetString("MainMenuCredential"),
                $"<color={ColorHelper.AuthorColorHex}>Slok</color>");
            credentialsText += "\n";
#if DEBUG
            var versionText = $"<color={ColorHelper.FSColorHex}>{Main.GitBranch}</color> - {Main.GitCommit}";
#elif RELEASE
            var versionText = 
                $"<color={ColorHelper.FSColorHex}>FS</color> - <color=#C8FF78>v{Main.DisplayedVersion}</color>";
#elif OPENBETA
            var versionText =
                $"<color={ColorHelper.FSColorHex}>{Main.GitBranch}</color> - {Main.GitCommit}\n" +
                $"<color={ColorHelper.FSColorHex}>FS</color> - <color=#C8FF78>v{Main.DisplayedVersion}</color>";
#endif

            credentialsText += versionText;

            if (Main.IsAprilFools)
            {
                credentialsText =
                    "<color=#fffcbe>XtremeWives © 1987</color>\n<color=#ff0000>4.1.Never Gonna Give You Up</color>";
            }

            CreditTextCredential = Object.Instantiate(__instance.text);
            CreditTextCredential.name = "FinalSuspect CreditText";
            CreditTextCredential.alignment = TextAlignmentOptions.Right;
            CreditTextCredential.text = credentialsText;
            CreditTextCredential.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);

            CreditTextCredential.enabled = GameObject.Find("FinalSuspect Background");
            CreditTextCredential.SetOutlineColor(ColorHelper.ShadeColor(ColorHelper.FSColor, 0.75f));
            CreditTextCredential.SetOutlineThickness(0.20f);
            CreditTextCredential.fontStyle = FontStyles.Bold;
            var ap_credit = CreditTextCredential.gameObject.AddComponent<AspectPosition>();
            ap_credit.Alignment = AspectPosition.EdgeAlignments.RightBottom;
            ap_credit.DistanceFromEdge = new Vector3(5f, 0.4f);
            ap_credit.updateAlways = true;
        }

        AuthorLogo = new GameObject
        {
            layer = 5,
            name = "Author Logo"
        };
        AuthorLogo.AddComponent<SpriteRenderer>().sprite = LoadSprite("AuthorLogo2.png", 840f);
        AuthorLogo.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 120);
        AuthorLogo.transform.SetParent(VisitText.transform.parent);
        var ap_authorLogo = AuthorLogo.gameObject.AddComponent<AspectPosition>();
        ap_authorLogo.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        ap_authorLogo.DistanceFromEdge = new Vector3(0.6f, 0.5f);
        ap_authorLogo.updateAlways = true;

        AuthorLogo.SetActive(false);
        ModLogo = new GameObject
        {
            layer = 5,
            name = "Mod Logo"
        };
        ModLogo.AddComponent<SpriteRenderer>().sprite = LoadSprite("FinalSuspect-Logo.png", 250f);
        ModLogo.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 120);
        var ap_modLogo = ModLogo.gameObject.AddComponent<AspectPosition>();
        ap_modLogo.Alignment = AspectPosition.EdgeAlignments.RightBottom;
        ap_modLogo.DistanceFromEdge = new Vector3(1.6f, 0.4f);
        ap_modLogo.updateAlways = true;
        ModLogo.SetActive(false);
    }

    public static void CreateVisitText(VersionShower __instance)
    {
        if (!__instance)
            __instance = _instance;
        else
            _instance = __instance;

        VisitText = Object.Instantiate(__instance.text);
        VisitText.name = "FinalSuspect VisitText";
        VisitText.alignment = TextAlignmentOptions.Left;
        VisitText.text = VersionChecker.IsChecked
            ? string.Format(GetString("FinalSuspectWelcomeText"), ColorHelper.FSColorHex)
            : GetString("RetrieveVersionInfoFailed");
        VisitText.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        VisitText.enabled = GameObject.Find("FinalSuspect Background");

        __instance.text.alignment = TextAlignmentOptions.Left;
        var ap1 = OVersionShower.GetComponent<AspectPosition>();
        ap1.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        ap1.DistanceFromEdge = new Vector3(0.4f, -0.3f);
        ap1.updateAlways = true;

        var ap2 = VisitText.gameObject.AddComponent<AspectPosition>();
        ap2.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        ap2.DistanceFromEdge = new Vector3(1.4f, 0.1f);
        ap2.updateAlways = true;
    }
}

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
[HarmonyPriority(Priority.First)]
internal class TitleLogoPatch
{
    public static void Postfix(MainMenuManager __instance)
    {
        GameObject.Find("BackgroundTexture")?.SetActive(!ShowedBak);

        Color shade = new(0f, 0f, 0f, 0f);
        var standardActiveSprite = __instance.newsButton.activeSprites.GetComponent<SpriteRenderer>().sprite;
        var minorActiveSprite = __instance.quitButton.activeSprites.GetComponent<SpriteRenderer>().sprite;
        var style = MainMenuStyleManager.MainMenuStyles[Main.CurrentStyleId.Value];

        var friendsButton = FriendsButton.GetComponent<PassiveButton>();
        Dictionary<List<PassiveButton>, (Sprite, Color, Color, Color, Color)> mainButtons = new()
        {
            {
                [__instance.playButton, __instance.inventoryButton, __instance.shopButton],
                (standardActiveSprite, style.MainUIColors[0], shade, Color.white, Color.white)
            },
            {
                [__instance.newsButton, __instance.myAccountButton, __instance.settingsButton],
                (minorActiveSprite, style.MainUIColors[1], shade, Color.white, Color.white)
            },
            {
                [__instance.creditsButton, __instance.quitButton],
                (minorActiveSprite, style.MainUIColors[2], shade, Color.white, Color.white)
            },
            {
                [friendsButton],
                (minorActiveSprite, style.MainUIColors[3], shade, Color.white, Color.white)
            }
        };

        foreach (var kvp in mainButtons)
            kvp.Key.Do(button =>
            {
                FormatButtonColor(__instance, button, kvp.Value.Item2, kvp.Value.Item3, kvp.Value.Item4,
                    kvp.Value.Item5);
            });

        try
        {
            mainButtons.Keys.Flatten()?.DoIf(x => x, x => x.buttonText.color = Color.white);
        }
        catch
        {
            /* ignored */
        }

        if (!(ModStamp = GameObject.Find("ModStamp"))) return;
        ModStamp.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        ModStamp.GetComponent<SpriteRenderer>().sprite = LoadSprite("ModStamp.png", 100f);

        FinalSuspect_Background = new GameObject("FinalSuspect Background")
        {
            transform =
            {
                position = new Vector3(0, 0, 520f)
            }
        };

        var bgRenderer = FinalSuspect_Background.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = style.Sprite;


        if (!(Ambience = GameObject.Find("Ambience"))) return;
        if (!(Starfield = Ambience.transform.FindChild("starfield").gameObject)) return;
        Starfield.SetActive(style.StarFieldActive);
        var starGen = Starfield.GetComponent<StarGen>();
        starGen.SetDirection(new Vector2(0, style.StarGenDire));
        Starfield.transform.SetParent(FinalSuspect_Background.transform);
        Object.Destroy(Ambience);

        if (!(LeftPanel = GameObject.Find("LeftPanel"))) return;
        LeftPanel.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        LeftPanel.ForEachChild((Action<GameObject>)ResetParent);
        LeftPanel.SetActive(false);

        GameObject.Find("Divider")?.SetActive(false);

        if (!(RightPanel = GameObject.Find("RightPanel"))) return;
        var rightPanelAP = RightPanel.GetComponent<AspectPosition>();
        if (rightPanelAP) Object.Destroy(rightPanelAP);
        RightPanel.transform.localPosition = RightPanelOp + new Vector3(10f, 0f, 0f);
        RightPanel.GetComponent<SpriteRenderer>().color = new Color(1f, 0.78f, 0.9f, 1f);

        CloseRightButton = new GameObject("CloseRightPanelButton");
        CloseRightButton.transform.SetParent(RightPanel.transform);
        CloseRightButton.transform.localPosition = new Vector3(-4.78f * GetResolutionOffset(), 1.3f, 1f);
        CloseRightButton.transform.localScale = new Vector3(1f, 1f, 1f);
        CloseRightButton.AddComponent<BoxCollider2D>().size = new Vector2(0.6f, 1.5f);
        var closeRightSpriteRenderer = CloseRightButton.AddComponent<SpriteRenderer>();
        closeRightSpriteRenderer.sprite = LoadSprite("RightPanelCloseButton.png", 100f);
        closeRightSpriteRenderer.color = new Color(1f, 0.78f, 0.9f, 1f);
        var closeRightPassiveButton = CloseRightButton.AddComponent<PassiveButton>();
        closeRightPassiveButton.OnClick = new Button.ButtonClickedEvent();
        closeRightPassiveButton.OnClick.AddListener((global::System.Action)MainMenuManagerPatch.HideRightPanel);
        closeRightPassiveButton.OnMouseOut = new UnityEvent();
        closeRightPassiveButton.OnMouseOut.AddListener((global::System.Action)(() =>
            closeRightSpriteRenderer.color = new Color(1f, 0.78f, 0.9f, 1f)));
        closeRightPassiveButton.OnMouseOver = new UnityEvent();
        closeRightPassiveButton.OnMouseOver.AddListener((global::System.Action)(() =>
            closeRightSpriteRenderer.color = new Color(1f, 0.68f, 0.99f, 1f)));

        Tint = __instance.screenTint.gameObject;
        var ttap = Tint.GetComponent<AspectPosition>();
        if (ttap) Object.Destroy(ttap);
        Tint.transform.SetParent(RightPanel.transform);
        Tint.transform.localPosition =
            new Vector3(-0.0824f * GetResolutionOffset(), 0.0513f, Tint.transform.localPosition.z);
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
        Sizer.transform.localPosition =
            new Vector3(-4.0f * GetResolutionOffset(), 1.4f, -1.0f);
        AULogo.transform.localScale = new Vector3(0.66f, 0.67f, 1f);
        AULogo.transform.position += new Vector3(0f, 0.1f, 0f);
        var logoRenderer = AULogo.GetComponent<SpriteRenderer>();
        logoRenderer.sprite = LoadSprite("FinalSuspect-Logo.png");

        if (!(BottomButtonBounds = GameObject.Find("BottomButtonBounds"))) return;
        BottomButtonBounds.transform.localPosition -= new Vector3(0f, 0.1f, 0f);

        var mainButtonsobj = GameObject.Find("Main Buttons");
        mainButtonsobj.transform.position = new Vector3(-3.4f * GetResolutionOffset(),
            mainButtonsobj.transform.position.y, mainButtonsobj.transform.position.z);

        if (!(BackgroundTexture = GameObject.Find("BackgroundTexture"))) return;
        BackgroundTexture.GetComponent<SpriteRenderer>().color = style.MainUIColors[0].SetAlpha(1);

        return;

        static void ResetParent(GameObject obj)
        {
            obj.transform.SetParent(LeftPanel.transform.parent);
        }
    }
}

[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
internal class ModManagerLateUpdatePatch
{
    private static bool _firstRun;
    private static string _lastScene = "";


    public static void Prefix(ModManager __instance)
    {
        __instance.ShowModStamp();
        if (_firstRun)
        {
            if (_lastScene != SceneManager.GetActiveScene().name)
            {
                var last = _lastScene;
                _lastScene = SceneManager.GetActiveScene().name;
                if (last is "SplashIntro") return;
                OnSceneChange(_lastScene);
            }
        }
        else
        {
            OptionsMenuBehaviourStartPatch.SetCursor();
            __instance.ModStamp.sprite = LoadSprite("ModStamp.png", 100f);
            _firstRun = true;
        }

        LateTask.Update(Time.deltaTime);
        MainThreadTask.Update();
    }

    public static void Postfix(ModManager __instance)
    {
        var offset_y = HudManager.InstanceExists ? 1.6f : 0.9f;
        __instance.ModStamp.transform.position = AspectPosition.ComputeWorldPosition(
            __instance.localCamera, AspectPosition.EdgeAlignments.RightTop,
            new Vector3(0.4f, offset_y, __instance.localCamera.nearClipPlane + 0.1f));
    }

    private static void OnSceneChange(string name)
    {
        if (name is not ("MainMenu" or "MatchMaking")) return;
        var style = MainMenuStyleManager.MainMenuStyles[Main.CurrentStyleId.Value];
        var audio = FinalMusic.Musics.FirstOrDefault(x => x.CurrentAudio == style.MainMenuMusic);
        if (audio != null)
        {
            _ = new LateTask(() => { AudioPlayer.Play(audio, true); }, 0.01f, "Play Custom MainBG");
        }
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

[HarmonyPatch(typeof(ResolutionManager))]
internal class ResolutionManagerPatch
{
    [HarmonyPatch(nameof(ResolutionManager.SetResolution))]
    public static void Postfix(int width, int height)
    {
        _ = new LateTask(() =>
        {
            if (!GameObject.Find("MainUI")) return;
            var offset = GetResolutionOffset();
            CloseRightButton.transform.localPosition = new Vector3(-4.78f * offset, 1.3f, 1.0f);
            Tint.transform.localPosition =
                new Vector3(-0.0824f * offset, 0.0513f, Tint.transform.localPosition.z);
            Sizer.transform.localPosition = new Vector3(-4.0f * offset, 1.4f, -1.0f);
            var mainButtons = GameObject.Find("Main Buttons");
            mainButtons.transform.position = new Vector3(-3.4f * offset, mainButtons.transform.position.y,
                mainButtons.transform.position.z);
            MainMenuButtonHoverAnimation.RefreshButtons(mainButtons);

            List<GameObject> nullObj = [];
            foreach (var button in MainMenuCustomButtons)
            {
                if (!button)
                {
                    nullObj.Add(button);
                    continue;
                }

                var scale = Instance.quitButton.transform.localScale;
                button.transform.localScale =
                    new Vector3(scale.x * GetResolutionOffset(), button.transform.localScale.y);
            }

            foreach (var obj in nullObj) MainMenuCustomButtons.Remove(obj);

            CloseRightButton.transform.localPosition =
                new Vector3(-4.78f * GetResolutionOffset(), 1.3f, 1f);
        }, 0.01f, "RefreshMenu");
    }
}