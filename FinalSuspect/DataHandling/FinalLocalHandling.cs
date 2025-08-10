using FinalSuspect.ClientActions.FeatureItems.NameTag;
using FinalSuspect.DataHandling.FinalGameData;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalSuspect.DataHandling;

[HarmonyPatch]
public static class FinalLocalHandling
{
    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int AddColor = Shader.PropertyToID("_AddColor");

    public static string CheckAndGetNameWithDetails(
        byte id,
        out Color topcolor,
        out Color bottomcolor,
        out string toptext,
        out string bottomtext,
        bool topswap = false)
    {
        var data = GetFinalDataById(id);
        var player = data.Player;
        var name = IsInTask
            ? player.GetRealName()
            : data.Name ?? player.GetRealName();
        topcolor = Color.white;
        bottomcolor = Color.white;
        toptext = "";
        bottomtext = "";
        data.GetLobbyText(ref topcolor, ref bottomcolor, ref toptext, ref bottomtext);
        data.GetGameText(ref topcolor, ref toptext, topswap);
        SpamManager.CheckSpam(ref name);
        var ap = NameTagManager.ApplyFor(player).displayName;
        name += ap.RemoveHtmlTags() == "" ? "" : $" ({ap})";
        if (!player.GetCheatData().IsSuspectCheater && !player.GetCheatData().IsHacker ||
            !Main.EnableFAC.Value) return name;
        topcolor = ColorHelper.FaultColor;
        toptext = toptext.CheckAndAppendText(GetString("Id.Cheater"));
        return name;
    }

    private static void GetLobbyText(this FinalPlayerData data, ref Color topcolor, ref Color bottomcolor,
        ref string toptext, ref string bottomtext)
    {
        if (!IsLobby) return;
        var player = data.Player;
        if (player.IsHost()) toptext = toptext.CheckAndAppendText(GetString("Id.Host"));
        if (GetPlayerVersion(player.GetClientId(), out var ver))
        {
            if (Main.ForkId != ver.ForkId)
            {
                toptext = toptext.CheckAndAppendText($"<size=1.5>{ver.ForkId}</size>");
                topcolor = ColorHelper.UnmatchedColor;
            }
            else
            {
                switch (Main.version.CompareTo(ver.Version))
                {
                    case 0 when ver.Tag == $"{Main.GitCommit}({Main.GitBranch})":
                        topcolor = ColorHelper.FSColor;
                        break;
                    case 0 when ver.Tag != $"{Main.GitCommit}({Main.GitBranch})":
                        toptext = toptext.CheckAndAppendText($"<size=1.5>{ver.Tag}</size>");
                        topcolor = Color.yellow;
                        break;
                    default:
                        toptext = toptext.CheckAndAppendText($"<size=1.5>v{ver.Version}</size>");
                        topcolor = Color.red;
                        break;
                }
            }
        }
        else
        {
            if (player.IsLocalPlayer()) topcolor = ColorHelper.FSColor;
            else if (player.IsHost()) topcolor = ColorHelper.HostNameColor;
            else topcolor = ColorHelper.ClientlessColor;
        }

        if (!Main.ShowPlayerInfo.Value) return;
        bottomtext = bottomtext.CheckAndAppendText($"{player.GetPlatform()} {player.GetClient().FriendCode}");
        bottomcolor = ColorHelper.DownloadYellow;
    }


