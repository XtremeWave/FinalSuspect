using System;
using FinalSuspect.Helpers;
using InnerNet;
using TMPro;
using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(GameContainer))]
public class GameContainerPatch
{
    [HarmonyPatch(nameof(GameContainer.SetupGameInfo))]
    [HarmonyPostfix]
    public static void SetupGameInfo_Postfix(GameContainer __instance)
    {
        var mapTrans = __instance.mapLogo.transform;
        var old = mapTrans.parent.FindChild("NameText")?.gameObject;
        if (old)
            Object.Destroy(old);

        var nameText = new GameObject("NameText")
        {
            transform =
            {
                parent = __instance.mapLogo.transform.parent,
                localPosition = new Vector3(-0.6f, mapTrans.localPosition.y, mapTrans.localPosition.z),
                localScale = new Vector3(0.14f, 0.14f, 1f)
            }
        };
        var tmp = nameText.AddComponent<TextMeshPro>();

        var game = __instance.gameListing;
        var color = "#ffffff";
        string showHostName = null;
        var trueHostName = __instance.gameListing.TrueHostName;
        string platformName;
        var platform = game.Platform;

        switch (platform)
        {
            case Platforms.StandaloneEpicPC:
                color = "#905CDA";
                platformName = "Epic";
                break;
            case Platforms.StandaloneSteamPC:
                color = "#4391CD";
                platformName = "Steam";
                break;
            case Platforms.StandaloneMac:
                color = "#e3e3e3";
                platformName = "Mac.";
                break;
            case Platforms.StandaloneWin10:
                color = "#0078d4";
                platformName = GetString("Platform.MicrosoftStore");
                break;
            case Platforms.StandaloneItch:
                color = "#E35F5F";
                platformName = "Itch";
                break;
            case Platforms.IPhone:
                color = "#e3e3e3";
                platformName = GetString(platform);
                break;
            case Platforms.Android:
                color = "#1EA21A";
                platformName = GetString(platform);
                break;
            case Platforms.Switch:
                var halfLength = trueHostName.Length / 2;
                var firstHalf = trueHostName.AsSpan(0, halfLength).ToString();
                var secondHalf = trueHostName.AsSpan(halfLength).ToString();
                showHostName = $"<color=#00B2FF>{firstHalf}</color><color=#ff0000>{secondHalf}</color>";
                platformName = "<color=#00B2FF>Nintendo</color><color=#ff0000>Switch</color>";
                break;
            case Platforms.Xbox:
                color = "#07ff00";
                platformName = "Xbox";
                break;
            case Platforms.Playstation:
                color = "#0014b4";
                platformName = "PlayStation";
                break;
            case Platforms.Unknown:
            default:
                color = "#E57373";
                platformName = "???";
                break;
        }

        showHostName ??= $"<color={color}>{trueHostName}</color>";
        var platforms = $"<color={color}>{platformName}</color>";

        tmp.text = $"<size=40%>{showHostName}</size>" +
                   $"\n<size=18%><color={ColorHelper.FSColorHex}>{GameCode.IntToGameName(game.GameId)}</color>" +
                   $" <color=#ffff00>----</color>{platforms}<color=#ffff00>----</color></size>";
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Left;
    }
}