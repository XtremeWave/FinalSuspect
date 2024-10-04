using HarmonyLib;
using LibCpp2IL.MachO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalSuspect.Patches;

internal class ExilePatch
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    class BaseExileControllerPatch
    {
        public static void Prefix(ExileController __instance)
        {
            if (__instance.initData.networkedPlayer == null || 
                __instance.initData.networkedPlayer.PlayerId < 0 || 
                __instance.initData.networkedPlayer.PlayerId > 14) return;
            var player = XtremeGameData.XtremePlayerData.GetPlayerById(__instance.initData.networkedPlayer.PlayerId);
            player.SetDead();
            player.SetDeathReason(DataDeathReason.Exile, true);
        }
    }
}