    private static void GetGameText(this FinalPlayerData data, ref Color color, ref string roleText, bool topswap)
    {
        if (!IsInGame) return;
        if (!Main.EnableFinalSuspect.Value) return;

        var roleType = GetRoleById(data.PlayerId);
        var player = data.Player;
        var roleTag = data.RoleTag;

        if (CanSeeTargetRole(player, out var bothImp))
        {
            color = GetRoleColor(roleType);
            roleText = !topswap
                ? $"<size=80%>{GetRoleString(roleType.ToString())}</size> {GetProgressText(player)} {GetVitalText(player.PlayerId, doColor: CanSeeOthersRole())} "
                : $"{GetVitalText(player.PlayerId, doColor: CanSeeOthersRole())} {GetProgressText(player)} <size=80%>{GetRoleString(roleType.ToString())}</size>";
        }
        else if (roleTag.TagColor != Color.white || roleTag.TagStr != "" || roleTag.Room != "")
        {
            color = data.RoleTag.TagColor;
            roleText = !topswap
                ? $"[<size=80%>{data.RoleTag.TagStr}" + StringHelper.ColorString(ColorHelper.ClientlessColor,
                    $"{data.RoleTag.Room}") + "</size>]"
                : "[<size=80%>" + StringHelper.ColorString(ColorHelper.ClientlessColor, $"{data.RoleTag.Room}") +
                  $"{data.RoleTag.TagStr}</size>]";
        }
        else if (bothImp)
        {
            color = Palette.ImpostorRed;
        }

        if (player.GetFinalData().IsDisconnected) color = Color.gray;
    }

    private static string CheckAndAppendText(this string toptext, string extratext)
    {
        if (toptext != "")
            toptext += "\n";
        toptext += extratext;
        return toptext;
    }

    #region HUD

    public static void SetVentOutlineColor(Vent __instance, ref bool mainTarget)
    {
        if (!Main.EnableFinalSuspect.Value) return;
        var color = PlayerControl.LocalPlayer.GetRoleColor();
        __instance.myRend.material.SetColor(OutlineColor, color);
        __instance.myRend.material.SetColor(AddColor, mainTarget ? color : Color.clear);
    }

    public static void ShowMap(MapBehaviour map, MapOptions opts)
    {
        if (!Main.EnableFinalSuspect.Value) return;
        foreach (var data in FinalPlayerData.AllPlayerData)
            if (data.IsDisconnected)
            {
                data.Rend.gameObject.SetActive(false);
            }
            else
            {
                if (opts.Mode == MapOptions.Modes.CountOverlay)
                {
                    data.Rend.enabled = opts.ShowLivePlayerPosition;
                }
                else
                {
                    data.Player.SetPlayerMaterialColors(data.Rend);
                    data.Player.SetPlayerMaterialColors(data.Rend_DeadBody);
                    data.Rend.gameObject.SetActive(true);
                    UpdateMap();
                }
            }

        var roleType = PlayerControl.LocalPlayer.Data.Role.Role;
        var color = GetRoleColor(roleType);
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
        foreach (var data in FinalPlayerData.AllPlayerData)
        {
            var player = data.Player;
            data.Rend_DeadBody?.gameObject.SetActive(CanSeeTargetRole(player, out _) &&
                                                     player.GetFinalData().RealDeathReason is VanillaDeathReason.Kill);
            if (data.IsDisconnected || !CanSeeTargetRole(player, out _) || player.IsLocalPlayer())
            {
                data.Rend.gameObject.SetActive(false);
                continue;
            }

            var vector = player.transform.position;
            if (MeetingHud.Instance && data.PreMeetingPosition != null)
                vector = data.PreMeetingPosition.Value;
            else if (data.PreMeetingPosition != null) data.PreMeetingPosition = null;

            vector /= ShipStatus.Instance.MapScale;
            vector.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
            vector.z = -1f;
            data.Rend.transform.localPosition = vector;
            data.Rend.gameObject.SetActive(true);

            if (data.IsDead)
                data.Rend.color = Color.white.AlphaMultiplied(0.6f);
            else if (data.Rend_DeadBody)
                data.Rend_DeadBody.transform.localPosition = vector;
        }
    }

    public static bool GetHauntFilterText(HauntMenuMinigame __instance)
    {
        if (!Main.EnableFinalSuspect.Value) return true;
        var role = __instance.HauntTarget.GetRoleType();
        var color = GetRoleColor(role);
        __instance.NameText.color = __instance.FilterText.color = color;
        __instance.FilterText.text = GetRoleName(role);
        return false;
    }

    public static void GetChatBubbleText(byte playerId, ref string name, ref Color32 bgcolor, ref Color namecolor)
    {
        try
        {
            if (!Main.EnableFinalSuspect.Value)
            {
                namecolor = Color.white;
                return;
            }

            var player = GetPlayerById(playerId);
            name = player.CheckAndGetNameWithDetails(out namecolor, out _, out var toptext, out _,
                player.IsLocalPlayer());
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
            /* ignored */
        }
    }

