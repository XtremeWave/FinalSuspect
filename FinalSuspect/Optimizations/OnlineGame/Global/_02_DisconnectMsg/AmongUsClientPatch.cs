using FinalSuspect.DataHandling.FinalGameData;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using InnerNet;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(AmongUsClient))]

public class AmongUsClientPatch
{
    public static readonly List<int> ClientsProcessed = [];

    public static void Add(int id)
    {
        ClientsProcessed.Remove(id);
        ClientsProcessed.Add(id);
    }
    [HarmonyPatch(nameof(AmongUsClient.OnPlayerLeft))]
    [HarmonyPostfix]
    public static void OnPlayerLeft_Postfix([HarmonyArgument(0)] ClientData data, [HarmonyArgument(1)] DisconnectReasons reason)
    {
        try
        {
            if (data == null)
            {
                Error("错误的客户端数据：数据为空", "Session");
                return;
            }

            Info($"{data.PlayerName}(ClientID:{data.Id}/FriendCode:{data.FriendCode})" +
                 $"断开连接(理由:{reason}，Ping:{AmongUsClient.Instance.Ping})",
                "Session");
            var id = data.ColorId;
            var color = Palette.PlayerColors[id];
            var name = StringHelper.ColorString(color, data.PlayerName);

            // 附加描述掉线原因
            switch (reason)
            {
                case DisconnectReasons.Hacking:
                    NotificationPopperPatch.NotificationPop(
                        string.Format(GetString("Notification.PlayerLeftByAU-Anticheat"), name));
                    break;
                case DisconnectReasons.Error:
                    NotificationPopperPatch.NotificationPop(string.Format(GetString("Notification.PlayerLeftCuzError"),
                        name));
                    break;
                case DisconnectReasons.Kicked:
                case DisconnectReasons.Banned:
                    break;
                case DisconnectReasons.ClientTimeout:
                    NotificationPopperPatch.NotificationPop(
                        string.Format(GetString("Notification.PlayerLeftCuzTimeout"), name));
                    break;
                default:
                    if (!ClientsProcessed.Contains(data.Id))
                        NotificationPopperPatch.NotificationPop(string.Format(GetString("Notification.PlayerLeft"),
                            name));
                    break;
            }
            data.Character?.SetDisconnected();
            Dispose(data.Character?.PlayerId ?? 255);
           
            FinalGameData.PlayerVersion.PlayerVersions.Remove(data.Character?.GetClientId() ?? 0);

            ClientsProcessed.Remove(data.Id);
        }
        catch
        {
            /* ignored */
        }
    }
}