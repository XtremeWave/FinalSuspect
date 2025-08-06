using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public class LobbyStartPatch
{
    private static GameObject Paint;

    public static void Postfix(LobbyBehaviour __instance)
    {
        if (Paint) return;
        Paint = Object.Instantiate(__instance.transform.FindChild("Leftbox").gameObject, __instance.transform);
        Paint.name = "FinalSuspect Lobby Paint";
        Paint.transform.localPosition = new Vector3(0.042f, -2.59f, -10.5f);
        var renderer = Paint.GetComponent<SpriteRenderer>();
        renderer.sprite = LoadSprite("AuthorLogo2.png", 440f);
    }
}

[HarmonyPatch(typeof(HostInfoPanel), nameof(HostInfoPanel.SetUp))]
public static class HostInfoPanelUpdatePatch
{
    private static TextMeshPro HostText;

    public static void Postfix(HostInfoPanel __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!HostText)
            HostText = __instance.content.transform.FindChild("Name").GetComponent<TextMeshPro>();

        var htmlStringRgb = ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[__instance.player.ColorId]);
        var hostName = Main.HostNickName;
        var youLabel = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.HostYouLabel);

        HostText.text =
            $"<color=#{htmlStringRgb}>{hostName}</color>  <size=90%><b><font=\"Barlow-BoldItalic SDF\" material=\"Barlow-BoldItalic SDF Outline\">{youLabel}";
    }
}