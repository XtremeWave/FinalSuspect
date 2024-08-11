using HarmonyLib;
using System.Threading.Tasks;
using UnityEngine;
using static FinalSuspect_Xtreme.Translator;

namespace FinalSuspect_Xtreme;

[HarmonyPatch(typeof(IntroCutscene))]
class IntroCutscenePatch
{
    [HarmonyPatch(nameof(IntroCutscene.ShowRole)), HarmonyPostfix]
    public static void ShowRole_Postfix(IntroCutscene __instance)
    {
        if (!Main.EnableRoleBackGround.Value) return;

        _ = new LateTask(() =>
        {
            var roleType = PlayerControl.LocalPlayer.Data.Role.Role;
            var cr = roleType;
            __instance.YouAreText.color = Utils.GetRoleColor(cr);
            __instance.RoleText.text = Utils.GetRoleName(cr);
            __instance.RoleText.color = Utils.GetRoleColor(cr);
            __instance.RoleText.fontWeight = TMPro.FontWeight.Thin;
            __instance.RoleText.SetOutlineColor(Utils.ShadeColor(Utils.GetRoleColor(cr), 0.1f).SetAlpha(0.38f));
            __instance.RoleText.SetOutlineThickness(0.17f);
            __instance.RoleBlurbText.color = Utils.GetRoleColor(cr);
            __instance.RoleBlurbText.text = cr.GetRoleInfoForVanilla();

        }, 0.0001f, "Override Role Text");

    }
    [HarmonyPatch(nameof(IntroCutscene.CoBegin)), HarmonyPrefix]
    public static void CoBegin_Prefix()
    {
        GameStates.InGame = true;
    }
    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor)), HarmonyPostfix]
    public static void BeginImpostor_Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        if (!Main.EnableRoleBackGround.Value) return;
        if (Main.playerVersion.TryGetValue(0, out var ver) && Main.ForkId != ver.forkId) return;
        __instance.ImpostorText.gameObject.SetActive(true);
        var onlyimp = GameManager.Instance.LogicOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount) == 1;

        Color color = onlyimp ? Palette.DisabledGrey : new Color32(255, 25, 25, byte.MaxValue);
        string colorcode= onlyimp ? ColorHelper.ColorToHex(Palette.DisabledGrey) : "FF1919";
        __instance.TeamTitle.text = onlyimp?
            GetString("TeamImpostorOnly"):
            GetString("TeamImpostor");

        __instance.TeamTitle.color = color;
        __instance.ImpostorText.text = $"<color=#{colorcode}>";
        __instance.ImpostorText.text += onlyimp ? 
            GetString("ImpostorNumImpOnly"):
            $"{string.Format(GetString("ImpostorNumImp"), GameManager.Instance.LogicOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount))}";

        __instance.ImpostorText.text += "\n" + (onlyimp ?
            GetString("ImpostorIntroTextOnly"):
            GetString("ImpostorIntroText"));

        __instance.BackgroundBar.material.color = Palette.DisabledGrey;

        StartFadeIntro(__instance,  Palette.DisabledGrey, Palette.ImpostorRed);
    }

    [HarmonyPatch(nameof(IntroCutscene.BeginCrewmate)), HarmonyPostfix]
    public static void BeginCrewmate_Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
    {
        if (!Main.EnableRoleBackGround.Value) return;
        if (Main.playerVersion.TryGetValue(0, out var ver) && Main.ForkId != ver.forkId) return;

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
        int milliseconds = 0;
        while (true)
        {
            await Task.Delay(20);
            milliseconds += 20;
            float time = milliseconds / (float)500;
            Color LerpingColor = Color.Lerp(start, end, time);
            if (__instance == null || milliseconds > 500)
            {
                Logger.Info("ループを終了します", "StartFadeIntro");
                break;
            }
            __instance.BackgroundBar.material.color = LerpingColor;
        }
    }
    [HarmonyPatch(nameof(IntroCutscene.OnDestroy)), HarmonyPostfix]
    public static void OnDestroy_Postfix(IntroCutscene __instance)
    {
        Logger.Info("OnDestroy", "IntroCutscene");
    }
}