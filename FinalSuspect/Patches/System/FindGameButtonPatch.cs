using AmongUs.Data;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(FindGameButton), nameof(FindGameButton.OnClick))]
public class FindGameButtonPatch
{
    public static bool Prefix(FindGameButton __instance)
    {
        DataManager.Settings.Save();
        AmongUsClient.Instance.NetworkMode = NetworkModes.OnlineGame;
        AmongUsClient.Instance.MainMenuScene = "MainMenu";
        __instance.StartCoroutine(AmongUsClient.Instance.CoFindGame());
        return false;
    }
}