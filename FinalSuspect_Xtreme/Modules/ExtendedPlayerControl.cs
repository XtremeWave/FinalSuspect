using AmongUs.GameOptions;
using InnerNet;
using System.Linq;
using UnityEngine;


namespace FinalSuspect_Xtreme;

static class ExtendedPlayerControl
{
    public static ClientData GetClient(this PlayerControl player)
    {
        try
        {
            var client = AmongUsClient.Instance.allClients.ToArray().Where(cd => cd.Character.PlayerId == player.PlayerId).FirstOrDefault();
            return client;
        }
        catch
        {
            return null;
        }
    }
    public static int GetClientId(this PlayerControl player)
    {
        if (player == null) return -1;
        var client = player.GetClient();
        return client == null ? -1 : client.Id;
    }
    public static RoleTypes GetRoleType(this PlayerControl player)
    {
        if (player != null)
        {
            return GetRoleType(player.PlayerId);
        }
        return RoleTypes.Crewmate;
    }
    public static RoleTypes GetRoleType(byte id)
    {
        return XtremeGameData.XtremePlayerData.GetRoleById(id);
    }
    public static bool IsImpostor(this PlayerControl pc)
    {
        if (!XtremeGameData.GameStates.IsInGame) return false;
        switch (pc.GetRoleType())
        {
            case RoleTypes.Impostor:
            case RoleTypes.Shapeshifter:
            case RoleTypes.Phantom:
            case RoleTypes.ImpostorGhost:
                return true;
        }
        return false;
    }
    public static string GetNameWithRole(this PlayerControl player, bool forUser = false)
    {
        var ret = $"{player?.Data?.PlayerName}" + (XtremeGameData.GameStates.IsInGame? $"({Utils.GetRoleName(player.GetRoleType())})" : "");
        return (forUser ? ret : ret.RemoveHtmlTags());
    }
    public static Color GetRoleColor(this PlayerControl player)
    {
        return Utils.GetRoleColor(player.GetRoleType());
    }
    public static string GetRealName(this PlayerControl player, bool isMeeting = false)
    {
        return isMeeting ? player?.Data?.PlayerName : player?.name;
    }
    public static bool IsLocalPlayer(this PlayerControl player) => PlayerControl.LocalPlayer == player;
    public static void GetLobbyText(this PlayerControl player, ref string nametext, out string colorstr)
    {
        colorstr = "#ffffff";
        if (!XtremeGameData.GameStates.IsLobby) return;
        if (XtremeGameData.PlayerVersion.playerVersion.TryGetValue(player.PlayerId, out var ver) && ver != null)
        {
            if (Main.ForkId != ver.forkId)
            {
                nametext = $"<size=1.5>{ver.forkId}</size>\n{nametext}";
                colorstr = "#BFFFB9";
            }
            else if (Main.version.CompareTo(ver.version) == 0)
            {
                var currectbranch = ver.tag == $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})";
                nametext = currectbranch
                    ? nametext : $"<size=1.5>{ver.tag}</size>\n{nametext}";
                colorstr = currectbranch ? "#B1FFE7" : "#ffff00";
            }
            else
            {
                nametext = $"<size=1.5>v{ver.version}</size>\n{nametext}";
                colorstr = "#ff0000";
            }
        }
        else
        {
            colorstr = player.IsLocalPlayer() ? "#B1FFE7" : "#E1E0B3";
        }
    }
    public static bool GetGameText(
        this PlayerControl player,
        out string colorstr,
        out bool appendText,
        out string roleText)
    {
        colorstr = default;
        appendText = false;
        roleText = "";

        if (!XtremeGameData.GameStates.IsInGame) return false;
        if (XtremeGameData.GameStates.OtherModHost) return false;

        var roleType = player.GetRoleType();
        colorstr = "#ffffff";

        if (Utils.CanSeeOthersRole(player, out bool bothImp))
        {
            appendText = true;
            colorstr = Utils.GetRoleColorCode(roleType);
        }
        else if (bothImp) colorstr = "#ff1919";

        if (appendText)
            roleText = $"<color={colorstr}><size=80%>{Translator.GetRoleString(roleType.ToString())}</size></color> {Utils.GetProgressText(player)}";

        return true;
    }
        
}