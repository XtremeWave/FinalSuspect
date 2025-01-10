using System;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using Sentry.Unity.NativeUtils;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalSuspect.DataHandling;
[HarmonyPatch]
public static class XtremeLocalHandling
{
    public static string CheckAndGetNameWithDetails(
        this PlayerControl player, 
        out Color topcolor, 
        out Color bottomcolor, 
        out string toptext,
        out string bottomtext,
        bool topswap = false)
    {
        var name = player.GetDataName() ?? player.GetRealName();
        topcolor = Color.white;
        bottomcolor = Color.white;
        toptext = "";
        bottomtext = "";
        player.GetLobbyText(ref topcolor, ref bottomcolor, ref toptext, ref bottomtext);
        player.GetGameText(ref topcolor, ref toptext, topswap);
        SpamManager.CheckSpam(ref name);
        if (FAC.SuspectCheater.Contains(player.PlayerId))
        {
            topcolor = ColorHelper.FaultColor;
            toptext.CheckAndAppendText(GetString("Cheater"));
        }
        return name;
    }
    public static void GetLobbyText(this PlayerControl player, ref Color topcolor, ref Color bottomcolor, ref string toptext, ref string bottomtext)
    {
        if (!XtremeGameData.GameStates.IsLobby) return;
        if (player.IsHost())
            toptext = toptext.CheckAndAppendText(GetString("Host"));
        if (XtremeGameData.PlayerVersion.playerVersion.TryGetValue(player.PlayerId, out var ver) && ver != null)
        {
            if (Main.ForkId != ver.forkId)
            {
                toptext = toptext.CheckAndAppendText($"<size=1.5>{ver.forkId}</size>");
                topcolor = ColorHelper.UnmatchedColor;
            }
            else if (Main.version.CompareTo(ver.version) == 0 &&
                     ver.tag == $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})")
            {

                topcolor = ColorHelper.ModColor32;
            }
            else if (Main.version.CompareTo(ver.version) == 0 &&
                     ver.tag != $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})")
            {
                toptext = toptext.CheckAndAppendText($"<size=1.5>{ver.tag}</size>");
                topcolor = Color.yellow;
            }
            else
            {
                toptext = toptext.CheckAndAppendText($"<size=1.5>v{ver.version}</size>");
                topcolor = ColorHelper.FaultColor;
            }
        }
        else
        {
            if (player.IsLocalPlayer()) topcolor = ColorHelper.ModColor32;
            else if (player.IsHost()) topcolor = ColorHelper.HostNameColor;
            else topcolor = ColorHelper.ClientlessColor;
        }
        bottomtext = bottomtext.CheckAndAppendText(player.GetPlatform());
    }

    public static string GetPlatform(this PlayerControl player)
    {
        var color = "";
        var name = "";
        string text;
        switch (player.GetClient().PlatformData.Platform)
        {
            case Platforms.StandaloneEpicPC:
                color = "#905CDA";
                name = "Itch";
                break;
            case Platforms.StandaloneSteamPC:
                color = "#4391CD";
                name = "Steam";
                break;
            case Platforms.StandaloneMac:
                color = "#e3e3e3";
                name = "Mac.";
                break;
            case Platforms.StandaloneWin10:
                color = "#FFF88D";
                name = "Win-10";
                break;
            case Platforms.StandaloneItch:
                color = "#E35F5F";
                name = "Itch";
                break;
            case Platforms.IPhone:
                color = "#e3e3e3";
                name = GetString("IPhone");
                break;
            case Platforms.Android:
                color = "#1EA21A";
                name = GetString("Android");
                break;
            case Platforms.Switch:
                name = "<color=#00B2FF>Nintendo</color><color=#ff0000>Switch</color>";
                break;
            case Platforms.Xbox:
                color = "#07ff00";
                name = "Xbox";
                break;
            case Platforms.Playstation:
                color = "#0014b4";
                name = "PlayStation";
                break;
        }

        if (color != "" && name != "")
            text = $"<color={color}>{name}</color>";
        else
            text = name;
        return text;
    }
    public static void GetGameText(this PlayerControl player, ref Color color, ref string roleText ,bool topswap)
    {
        if (!XtremeGameData.GameStates.IsInGame) return;
        if (!Main.EnableFinalSuspect.Value) return;
        var roleType = player.GetRoleType();
        if (Utils.CanSeeTargetRole(player, out bool bothImp))
        {
            color = Utils.GetRoleColor(roleType);
            if (!topswap)
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

    public static string CheckAndAppendText(this string toptext, string extratext)
    {
        if (toptext != "")
            toptext += " ";
        toptext += extratext;
        return toptext;
    }
    #region FixedUpdate
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate)), HarmonyPostfix]
    public static void OnFixedUpdate(PlayerControl __instance)
    {
        Main.EnableFinalSuspect.Value = !XtremeGameData.GameStates.OtherModHost;

        if (__instance == null) return;

        try
        {
            var name = __instance.CheckAndGetNameWithDetails(out Color topcolor, out Color bottomcolor, out string toptext, out string bottomtext);
            if (Main.EnableFinalSuspect.Value && XtremeGameData.GameStates.IsInGame)
            { 
                DisconnectSync(__instance);
                DeathSync(__instance);
            }
            
            
            var TopTextTransform = __instance.cosmetics.nameText.transform.Find("TopText");
            var TopText = TopTextTransform.GetComponent<TextMeshPro>();
            TopText.enabled = true;
            TopText.text = toptext;
            TopText.color = topcolor;
            TopText.transform.SetLocalY(0.2f);
            
            var BottomTextTransform = __instance.cosmetics.nameText.transform.Find("BottomText");
            var BottomText = BottomTextTransform.GetComponent<TextMeshPro>();
            BottomText.enabled = true;
            BottomText.text = bottomtext;
            BottomText.color = bottomcolor;
            BottomText.transform.SetLocalY(0.2f);

            __instance.cosmetics.nameText.text = name;
            __instance.cosmetics.nameText.color = topcolor;
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

                var name = pc.CheckAndGetNameWithDetails(out Color color, out _, out string toptext, out _);

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

                if (toptext.Length > 0)
                {
                    roleTextMeeting.text = toptext;
                    roleTextMeeting.color = color;
                    roleTextMeeting.enabled = true;
                }
            }
            catch (Exception e)
            {
                XtremeLogger.Test(e);
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
        name = player.CheckAndGetNameWithDetails(out namecolor, out _,  out string toptext, out _, player.IsLocalPlayer());
        toptext = toptext.Replace("\n", " ");
        if (player.IsLocalPlayer())
            name = $"<size=60%>{toptext}</size>  " + name;
        else
            name += $"  <size=60%>{toptext}</size>";
        
        if (!player.IsAlive())
            bgcolor = new Color32(255, 0, 0, 120);
    }
    

}