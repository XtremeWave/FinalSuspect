using System.Collections.Generic;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using HarmonyLib;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(NotificationPopper), nameof(NotificationPopper.AddDisconnectMessage))]
public class NotificationPopperPatch
{
    private static List<string> WaitToSend = new();
    private static bool Prefix(NotificationPopper __instance, string item)
    {
        if (!WaitToSend.Contains(item)) return false;
        WaitToSend.Remove(item);
        return true;
    }
    public static void AddItem(string text)
    {
        SpamManager.CheckSpam(ref text);
        WaitToSend.Add(text);
        if (DestroyableSingleton<HudManager>._instance) DestroyableSingleton<HudManager>.Instance.Notifier.AddDisconnectMessage(text);
        else WaitToSend.Remove(text);
    }

    public static void NotificationPop(string text)
    {
        AddItem(text);
    }
}