﻿using FinalSuspect.Player;
using HarmonyLib;

namespace FinalSuspect.Patches;

[HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetFilterText))]
public static class HauntMenuMinigameSetFilterTextPatch
{
    public static bool Prefix(HauntMenuMinigame __instance)
    {
        return XtremeLocalHandling.GetHauntFilterText(__instance);
    }
}