    #endregion

    #region FixedUpdate

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    public static void OnFixedUpdate(PlayerControl __instance)
    {
        Main.EnableFinalSuspect.Value = !OtherModHost;

        if (!__instance) return;

        try
        {
            var name = __instance.CheckAndGetNameWithDetails(out var topcolor, out var bottomcolor, out var toptext,
                out var bottomtext);
            if (Main.EnableFinalSuspect.Value)
            {
                DisconnectSync(__instance);
                DeathSync(__instance);
            }

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

            __instance.GetCheatData().HandleCheatData();
        }
        catch
        {
            var create = (
                             IsFreePlay || (__instance.GetRealName() != "Player(Clone)" && IsLobby)
                         )
                         && FinalPlayerData.AllPlayerData.All(data => data.PlayerId != __instance.PlayerId);
            if (create) FinalPlayerData.CreateDataFor(__instance);
        }
    }

    private static void DisconnectSync(PlayerControl pc)
    {
        if (!IsInTask || IsFreePlay) return;
        var data = pc.GetFinalData();
        var currectlyDisconnect = pc.Data.Disconnected && !data.IsDisconnected;
        var taskNotAssgin = data.TotalTaskCount == 0 && !data.IsImpostor;
        var roleNotAssgin = data.RoleWhenAlive == null;

        if (pc.GetFinalData().IsDisconnected)
        {
            pc.Data.Disconnected = true;
            pc.Data.IsDead = true;
        }

        if (!currectlyDisconnect && !taskNotAssgin && !roleNotAssgin) return;
        pc.SetDisconnected();
        pc.SetDeathReason(VanillaDeathReason.Disconnect, taskNotAssgin || roleNotAssgin);
    }

    private static void DeathSync(PlayerControl pc)
    {
        if (!IsInTask || pc.GetFinalData().IsDead) return;
        if (pc.Data.IsDead) pc.SetDead();
    }

    #endregion

    #region MeetingHud

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.HigherThanNormal)]
    public static void OnMeetingStart(MeetingHud __instance)
    {
        if (!Main.EnableFinalSuspect.Value) return;
        foreach (var pva in __instance.playerStates)
            try
            {
                pva.ColorBlindName.transform.localPosition -= new Vector3(1.35f, 0f, 0f);

                var name = CheckAndGetNameWithDetails(pva.TargetPlayerId, out var color, out _, out var toptext, out _);

                var roleTextMeeting = Object.Instantiate(pva.NameText, pva.NameText.transform, true);
                roleTextMeeting.text = "";
                roleTextMeeting.enabled = false;
                roleTextMeeting.transform.localPosition = new Vector3(0f, -0.18f, 0f);
                roleTextMeeting.fontSize = 1.5f;
                roleTextMeeting.gameObject.name = "RoleTextMeeting";
                roleTextMeeting.enableWordWrapping = false;

                pva.NameText.text = name;
                pva.NameText.color = color;

                roleTextMeeting.text = toptext;
                roleTextMeeting.color = color;
                roleTextMeeting.enabled = toptext.Length > 0;
            }
            catch
            {
                /* ignored */
            }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.First)]
    public static void MeetingHudUpdate(MeetingHud __instance)
    {
        if (!Main.EnableFinalSuspect.Value) return;
        foreach (var pva in __instance.playerStates)
            try
            {
                var name = CheckAndGetNameWithDetails(pva.TargetPlayerId, out var color, out _, out var toptext, out _);

                var roleTextMeetingTransform = pva.NameText.transform.Find("RoleTextMeeting");
                var roleTextMeeting = roleTextMeetingTransform.GetComponent<TextMeshPro>();

                pva.NameText.text = name;
                pva.NameText.color = color;

                roleTextMeeting.text = toptext;
                roleTextMeeting.color = color;
                roleTextMeeting.enabled = toptext.Length > 0;
            }
            catch
            {
                /* ignored */
            }
    }

    #endregion
}