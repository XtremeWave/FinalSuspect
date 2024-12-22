using FinalSuspect.Modules.Managers;
using Il2CppSystem.Reflection;
using UnityEngine;

namespace FinalSuspect.Player;

public static class XtremeLocalHandling
{
    public static string CheckAndGetNameWithDetails(this PlayerControl player, out Color color, out string headdertext)
    {
        
        var name = player.GetRealName();
        color = Color.white;
        headdertext = "";
        player.GetLobbyText(ref color, ref headdertext);
        player.GetGameText(ref color, ref headdertext);
        SpamManager.CheckSpam(ref name);
        return name;
    }
    public static void GetLobbyText(this PlayerControl player, ref Color color, ref string headdertext)
    {
        if (!XtremeGameData.GameStates.IsLobby) return;
        if (XtremeGameData.PlayerVersion.playerVersion.TryGetValue(player.PlayerId, out var ver) && ver != null)
        {
            if (Main.ForkId != ver.forkId)
            {
                headdertext = $"<size=1.5>{ver.forkId}</size>";
                color = new Color32(191, 255, 185, 255);
            }
            else if (Main.version.CompareTo(ver.version) == 0 &&
                     ver.tag == $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})")
            {
                color = new Color32(177, 255, 231, 255);
            }
            else if (Main.version.CompareTo(ver.version) == 0 &&
                     ver.tag != $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})")
            {
                headdertext = $"<size=1.5>{ver.tag}</size>";
                color = Color.yellow;
            }
            else
            {
                headdertext = $"<size=1.5>v{ver.version}</size>";
                color = ColorHelper.FaultColor;
            }
        }
        else
        {
            color = player.IsLocalPlayer() ? new Color32(177, 255, 231, 255): new Color32(225, 224, 179, 255);
        }
    }
    public static void GetGameText(this PlayerControl player, ref Color color, ref string roleText)
    {
        if (!XtremeGameData.GameStates.IsInGame) return;
        if (XtremeGameData.GameStates.OtherModHost) return;

        var roleType = player.GetRoleType();
        
        if (Utils.CanSeeOthersRole(player, out bool bothImp))
        {
            color = bothImp ? Palette.ImpostorRed : Utils.GetRoleColor(roleType);
            roleText = $"<size=80%>{Translator.GetRoleString(roleType.ToString())}</size> {Utils.GetProgressText(player)} {Utils.GetVitalText(player.PlayerId)}";
        }
        if (player.GetPlayerData().IsDisconnected)
            color = Color.gray;
    }
    #region FixedUpdate

    public static void OnFixedUpdate(this PlayerControl player)
    {

        if (player == null) return;

        try
        {
            var name = player.CheckAndGetNameWithDetails(out Color color, out string headdertext);
            DisconnectSync(player);
            DeathSync(player);
            
            var HeadderTextTransform = player.cosmetics.nameText.transform.Find("RoleText");
            var HeadderText = HeadderTextTransform.GetComponent<TMPro.TextMeshPro>();
            HeadderText.enabled = true;
            HeadderText.text = headdertext;
            HeadderText.color = color;
            HeadderText.transform.SetLocalY(0.2f);

            player.cosmetics.nameText.text = name;
            player.cosmetics.nameText.color = color;
        }
        catch
        {
            XtremePlayerData.CreateDataFor(player);
        }

        

    }
    static void DisconnectSync(PlayerControl pc)
    {
        if (!XtremeGameData.GameStates.IsInTask) return;
        var data = pc.GetPlayerData();
        var currectlyDisconnect = pc.Data.Disconnected && !data.IsDisconnected;
        var Task_NotAssgin = data.TotalTaskCount == 0 && !data.IsImpostor;
        var Role_NotAssgin = data.RoleWhenAlive == null;

        if (currectlyDisconnect || Task_NotAssgin || Role_NotAssgin)
        {
            pc.SetDisconnected();
            pc.SetDeathReason(DataDeathReason.Disconnect, Task_NotAssgin || Role_NotAssgin);
        }
    }
    static void DeathSync(PlayerControl pc)
    {
        if (!XtremeGameData.GameStates.IsInTask || pc.GetPlayerData().IsDead) return;
        if (pc.Data.IsDead) pc.SetDead();
    }


    #endregion
    
    #region MeetingHud

    public static void OnMeetingStart(MeetingHud hud)
    {
        foreach (var pva in hud.playerStates)
        {
            pva.ColorBlindName.transform.localPosition -= new Vector3(1.35f, 0f, 0f);

            var pc = Utils.GetPlayerById(pva.TargetPlayerId);
            var name = pc.CheckAndGetNameWithDetails(out Color color, out string headdertext);
            
            var roleTextMeeting = Object.Instantiate(pva.NameText);
            roleTextMeeting.text = "";
            roleTextMeeting.enabled = false;
            roleTextMeeting.transform.SetParent(pva.NameText.transform);
            roleTextMeeting.transform.localPosition = new Vector3(0f, -0.18f, 0f);
            roleTextMeeting.fontSize = 1.5f;
            roleTextMeeting.gameObject.name = "RoleTextMeeting";
            roleTextMeeting.enableWordWrapping = false;


                
            pva.NameText.text = name;
            pva.NameText.color = color;

            if (headdertext.Length > 0)
            {
                roleTextMeeting.text = headdertext;
                roleTextMeeting.color = color;
                roleTextMeeting.enabled = true;
            }
        }
    }


    #endregion

    #region HUD

    public static void SetVentOutlineColor(Vent __instance, ref bool mainTarget)
    {
        Color color = PlayerControl.LocalPlayer.GetRoleColor();
        __instance.myRend.material.SetColor("_OutlineColor", color);
        __instance.myRend.material.SetColor("_AddColor", mainTarget ? color : Color.clear);
    }

    #endregion
    public static void ShowMap(MapBehaviour map, bool normal)
    {
        var roleType = PlayerControl.LocalPlayer.Data.Role.Role;
        var color = normal? Utils.GetRoleColor(roleType): Palette.DisabledGrey;
        if (Main.EnableMapBackGround.Value)
            map.ColorControl.SetColor(color);
    }

    public static bool GetHauntFilterText(HauntMenuMinigame __instance)
    {
        if (__instance.HauntTarget != null)
        {
            var role = __instance.HauntTarget.GetRoleType();
            var color = Utils.GetRoleColor(role);
            __instance.NameText.color = __instance.FilterText.color = color;
            __instance.FilterText.text = Utils.GetRoleName(role);
            return false;
        }
        return true;
    }

    public static void GetChatBubbleText(byte playerId, Color NAMETEXTCOLOR, ref string name, ref Color32 bgcolor,
        ref Color namecolor)
    {


        var player = Utils.GetPlayerById(playerId);
        name = player.CheckAndGetNameWithDetails(out namecolor, out string headdertext);

        if (!player.IsAlive())
            bgcolor = new Color32(255, 0, 0, 120);
        if (NAMETEXTCOLOR == Color.green)
        {
            bgcolor = ColorHelper.HalfYellow;
            namecolor = ColorHelper.TeamColor32;
        }
    }
    

}