using AmongUs.Data;
using FinalSuspect.DataHandling.FinalGameData;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Modules.Core.Game.UI;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using InnerNet;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
public class OnGameJoinedPatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        LastResult.DestroyAll();
        Info($"{__instance.GameId} 加入房间", "OnGameJoined");
        FinalGameData.PlayerVersion.PlayerVersions = new Dictionary<int, FinalGameData.PlayerVersion>();
        FinalPlayerData.InitializeAll();
        UpdateGameState(false, StateTypes.InGame);
        UpdateGameState(false, StateTypes.InMeeting);
        ErrorText.Instance.Clear();
        ServerAddManager.SetServerName();
        FinalGameData.JoinedCompleted = false;
        Init_FAC();
        _ = new LateTask(() => { _ = RPC.RpcVersionCheck(); }, 0.5f, "SyncJoined");
        _ = new LateTask(() => { FinalGameData.JoinedCompleted = true; }, 4f, "SyncJoined");

        SoundManager.Instance.ChangeAmbienceVolume(DataManager.Settings.Audio.AmbienceVolume);
        //Main.NewLobby = true;
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
public class OnPlayerJoinedPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
    {
        Info($"{client.PlayerName}(ClientID:{client.Id}/FriendCode:{client.FriendCode}) 加入房间", "Session");

        BanManager.CheckFriendCode(client);
        BanManager.CheckBanPlayer(client);
        BanManager.CheckDenyNamePlayer(client);
        KickUnspawnedPlayers(client);
    }

    private static void KickUnspawnedPlayers(ClientData client)
    {
        _ = new LateTask(() =>
        {
            try
            {
                if (!AmHost || AmongUsClient.Instance.allClients.Contains(client) ||
                    !client.Character.Data.IsIncomplete) return;
                SendInGame(GetString("Warning.InvalidColor") +
                           $" {client.PlayerName}(ClientID:{client.Id}/FriendCode:{client.FriendCode})");
                AmongUsClient.Instance.KickPlayer(client.Id, false);
                Info(
                    $"Kicked {client.PlayerName}(ClientID:{client.Id}/FriendCode:{client.FriendCode}) due to it was unspawned",
                    "OnPlayerJoinedPatchPostfix");
            }
            catch
            {
                /* ignored */
            }
        }, 4.5f, "Kick Unspawned Players");
    }
}

