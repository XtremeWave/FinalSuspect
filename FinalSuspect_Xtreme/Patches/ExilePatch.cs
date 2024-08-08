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
            try
            {
                if (__instance.exiled != null)
                {
                    GamePlayerData.GetPlayerDataById(__instance.exiled.PlayerId).Exiled = true;
                    GamePlayerData.GetPlayerById(__instance.exiled.PlayerId).SetDeathReason(DataDeathReason.Exile);
                }
            }
            catch { }
        }
    }
}
