using FinalSuspect.ClientActions.FeatureItems.MainMenuStyle;
using FinalSuspect.Helpers;
using Il2CppSystem;
using UnityEngine;

namespace FinalSuspect.Patches.System;

[HarmonyPatch]
public class MainMenuButtonHoverAnimation
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Start_Postfix(MainMenuManager __instance)
    {
        var mainButtons = GameObject.Find("Main Buttons");

        mainButtons.ForEachChild((Action<GameObject>)Init);
    }

    private static void SetButtonStatus(GameObject obj, bool active)
    {
        ModMainMenuManager.AllButtons.TryAdd(obj, (obj.transform.position, active));
        ModMainMenuManager.AllButtons[obj] = (ModMainMenuManager.AllButtons[obj].Item1, active);
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
    [HarmonyPostfix]
    private static void Update_Postfix(MainMenuManager __instance)
    {
        if (!GameObject.Find("MainUI")) return;

        var style = MainMenuStyleManager.MainMenuStyles[Main.CurrentStyleId.Value];
        FormatButtonColor(__instance, __instance.newsButton,
            !ModNewsHistory.AnnouncementLoadComplete
                ? ColorHelper.ConvertToLightGray(style.MainUIColors[1])
                : style.MainUIColors[1], new Color(0f, 0f, 0f, 0f), Color.white, Color.white);

        __instance.newsButton.enabled = ModNewsHistory.AnnouncementLoadComplete;
        foreach (var (button, value) in ModMainMenuManager.AllButtons.Where(x => x.Key != null && x.Key.active))
        {
            var pos = button.transform.position;
            var targetPos = value.Item1 + new Vector3(value.Item2 ? 0.35f : 0f, 0f, 0f);
            if (value.Item2 && pos.x > value.Item1.x + 0.2f) continue;
            button.transform.position = value.Item2
                ? Vector3.Lerp(pos, targetPos, Time.deltaTime * 2f)
                : Vector3.MoveTowards(pos, targetPos, Time.deltaTime * 2f);
        }
    }

    public static void RefreshButtons(GameObject obj)
    {
        ModMainMenuManager.AllButtons = new Dictionary<GameObject, (Vector3, bool)>();
        obj.ForEachChild((Action<GameObject>)Init);
    }

    private static void Init(GameObject obj)
    {
        if (obj.name is "BottomButtonBounds" or "Divider") return;
        if (ModMainMenuManager.AllButtons.ContainsKey(obj)) return;
        SetButtonStatus(obj, false);
        var pb = obj.GetComponent<PassiveButton>();
        pb.OnMouseOver.AddListener((global::System.Action)(() => SetButtonStatus(obj, true)));
        pb.OnMouseOut.AddListener((global::System.Action)(() => SetButtonStatus(obj, false)));
    }
}