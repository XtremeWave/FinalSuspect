using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(JoinGameButton), nameof(JoinGameButton.OnClick))]
class JoinGameButtonPatch
{
    public static void Prefix(JoinGameButton __instance)
    {
        if (__instance.GameIdText == null) return;
        if (__instance.GameIdText.text == "" && Regex.IsMatch(GUIUtility.systemCopyBuffer.Trim('\r', '\n'), @"^[A-Z]{6}$"))
        {
            Modules.Core.Plugin.Logger.Info($"{GUIUtility.systemCopyBuffer}", "ClipBoard");
            __instance.GameIdText.SetText(GUIUtility.systemCopyBuffer.Trim('\r', '\n'));
        }
    }
}