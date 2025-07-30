using System;
using FinalSuspect.Helpers;
using UnityEngine;
using UnityEngine.UI;
using static FinalSuspect.Modules.Features.DisplayedRoleTag.DisplayerRoleTagHelper;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(MeetingHud))]
public class MeetingButtonManager
{
    private static int Count;
    private static bool ButtonCreated;

    private static void ClearMeetingButton(MeetingHud __instance)
        => __instance.playerStates.ToList().ForEach(x =>
        {
            if (x.transform.FindChild("Custom Meeting Button") != null)
                UnityEngine.Object.Destroy(x.transform.FindChild("Custom Meeting Button").gameObject);
        });

    [HarmonyPatch(nameof(MeetingHud.Start)), HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void Start(MeetingHud __instance)
    {
        textTemplate = UnityEngine.Object.Instantiate(__instance.playerStates[0]!.NameText);
        textTemplate.enabled = false;

        ButtonCreated = false;
        CreateMeetingButton(__instance);
    }

    [HarmonyPatch(nameof(MeetingHud.Update)), HarmonyPostfix, HarmonyPriority(Priority.LowerThanNormal)]
    public static void Update(MeetingHud __instance)
    {
        if (__instance == null || !IsInGame || __instance.IsDestroyedOrNull()) return;

        Count = Count > 20 ? 0 : ++Count;
        if (Count != 0) return;

        if (!IsInMeeting && __instance.lastSecond < 1)
        {
            if (GameObject.Find("Custom Meeting Button") != null) ClearMeetingButton(__instance);
            return;
        }

        if (!ButtonCreated)
        {
            CreateMeetingButton(__instance);
        }
    }

    private static void CreateMeetingButton(MeetingHud __instance)
    {
        foreach (var pva in __instance.playerStates)
        {
            var pc = GetPlayerById(pva.TargetPlayerId);
            if (pc == null) continue;
            var template = pva.Buttons.transform.Find("CancelButton").gameObject;
            var targetBox = UnityEngine.Object.Instantiate(template, pva.transform);
            targetBox.name = "Custom Meeting Button";
            targetBox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.31f);

            var highlight = targetBox.transform.FindChild("ControllerHighlight").gameObject;
            highlight.GetComponent<SpriteRenderer>().color =
                ColorHelper.FSColor;
            highlight.transform.localPosition = new Vector3(0f, 0f, 1.4f);

            var renderer = targetBox.GetComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite("EditTag.png", 115f);
            var button = targetBox.GetComponent<PassiveButton>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener((Action)(() => { ShowSelectionPanel(__instance, pc); }));
        }

        ButtonCreated = true;
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy)), HarmonyPostfix]
    public static void OnDestroy()
    {
        if (textTemplate != null && textTemplate.gameObject != null)
            UnityEngine.Object.Destroy(textTemplate.gameObject);
    }
}