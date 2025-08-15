using System;
using FinalSuspect.Helpers;
using UnityEngine;
using UnityEngine.UI;
using static FinalSuspect.Modules.Features.DisplayedRoleTag.DisplayerRoleTagHelper;
using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(MeetingHud))]
public class MeetingButtonManager
{
    private static int _count;
    private static bool _buttonCreated;

    private static void ClearMeetingButton(MeetingHud __instance)
        => __instance.playerStates.ToList().ForEach(x =>
        {
            if (x.transform.FindChild("Custom Meeting Button") != null)
                Object.Destroy(x.transform.FindChild("Custom Meeting Button").gameObject);
        });

    [HarmonyPatch(nameof(MeetingHud.Start)), HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void Start(MeetingHud __instance)
    {
        textTemplate = Object.Instantiate(__instance.playerStates[0]!.NameText);
        textTemplate.enabled = false;

        _buttonCreated = false;
        CreateMeetingButton(__instance);
    }

    [HarmonyPatch(nameof(MeetingHud.Update)), HarmonyPostfix, HarmonyPriority(Priority.LowerThanNormal)]
    public static void Update(MeetingHud __instance)
    {
        if (__instance == null || !IsInGame || __instance.IsDestroyedOrNull()) return;

        _count = _count > 20 ? 0 : ++_count;
        if (_count != 0) return;

        if (!IsInMeeting && __instance.lastSecond < 1)
        {
            if (GameObject.Find("Custom Meeting Button") != null) ClearMeetingButton(__instance);
            return;
        }

        if (!_buttonCreated)
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
            if (CanSeeTargetRole(pc, out _)) continue;
            var template = pva.Buttons.transform.Find("CancelButton").gameObject;
            var targetBox = Object.Instantiate(template, pva.transform);
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

        _buttonCreated = true;
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy)), HarmonyPostfix]
    public static void OnDestroy()
    {
        if (textTemplate != null && textTemplate.gameObject != null)
            Object.Destroy(textTemplate.gameObject);
    }
}