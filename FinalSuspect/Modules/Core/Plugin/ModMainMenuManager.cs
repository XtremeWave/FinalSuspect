using UnityEngine;

namespace FinalSuspect.Modules.Core.Plugin;

public static class ModMainMenuManager
{
    public static MainMenuManager Instance;

    public static GameObject InviteButton;
    public static GameObject GithubButton;
    public static GameObject UpdateButton;
    public static GameObject PlayButton;
    public static GameObject FriendsButton;
    public static readonly List<GameObject> MainMenuCustomButtons = [];

    public static bool isOnline;
    public static bool ShowedBak;
    public static bool ShowingPanel;

    public static GameObject ModStamp;
    public static GameObject FinalSuspect_Background;
    public static GameObject Ambience;
    public static GameObject Starfield;
    public static GameObject LeftPanel;
    public static GameObject RightPanel;
    public static GameObject CloseRightButton;
    public static GameObject Tint;
    public static GameObject Sizer;
    public static GameObject AULogo;
    public static GameObject BottomButtonBounds;
    public static GameObject BackgroundTexture;

    public static Vector3 RightPanelOp = new(2.8f, -0.4f, -5.0f);
    public static Dictionary<GameObject, (Vector3, bool)> AllButtons = new();
    public static bool Active = true;
}