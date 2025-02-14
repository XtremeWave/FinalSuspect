using System.Threading.Tasks;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using Il2CppSystem.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(IntroCutscene))]
class IntroCutscenePatch
{
    [HarmonyPatch(nameof(IntroCutscene.ShowRole)), HarmonyPostfix]
    public static void ShowRole_Postfix(IntroCutscene __instance)
    {
        if (!Main.EnableRoleBackGround.Value) return;
        if (XtremeGameData.GameStates.OtherModHost) return;

        _ = new LateTask(() =>
        {
            var roleType = PlayerControl.LocalPlayer.Data.Role.Role;
            var cr = roleType;
            __instance.YouAreText.color = __instance.RoleText.color = __instance.RoleBlurbText.color = Utils.GetRoleColor(cr);
            __instance.RoleText.text = Utils.GetRoleName(cr);
            __instance.RoleText.fontWeight = FontWeight.Thin;
            __instance.RoleText.SetOutlineColor(Utils.GetRoleColor(cr).ShadeColor(0.1f).SetAlpha(0.38f));
            __instance.RoleText.SetOutlineThickness(0.17f);
            __instance.RoleBlurbText.text = cr.GetRoleInfoForVanilla();
        }, 0.0001f, "Override Role Text");

    }
    [HarmonyPatch(nameof(IntroCutscene.CoBegin)), HarmonyPrefix]
    public static void CoBegin_Prefix()
    {
        XtremeGameData.GameStates.InGame = true;
        XtremeLogger.Info("Game Start", "IntroCutscene");
    }
    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor)), HarmonyPostfix]
    public static void BeginImpostor_Postfix(IntroCutscene __instance, ref List<PlayerControl> yourTeam)
    {
        if (!Main.EnableRoleBackGround.Value) return;
        if (XtremeGameData.GameStates.OtherModHost) return;

        __instance.ImpostorText.gameObject.SetActive(true);
        var onlyimp = GameManager.Instance.LogicOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount) == 1;

        var color = Palette.ImpostorRed;
        var colorcode = onlyimp ? ColorHelper.ColorToHex(Palette.DisabledGrey) : ColorHelper.ColorToHex(Palette.ImpostorRed);
        __instance.TeamTitle.text = onlyimp 
            ? GetString("TeamImpostorOnly") 
            : GetString("TeamImpostor");

        __instance.TeamTitle.color = color;
        __instance.ImpostorText.text = $"<color=#{colorcode}>";
        __instance.ImpostorText.text += onlyimp
            ? GetString("ImpostorNumImpOnly")
            : $"{string.Format(GetString("ImpostorNumImp"), GameManager.Instance.LogicOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount))}";

        __instance.ImpostorText.text += "\n" + (onlyimp
            ? GetString("ImpostorIntroTextOnly")
            : GetString("ImpostorIntroText"));

        __instance.BackgroundBar.material.color = Palette.DisabledGrey;

        StartFadeIntro(__instance, Palette.DisabledGrey, Palette.ImpostorRed);
    }

    [HarmonyPatch(nameof(IntroCutscene.BeginCrewmate)), HarmonyPostfix]
    public static void BeginCrewmate_Postfix(IntroCutscene __instance, ref List<PlayerControl> teamToDisplay)
    {
        if (!Main.EnableRoleBackGround.Value) return;
        if (XtremeGameData.GameStates.OtherModHost) return;

        __instance.TeamTitle.text = $"{GetString("TeamCrewmate")}";
        __instance.ImpostorText.text = 
            $"{string.Format(GetString("ImpostorNumCrew"), GameManager.Instance.LogicOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount))}";
        __instance.ImpostorText.text += "\n" + GetString("CrewmateIntroText");
        __instance.TeamTitle.color = new Color32(140, 255, 255, byte.MaxValue);

        StartFadeIntro(__instance, new Color32(140, 255, 255, byte.MaxValue), PlayerControl.LocalPlayer.GetRoleColor());


        if (Input.GetKey(KeyCode.RightControl))
        {
            __instance.TeamTitle.text = "警告";
            __instance.ImpostorText.gameObject.SetActive(true);
            __instance.ImpostorText.text = "请远离无知的玩家";
            __instance.TeamTitle.color = Color.magenta;
            StartFadeIntro(__instance, Color.red, Color.magenta);
        }
    }

    private static async void StartFadeIntro(IntroCutscene __instance, Color start, Color end)
    {
        await Task.Delay(1000);
        var milliseconds = 0;
        while (true)
        {
            await Task.Delay(20);
            milliseconds += 20;
            var time = milliseconds / (float)500;
            var LerpingColor = Color.Lerp(start, end, time);
            if (__instance == null || milliseconds > 500)
            {
                XtremeLogger.Info("break", "StartFadeIntro");
                break;
            }
            __instance.BackgroundBar.material.color = LerpingColor;
        }
    }

}