using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using TMPro;
using UnityEngine;

namespace FinalSuspect.DataHandling;
[HarmonyPatch]
public static class XtremeLocalHandling
{
    public static string CheckAndGetNameWithDetails(
        this PlayerControl player, 
        out Color color, 
        out string headertext,
        bool headderswap = false)
    {
        var name = player.GetDataName() ?? player.GetRealName();
        color = Color.white;
        headertext = "";
        player.GetLobbyText(ref color, ref headertext);
        player.GetGameText(ref color, ref headertext, headderswap);
        SpamManager.CheckSpam(ref name);
        if (FAC.SetNameNum[player.PlayerId] > 3)
        {
            color = ColorHelper.FaultColor;
        }
        return name;
    }
    public static void GetLobbyText(this PlayerControl player, ref Color color, ref string headertext)
    {
        if (!XtremeGameData.GameStates.IsLobby) return;
        if (XtremeGameData.PlayerVersion.playerVersion.TryGetValue(player.PlayerId, out var ver) && ver != null)
        {
            if (Main.ForkId != ver.forkId)
            {
                headertext = $"<size=1.5>{ver.forkId}</size>";
                color = ColorHelper.UnmatchedColor;
            }
            else if (Main.version.CompareTo(ver.version) == 0 &&
                     ver.tag == $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})")
            {
                color = ColorHelper.ModColor32;
            }
            else if (Main.version.CompareTo(ver.version) == 0 &&
                     ver.tag != $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})")
            {
                headertext = $"<size=1.5>{ver.tag}</size>";
                color = Color.yellow;
            }
            else
            {
                headertext = $"<size=1.5>v{ver.version}</size>";
                color = ColorHelper.FaultColor;
            }
        }
        else
        {
            if (player.IsLocalPlayer()) color = ColorHelper.ModColor32;
            else if (player.IsHost())
            {
                headertext = GetString("Host");
                color = ColorHelper.HostNameColor;
            }
            else color = ColorHelper.ClientlessColor;
        }
    }
    public static void GetGameText(this PlayerControl player, ref Color color, ref string roleText ,bool headderswap)
    {
        if (!XtremeGameData.GameStates.IsInGame) return;
        if (!Main.EnableFinalSuspect.Value) return;
        var roleType = player.GetRoleType();
        
        if (Utils.CanSeeTargetRole(player, out bool bothImp))
        {
            color = Utils.GetRoleColor(roleType);
            if (!headderswap)
                roleText = $"<size=80%>{GetRoleString(roleType.ToString())}</size> {Utils.GetProgressText(player)} {Utils.GetVitalText(player.PlayerId)}";
            else
                roleText = $"{Utils.GetVitalText(player.PlayerId)} {Utils.GetProgressText(player)} <size=80%>{GetRoleString(roleType.ToString())}</size>";
        }
        else if (bothImp)
        {
            color = Palette.ImpostorRed;
        }
        if (player.GetPlayerData().IsDisconnected)
            color = Color.gray;
    }
    #region FixedUpdate
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate)), HarmonyPostfix]
    public static void OnFixedUpdate(PlayerControl __instance)
    {
        Main.EnableFinalSuspect.Value = !XtremeGameData.GameStates.OtherModHost;

        if (__instance == null) return;

        try
        {
            var name = __instance.CheckAndGetNameWithDetails(out Color color, out string headertext);
            if (Main.EnableFinalSuspect.Value)
            { 
                DisconnectSync(__instance);
                DeathSync(__instance);
            }
            
            
            var HeaderTextTransform = __instance.cosmetics.nameText.transform.Find("RoleText");
            var HeaderText = HeaderTextTransform.GetComponent<TextMeshPro>();
            HeaderText.enabled = true;
            HeaderText.text = headertext;
            HeaderText.color = color;
            HeaderText.transform.SetLocalY(0.2f);

            __instance.cosmetics.nameText.text = name;
            __instance.cosmetics.nameText.color = color;
        }
        catch
        {
            var create = __instance.GetRealName() == null && XtremeGameData.GameStates.IsFreePlay ||
                         __instance.GetRealName() != "Player(Clone)";
            if (create)
                XtremePlayerData.CreateDataFor(__instance);
        }
    }
    static void DisconnectSync(PlayerControl pc)
    {
        if (!XtremeGameData.GameStates.IsInTask || XtremeGameData.GameStates.IsFreePlay) return;
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

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.First)]
    public static void OnMeetingStart(MeetingHud __instance)
    {
        if (!Main.EnableFinalSuspect.Value) return;
        foreach (var pva in __instance.playerStates)
        {
            try
            {
                pva.ColorBlindName.transform.localPosition -= new Vector3(1.35f, 0f, 0f);
                var pc = Utils.GetPlayerById(pva.TargetPlayerId);

                var name = pc.CheckAndGetNameWithDetails(out Color color, out string headertext);
            
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

                if (headertext.Length > 0)
                {
                    roleTextMeeting.text = headertext;
                    roleTextMeeting.color = color;
                    roleTextMeeting.enabled = true;
                }
            }
            catch 
            {
                
            }
            
        }
    }


    #endregion

    #region HUD

    public static void SetVentOutlineColor(Vent __instance, ref bool mainTarget)
    {
        if (!Main.EnableFinalSuspect.Value) return;
        Color color = PlayerControl.LocalPlayer.GetRoleColor();
        __instance.myRend.material.SetColor("_OutlineColor", color);
        __instance.myRend.material.SetColor("_AddColor", mainTarget ? color : Color.clear);
    }

    #endregion
    public static void ShowMap(MapBehaviour map, bool normal)
    {
        if (!Main.EnableFinalSuspect.Value) return;
        var roleType = PlayerControl.LocalPlayer.Data.Role.Role;
        var color = normal? Utils.GetRoleColor(roleType): Palette.DisabledGrey;
        if (Main.EnableMapBackGround.Value)
            map.ColorControl.SetColor(color);
    }

    public static bool GetHauntFilterText(HauntMenuMinigame __instance)
    {
        if (!Main.EnableFinalSuspect.Value) return false;
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

    public static void GetChatBubbleText(byte playerId, ref string name, ref Color32 bgcolor,
        ref Color namecolor)
    {
        if (!Main.EnableFinalSuspect.Value)
        {
            namecolor = Color.white;
            return;
        }
        var player = Utils.GetPlayerById(playerId);
        name = player.CheckAndGetNameWithDetails(out namecolor, out string headertext, player.IsLocalPlayer());
        if (player.IsLocalPlayer())
            name = $"<size=60%>{headertext}</size>  " + name;
        else
            name += $"  <size=60%>{headertext}</size>";
        
        if (!player.IsAlive())
            bgcolor = new Color32(255, 0, 0, 120);
    }
    

}