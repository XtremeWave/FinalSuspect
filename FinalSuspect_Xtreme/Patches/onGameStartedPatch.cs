using AmongUs.GameOptions;
using FinalSuspect_Xtreme.Attributes;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace FinalSuspect_Xtreme;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
internal class CoStartGamePatch
{

    public static void Postfix()
    {
        IntroCutsceneOnDestroyPatch.introDestroyed = false;
        GameModuleInitializerAttribute.InitializeAll();

    }

}