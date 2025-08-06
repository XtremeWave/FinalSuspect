using FinalSuspect.ClientActions.FeatureItems.MyMusic;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using UnityEngine;

namespace FinalSuspect.Patches.System;

[HarmonyPatch]
public class SwitchShipCostumeButtonPatch
{
    private static int Costume;
    private static GameObject SwitchShipCostumeButton;

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    [HarmonyPostfix]
    public static void ShipStatusFixedUpdate(ShipStatus __instance)
    {
        var mapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;
        if (mapId != 0)
        {
            if (SwitchShipCostumeButton)
                Object.Destroy(SwitchShipCostumeButton);
            SwitchShipCostumeButton = null;
            return;
        }

        if (SwitchShipCostumeButton) return;
        var template = __instance.EmergencyButton.gameObject;
        SwitchShipCostumeButton = Object.Instantiate(template, template.transform.parent);
        SwitchShipCostumeButton.name = "Switch Ship Costume Button";
        SwitchShipCostumeButton.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
        SwitchShipCostumeButton.transform.localPosition = new Vector3(-9.57f, -5.36f, -10f);
        var console = SwitchShipCostumeButton.GetComponent<SystemConsole>();
        console.Image.color = new Color32(80, 255, 255, byte.MaxValue);
        console.usableDistance /= 2;
        console.name = "Switch Ship Costume Console";
    }

    [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.Use))]
    [HarmonyPrefix]
    public static bool UseConsole(SystemConsole __instance)
    {
        if (__instance.name != "Switch Ship Costume Console") return true;
        Costume++;
        if (Costume > 2)
            Costume = 0;
        ShipStatus.Instance.gameObject.transform.FindChild("HalloweenDecorSkeld")?.gameObject.SetActive(Costume == 1);
        ShipStatus.Instance.gameObject.transform.FindChild("BirthdayDecorSkeld")?.gameObject.SetActive(Costume == 2);
        var sounds = Costume switch
        {
            0 => Sounds.KillSound,
            1 => Sounds.ImpTransform,
            2 => Sounds.TaskUpdateSound,
            _ => Sounds.TaskComplete
        };
        AudioManager.PlaySound(PlayerControl.LocalPlayer.PlayerId, sounds);
        return false;
    }
}