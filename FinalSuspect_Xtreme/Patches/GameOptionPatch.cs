using AmongUs.GameOptions;
using HarmonyLib;
using static FinalSuspect_Xtreme.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FinalSuspect_Xtreme.Patches;
[HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.UpdateValuesAndText))]
class RoleOptionSettingUpdateValuesAndTextPatch
{
    public static void Postfix(RoleOptionSetting __instance)
    {
        var rolecolor = Utils.GetRoleColor(__instance.Role.Role);
        __instance.labelSprite.color = Utils.ShadeColor(rolecolor, 0.2f);
        __instance.titleText.color = Color.white;
    }

}
