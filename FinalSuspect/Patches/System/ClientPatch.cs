using System;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using FinalSuspect.Modules.Resources;
using FinalSuspect.Patches.Game_Vanilla;
using InnerNet;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.MakePublic))]
internal class MakePublicPatch
{
    public static bool Prefix()
    {
        if (Main.OfflineMode.Value) return true;
        if (!VersionChecker.IsBroken && (!VersionChecker.HasUpdate || !VersionChecker.ForceUpdate) &&
            VersionChecker.IsSupported) return true;
        var message = "";
        if (VersionChecker.IsBroken) message = GetString("ModBrokenMessage");
        if (VersionChecker.HasUpdate) message = GetString("CanNotJoinPublicRoomNoLatest");
        Info(message, "MakePublicPatch");
        SendInGame(message);
        return false;
    }
}

[HarmonyPatch(typeof(MMOnlineManager), nameof(MMOnlineManager.Start))]
internal class MmOnlineManagerStartPatch
{
    public static void Postfix()
    {
        if (!(VersionChecker.HasUpdate || VersionChecker.IsBroken || !VersionChecker.IsSupported)) return;
        var obj = GameObject.Find("FindGameButton");
        if (!obj) return;
        obj.SetActive(false);
        _ = obj.transform.parent.gameObject;
        var textObj = Object.Instantiate(obj.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>());
        textObj.transform.position = new Vector3(0.5f, -0.4f, 0f);
        textObj.name = "CanNotJoinPublic";
        textObj.DestroyTranslator();
        var message = "";
        if (VersionChecker.HasUpdate)
            message = GetString("CanNotJoinPublicRoomNoLatest");
        else if (VersionChecker.IsBroken)
            message = GetString("ModBrokenMessage");
        else if (!VersionChecker.IsSupported) message = GetString("UnsupportedVersion");

        textObj.text = $"<size=2>{StringHelper.ColorString(Color.red, message)}</size>";
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsAllowedOnline))]
internal class RunLoginPatch
{
    public static void Prefix(ref bool canOnline)
    {
        // Ref: https://github.com/0xDrMoe/TownofHost-Enhanced/blob/main/Patches/ClientPatch.cs
        var friendCode = EOSManager.Instance?.friendCode;
        canOnline = !string.IsNullOrEmpty(friendCode) && !BanManager.CheckFACStatus(friendCode, null);

#if DEBUG
        // 如果您希望在调试版本公开您的房间，请仅用于测试用途
        // 如果您修改了代码，请在房间公告内表明这是修改版本，并给出修改作者
        // If you wish to make your lobby public in a debug build, please use it only for testing purposes
        // If you modify the code, please indicate in the lobby announcement that this is a modified version and provide the author of the modification
        canOnline = Environment.UserName is "Slok7" or "LezaiYa";
#endif
    }
}

[HarmonyPatch(typeof(BanMenu), nameof(BanMenu.SetVisible))]
internal class BanMenuSetVisiblePatch
{
    public static bool Prefix(BanMenu __instance, bool show)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        show &= PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data;
        __instance.BanButton.gameObject.SetActive(AmongUsClient.Instance.CanBan());
        __instance.KickButton.gameObject.SetActive(AmongUsClient.Instance.CanKick());
        __instance.MenuButton.gameObject.SetActive(show);
        return false;
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.CanBan))]
internal class InnerNetClientCanBanPatch
{
    public static bool Prefix(InnerNetClient __instance, ref bool __result)
    {
        __result = __instance.AmHost;
        return false;
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.KickPlayer))]
internal class KickPlayerPatch
{
    public static bool Prefix(int clientId, bool ban)
    {
        try
        {
            if (clientId == AmongUsClient.Instance.HostId) return false;
            if (Main.AllPlayerControls.Where(p => p.IsDev()).Any(p =>
                    AmongUsClient.Instance.GetRecentClient(clientId).FriendCode == p.FriendCode))
            {
                SendInGame(GetString("Warning.CantKickDev"));
                return false;
            }

            if (OnPlayerLeftPatch.ClientsProcessed.Contains(clientId)) return true;
            OnPlayerLeftPatch.Add(clientId);
            var color = Palette.PlayerColors[AmongUsClient.Instance.GetRecentClient(clientId).ColorId];
            var name = AmongUsClient.Instance.GetRecentClient(clientId).PlayerName;
            if (ban)
            {
                BanManager.AddBanPlayer(AmongUsClient.Instance.GetRecentClient(clientId));
                NotificationPopperPatch.NotificationPop(string.Format(GetString("Notification.PlayerBanByHost"),
                    StringHelper.ColorString(color, name)));
            }
            else
            {
                NotificationPopperPatch.NotificationPop(string.Format(GetString("Notification.PlayerKickByHost"),
                    StringHelper.ColorString(color, name)));
            }
        }
        catch
        {
            /* ignored */
        }

        return true;
    }
}