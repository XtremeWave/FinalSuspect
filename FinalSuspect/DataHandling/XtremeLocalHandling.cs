using System;
using System.Linq;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Features.CheckingandBlocking;
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
        return CheckAndGetNameWithDetails(player.PlayerId, out topcolor, out bottomcolor, out toptext, out bottomtext, topswap);
    }
    public static string CheckAndGetNameWithDetails(
        byte id, 
        out Color topcolor, 
        out Color bottomcolor, 
        out string toptext,
        out string bottomtext,
        bool topswap = false)
    {
        var data = XtremePlayerData.GetXtremeDataById(id);
        var player = data.Player;
        var name = XtremeGameData.GameStates.IsInTask 
            ? player.GetRealName() 
            : data.Name ?? player.GetRealName();
        topcolor = Color.white;
        bottomcolor = Color.white;
        toptext = "";
        bottomtext = "";
        data.GetLobbyText(ref topcolor, ref bottomcolor, ref toptext, ref bottomtext);
        data.GetGameText(ref topcolor, ref toptext, topswap);
        SpamManager.CheckSpam(ref name);
        if (!player.GetCheatData().IsSuspectCheater || Main.DisableFAC.Value) return name;
        topcolor = ColorHelper.FaultColor;
        toptext = toptext.CheckAndAppendText(GetString("Cheater"));
        return name;
    }

    private static void GetLobbyText(this XtremePlayerData data, ref Color topcolor, ref Color bottomcolor, ref string toptext, ref string bottomtext)
    {
        if (!XtremeGameData.GameStates.IsLobby) return;
        var player = data.Player;

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
                topcolor = Color.red;
            }
        }
        else
        {
            if (player.IsLocalPlayer()) topcolor = ColorHelper.ModColor32;
            else if (player.IsHost()) topcolor = ColorHelper.HostNameColor;
            else topcolor = ColorHelper.ClientlessColor;
        }

        if (Main.ShowPlayerInfo.Value)
        {
            bottomtext = bottomtext.CheckAndAppendText($"{player.GetPlatform()} {player.GetClient().FriendCode}");
            bottomcolor = ColorHelper.DownloadYellow;
        }
       
    }

    private static string GetPlatform(this PlayerControl player)
    {
        try
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
        catch
        {
            return "";
        }
        
    }

    private static void GetGameText(this XtremePlayerData data, ref Color color, ref string roleText ,bool topswap)
    {
        if (!XtremeGameData.GameStates.IsInGame) return;
        if (!Main.EnableFinalSuspect.Value) return;
        
        var roleType = XtremePlayerData.GetRoleById(data.PlayerId);
        var player = data.Player;

        if (Utils.CanSeeTargetRole(player, out var bothImp))
        {
            color = Utils.GetRoleColor(roleType);
            if (!topswap)
                roleText = $"<size=80%>{GetRoleString(roleType.ToString())}</size> {Utils.GetProgressText(player)} {Utils.GetVitalText(player.PlayerId, docolor:Utils.CanSeeOthersRole())}";
            else
                roleText = $"{Utils.GetVitalText(player.PlayerId, docolor:Utils.CanSeeOthersRole())} {Utils.GetProgressText(player)} <size=80%>{GetRoleString(roleType.ToString())}</size>";
        }
        else if (bothImp)
        {
            color = Palette.ImpostorRed;
        }

        if (player.GetXtremeData().IsDisconnected)
        {
            color = Color.gray;
        }
            
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
            var name = __instance.CheckAndGetNameWithDetails(out var topcolor, out var bottomcolor, out var toptext, out var bottomtext);
            if (Main.EnableFinalSuspect.Value)
            { 
                DisconnectSync(__instance);
                DeathSync(__instance);
            }
            
            __instance.GetCheatData().HandleBan();
            __instance.GetCheatData().HandleLobbyPosition();
            __instance.GetCheatData().HandleSuspectCheater();
            
            var topTextTransform = __instance.cosmetics.nameText.transform.Find("TopText");
            var topText = topTextTransform.GetComponent<TextMeshPro>();
            topText.enabled = true;
            topText.text = toptext;
            topText.color = topcolor;
            topText.transform.SetLocalY(0.2f);
            
            var bottomTextTransform = __instance.cosmetics.nameText.transform.Find("BottomText");
            var bottomText = bottomTextTransform.GetComponent<TextMeshPro>();
            bottomText.enabled = true;
            bottomText.text = bottomtext;
            bottomText.color = bottomcolor;
            bottomText.transform.SetLocalY(-1.6f);
            bottomText.fontSize = 1.6f;

            __instance.cosmetics.nameText.text = name;
            __instance.cosmetics.nameText.color = topcolor;
        }
        catch
        {
            var create = (__instance.GetRealName() == null && XtremeGameData.GameStates.IsFreePlay ||
                         __instance.GetRealName() != "Player(Clone)") 
                         && XtremePlayerData.AllPlayerData.All(data => data.PlayerId != __instance.PlayerId);
            if (create)
                XtremePlayerData.CreateDataFor(__instance);
        }
    }

    private static void DisconnectSync(PlayerControl pc)
    {
        if (!XtremeGameData.GameStates.IsInTask || XtremeGameData.GameStates.IsFreePlay) return;
        var data = pc.GetXtremeData();
        var currectlyDisconnect = pc.Data.Disconnected && !data.IsDisconnected;
        var Task_NotAssgin = data.TotalTaskCount == 0 && !data.IsImpostor;
        var Role_NotAssgin = data.RoleWhenAlive == null;

        if (pc.GetXtremeData().IsDisconnected)
        {
            pc.Data.Disconnected = true;
            pc.Data.IsDead = true;
        }
            
        if (!currectlyDisconnect && !Task_NotAssgin && !Role_NotAssgin) return;
        pc.SetDisconnected();
        pc.SetDeathReason(VanillaDeathReason.Disconnect, Task_NotAssgin || Role_NotAssgin);
    }

    private static void DeathSync(PlayerControl pc)
    {
        if (!XtremeGameData.GameStates.IsInTask || pc.GetXtremeData().IsDead) return;
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

                var name = CheckAndGetNameWithDetails(pva.TargetPlayerId, out var color, out _, out var toptext, out _);

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
            catch 
            { }

        }
    }
    
    
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.First)]
    public static void MeetingHudUpdate(MeetingHud __instance)
    {
        if (!Main.EnableFinalSuspect.Value) return;
        foreach (var pva in __instance.playerStates)
        {
            try
            {
                var name = CheckAndGetNameWithDetails(pva.TargetPlayerId, out var color, out _, out var toptext, out _);

                var roleTextMeetingTransform = pva.NameText.transform.Find("RoleTextMeeting");
                var roleTextMeeting = roleTextMeetingTransform.GetComponent<TextMeshPro>();

                pva.NameText.text = name;
                pva.NameText.color = color;

                if (toptext.Length > 0)
                {
                    roleTextMeeting.text = toptext;
                    roleTextMeeting.color = color;
                    roleTextMeeting.enabled = true;
                }
            }
            catch 
            { }

        }
    }


    #endregion

    #region HUD

    public static void SetVentOutlineColor(Vent __instance, ref bool mainTarget)
    {
        if (!Main.EnableFinalSuspect.Value) return;
        var color = PlayerControl.LocalPlayer.GetRoleColor();
        __instance.myRend.material.SetColor("_OutlineColor", color);
        __instance.myRend.material.SetColor("_AddColor", mainTarget ? color : Color.clear);
    }

    #endregion
    
    public static void ShowMap(MapBehaviour map, MapOptions opts)
    {
        if (!Main.EnableFinalSuspect.Value ) return;
        foreach (var data in XtremePlayerData.AllPlayerData)
        {
            if (data.IsDisconnected)
                data.rend.gameObject.SetActive(false);
            else
            {
                if (opts.Mode == MapOptions.Modes.CountOverlay)
                    data.rend.enabled = opts.ShowLivePlayerPosition;
                else
                {
                    data.Player.SetPlayerMaterialColors(data.rend);
                    data.rend.gameObject.SetActive(true);
                    UpdateMap();
                }
            }
            
        }
        
        if (!Main.EnableMapBackGround.Value) return;
        var roleType = PlayerControl.LocalPlayer.Data.Role.Role;
        var color = Utils.GetRoleColor(roleType);
        var mode = opts.Mode;
        switch (mode)
        {
            case MapOptions.Modes.CountOverlay:
                color = Palette.AcceptedGreen;
                break;
            case MapOptions.Modes.Sabotage:
                color = Palette.DisabledGrey;
                break;
        }

        map.ColorControl.SetColor(color);

    }
    public static void UpdateMap()
    {
        if (!Main.EnableFinalSuspect.Value) return;
        foreach (var data in XtremePlayerData.AllPlayerData)
        {
            var player = data.Player;
            if (data.deadbodyrend)
                data.deadbodyrend.gameObject.SetActive(Utils.CanSeeTargetRole(player, out _));
            if (data.IsDisconnected || !Utils.CanSeeTargetRole(player, out _) || player.IsLocalPlayer())
            {
                data.rend.gameObject.SetActive(false);
                continue;
            }
            if (data.IsDead)
                data.rend.color = Color.white.AlphaMultiplied(0.6f);
           
            var vector = player.transform.position;
            if (MeetingHud.Instance && data.preMeetingPosition != null)
            {
                vector = data.preMeetingPosition.Value;
            }
            else if (data.preMeetingPosition != null)
            {
               data.preMeetingPosition = null;
            }

            vector /= ShipStatus.Instance.MapScale;
            vector.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
            vector.z = -1f;
            data.rend.transform.localPosition = vector;
            data.rend.gameObject.SetActive(true);
        }
        
    }

    
    public static bool GetHauntFilterText(HauntMenuMinigame __instance)
    {
        if (!Main.EnableFinalSuspect.Value) return true;
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
        try
        {
            if (!Main.EnableFinalSuspect.Value)
            {
                namecolor = Color.white;
                return;
            }
            var player = Utils.GetPlayerById(playerId);
            name = player.CheckAndGetNameWithDetails(out namecolor, out _,  out var toptext, out _, player.IsLocalPlayer());
            toptext = toptext.Replace("\n", " ");
            if (player.IsLocalPlayer())
                name = $"<size=60%>{toptext}</size>  " + name;
            else
                name += $"  <size=60%>{toptext}</size>";
        
            if (!player.IsAlive())
                bgcolor = new Color32(255, 0, 0, 120);
        }
        catch 
        {
        }
       
    }
    

}