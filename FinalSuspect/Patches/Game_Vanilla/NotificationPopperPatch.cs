using FinalSuspect.Modules.Features.CheckingandBlocking;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(NotificationPopper), nameof(NotificationPopper.AddDisconnectMessage))]
public class NotificationPopperPatch
{
    private static readonly List<string> WaitToSend = [];

    public static bool Prefix(string item)
    {
        if (!WaitToSend.Contains(item)) return false;
        SpamManager.CheckSpam(ref item);
        WaitToSend.Remove(item);
        return true;
    }

    private static void AddItem(string text)
    {
        WaitToSend.Add(text);
        if (DestroyableSingleton<HudManager>._instance)
            DestroyableSingleton<HudManager>.Instance.Notifier.AddDisconnectMessage(text);
        else WaitToSend.Remove(text);
    }

    public static void NotificationPop(string text)
    {
        AddItem(text);
    }
}