using System.Threading.Tasks;
using FinalSuspect.DataHandling.FinalGameData;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using TMPro;
using UnityEngine;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(IntroCutscene))]
internal class IntroCutscenePatch
{
    [HarmonyPatch(nameof(IntroCutscene.CoBegin))]
    [HarmonyPrefix]
    public static void CoBegin_Prefix()
    {
        UpdateGameState(true, StateTypes.InGame);
        Info("Game Start", "IntroCutscene");
    }

    [HarmonyPatch(typeof(IntroCutscene._ShowRole_d__41), nameof(IntroCutscene._ShowRole_d__41.MoveNext))]
    [HarmonyPostfix]
    public static void Postfix(IntroCutscene._ShowRole_d__41 __instance, ref bool __result)
    {
        if (!Main.EnableFinalSuspect.Value) return;
        var intro = __instance.__4__this;

        _ = new MainThreadTask(() =>
        {
            var roleType = PlayerControl.LocalPlayer.Data.Role.Role;
            intro.YouAreText.color =
                intro.RoleText.color =
                    intro.RoleBlurbText.color = GetRoleColor(roleType);
            intro.RoleText.text = GetRoleName(roleType);
            intro.RoleText.fontWeight = FontWeight.Thin;
            intro.RoleText.SetOutlineColor(GetRoleColor(roleType).ShadeColor(0.1f).SetAlpha(0.38f));
            intro.RoleText.SetOutlineThickness(0.17f);
            intro.RoleBlurbText.text = roleType.GetRoleInfoForVanilla();
        }, "Override Role Text");
    }

    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor))]
    [HarmonyPostfix]
    public static void BeginImpostor_Postfix(IntroCutscene __instance)
    {
        if (!Main.EnableFinalSuspect.Value) return;

        __instance.ImpostorText.gameObject.SetActive(true);

        var impostorCount = GameManager.Instance.LogicOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount);
        var onlyImp = impostorCount == 1;

        var impColor = Palette.ImpostorRed;
        var teamTitleText = GetString(onlyImp ? "Team.Imp_Only" : "Team.Imp");
        var introText = GetString(onlyImp ? "IntroText.Imp_Only" : "IntroText.Imp");

        __instance.TeamTitle.text = teamTitleText;
        __instance.TeamTitle.color = impColor;

        __instance.ImpostorText.text = onlyImp
            ? $"{GetString("ImpostorNum.Imp_Only")}\n{introText}"
            : $"{string.Format(GetString("ImpostorNum.Imp"), impostorCount)}\n{introText}";

        __instance.BackgroundBar.material.color = Palette.DisabledGrey;
        StartFadeIntro(__instance, Palette.DisabledGrey, impColor);
    }

    [HarmonyPatch(nameof(IntroCutscene.BeginCrewmate))]
    [HarmonyPostfix]
    public static void BeginCrewmate_Postfix(IntroCutscene __instance)
    {
        if (!Main.EnableFinalSuspect.Value) return;

        __instance.TeamTitle.text = $"{GetString("Team.Crew")}";
        __instance.ImpostorText.text =
            $"{string.Format(GetString("ImpostorNum.Crew"), GameManager.Instance.LogicOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount))}";
        __instance.ImpostorText.text += "\n" + GetString("IntroText.Crewmate");
        __instance.TeamTitle.color = new Color32(140, 255, 255, byte.MaxValue);

        StartFadeIntro(__instance, new Color32(140, 255, 255, byte.MaxValue), PlayerControl.LocalPlayer.GetRoleColor());


        if (!Input.GetKey(KeyCode.RightControl)) return;
        __instance.TeamTitle.text = "警告";
        __instance.ImpostorText.gameObject.SetActive(true);
        __instance.ImpostorText.text = "请远离无知的玩家";
        __instance.TeamTitle.color = Color.magenta;
        StartFadeIntro(__instance, Color.red, Color.magenta);
    }

    private static async void StartFadeIntro(IntroCutscene __instance, Color start, Color end)
    {
        try
        {
            await Task.Delay(1000);
            var milliseconds = 0;
            while (true)
            {
                await Task.Delay(20);
                milliseconds += 20;
                var time = milliseconds / (float)500;
                var lerpingColor = Color.Lerp(start, end, time);
                if (!__instance || milliseconds > 500)
                {
                    Info("break", "StartFadeIntro");
                    break;
                }

                __instance.BackgroundBar.material.color = lerpingColor;
            }
        }
        catch
        {
            /* ignored */
        }
    }
}