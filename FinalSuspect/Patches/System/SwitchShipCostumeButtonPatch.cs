using FinalSuspect.ClientItems.FeatureItems.MyMusic;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using UnityEngine;

namespace FinalSuspect.Patches.System;

[HarmonyPatch]
public class SwitchShipCostumeButtonPatch
{
    private static int _costume;
    private static GameObject _switchShipCostumeButton;

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    [HarmonyPostfix]
    public static void ShipStatusFixedUpdate(ShipStatus __instance)
    {
        var mapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;
        if (mapId != 0)
        {
            if (_switchShipCostumeButton)
                Object.Destroy(_switchShipCostumeButton);
            _switchShipCostumeButton = null;
            return;
        }

        if (_switchShipCostumeButton) return;
        var template = __instance.EmergencyButton.gameObject;
        _switchShipCostumeButton = Object.Instantiate(template, template.transform.parent);
        _switchShipCostumeButton.name = "Switch Ship Costume Button";
        _switchShipCostumeButton.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
        _switchShipCostumeButton.transform.localPosition = new Vector3(-9.57f, -5.36f, -10f);
        var console = _switchShipCostumeButton.GetComponent<SystemConsole>();
        console.Image.color = new Color32(80, 255, 255, byte.MaxValue);
        console.usableDistance /= 2;
        console.name = "Switch Ship Costume Console";
    }

    [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.Use))]
    [HarmonyPrefix]
    public static bool UseConsole(SystemConsole __instance)
    {
        if (__instance.name != "Switch Ship Costume Console") return true;
        _costume++;
        if (_costume > 2)
            _costume = 0;
        ShipStatus.Instance.gameObject.transform.FindChild("HalloweenDecorSkeld")?.gameObject.SetActive(_costume == 1);
        ShipStatus.Instance.gameObject.transform.FindChild("BirthdayDecorSkeld")?.gameObject.SetActive(_costume == 2);
        var sounds = _costume switch
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