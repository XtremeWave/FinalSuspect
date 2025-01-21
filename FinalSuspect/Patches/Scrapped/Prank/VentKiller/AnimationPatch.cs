/*using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FinalSuspect.Patches.Prank;


[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcEnterVent))]
internal static class RpcEnterVentPatch
{
    private static bool Prefix(PlayerPhysics __instance, [HarmonyArgument(0)]int id)
    {
        if (id < 10000 || !PlayerControl.LocalPlayer.IsImpostor()) return true;
        var killbutton = DestroyableSingleton<KillButton>.Instance;
        if (!killbutton.currentTarget || killbutton.isCoolingDown || PlayerControl.LocalPlayer.Data.IsDead)
            return false;
        if (AmongUsClient.Instance.AmClient)
        {
            AmongUsClient.Instance.StopAllCoroutines();
            AmongUsClient.Instance.StartCoroutine(__instance.CoEnterVent(id));
        }

        return false;
    }
        
}
[HarmonyPatch(typeof(PlayerPhysics._CoEnterVent_d__55), nameof(PlayerPhysics._CoEnterVent_d__55.MoveNext))]
internal static class EnterVentAnimationPatch
{
    private static void Postfix(PlayerPhysics._CoEnterVent_d__55 __instance, bool __result)
    {
        bool flag = !__result;
        if (flag)
        {
            bool flag2 = __instance.__4__this.myPlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;
            if (flag2)
            {
                bool flag3 = __instance._vent_5__2.IsPlayer();
                if (flag3)
                {
                    PlayerControl.LocalPlayer.MyPhysics.CoExitVent(__instance._vent_5__2.Id);
                }
            }
        }
    }
}

[HarmonyPatch(typeof(PlayerPhysics._CoExitVent_d__56), nameof(PlayerPhysics._CoExitVent_d__56.MoveNext))]
internal static class ExitVentAnimationPatch
{
    private static void Postfix(PlayerPhysics._CoExitVent_d__56 __instance, bool __result)
    {
        bool flag = !__result;
        if (flag)
        {
            int id = __instance.id;
            bool flag2 = id == -1;
            if (!flag2)
            {
                List<Vent> list = Enumerable.ToList<Vent>(ShipStatus.Instance.AllVents);
                Vent vent = Enumerable.FirstOrDefault<Vent>(list, (Vent v) => v.Id == id);
                bool flag3 = !vent;
                if (!flag3)
                {
                    bool flag4 = !vent.IsPlayer();
                    if (!flag4)
                    {
                        list.Remove(vent);
                        ShipStatus.Instance.AllVents = list.ToArray();
                        Object.Destroy(vent);
                        int playerId = id / 10000 - 1;
                        bool flag5 = __instance.__4__this.myPlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;
                        if (flag5)
                        {
                            NetworkedPlayerInfo networkedPlayerInfo =
                                Enumerable.FirstOrDefault<NetworkedPlayerInfo>(GameData.Instance.AllPlayers.ToArray(),
                                    (NetworkedPlayerInfo p) => (int)p.PlayerId == playerId);
                            bool flag6 = networkedPlayerInfo && networkedPlayerInfo.Object;
                            if (flag6)
                            {
                                PlayerControl.LocalPlayer.CmdCheckMurder(networkedPlayerInfo.Object);
                            }
                        }
                    }
                }
            }
        }
    }
}*/
