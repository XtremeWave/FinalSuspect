using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.Prank;

// SpecialImpostor
/*[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
public static class StartGamePatch
{
    private static void Postfix()
    {
        if (!PlayerControl.LocalPlayer.IsImpostor()) return;
        List<Vent> list = Enumerable.ToList(ShipStatus.Instance.AllVents);
        Vent vent = Object.Instantiate(list[0]);
        vent.gameObject.SetActive(false);
        foreach (PlayerControl playerControl in Main.AllPlayerControls.Where(x =>x != PlayerControl.LocalPlayer)/*.Where(x => !x.IsImpostor()))
        {
            Vent ventaspc = playerControl.gameObject.AddComponent<Vent>();
            ventaspc.Id = ((int)playerControl.PlayerId + 1) * 10000;
            ventaspc.myAnim = vent.myAnim;
            ventaspc.Buttons = new Il2CppReferenceArray<ButtonBehavior>(Array.Empty<ButtonBehavior>());
            ventaspc.myRend = playerControl.GetComponent<SpriteRenderer>();
            list.Add(ventaspc);
        }
        ShipStatus.Instance.AllVents = new Il2CppReferenceArray<Vent>(list.ToArray());
    }
    public static bool IsPlayer(this Vent vent)
    {
        return vent.Id >= 10000;
    }
}*/