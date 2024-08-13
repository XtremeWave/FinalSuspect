using HarmonyLib;
using LibCpp2IL.MachO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalSuspect_Xtreme.Patches;

internal class ExilePatch
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    class BaseExileControllerPatch
    {
        public static void Prefix(ExileController __instance)
        {
            if (__instance.exiled == null || __instance.exiled.PlayerId < 0 || __instance.exiled.PlayerId > 14) return;
            try
            {
                XtremeGameData.PlayerData.GetPlayerDataById(__instance.exiled.PlayerId).Exiled = true;
                XtremeGameData.PlayerData.GetPlayerById(__instance.exiled.PlayerId).SetDeathReason(DataDeathReason.Exile);

            }
            catch { }
        }
    }
}
