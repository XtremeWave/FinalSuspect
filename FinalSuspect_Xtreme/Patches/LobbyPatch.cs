using HarmonyLib;
using UnityEngine;
using TMPro;

namespace FinalSuspect_Xtreme.Patches;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public class LobbyStartPatch
{
    private static GameObject Paint;
    public static void Postfix(LobbyBehaviour __instance)
    {
        if (Paint != null) return;
        Paint = Object.Instantiate(__instance.transform.FindChild("Leftbox").gameObject, __instance.transform);
        Paint.name = "FinalSuspect_Xtreme Lobby Paint";
        Paint.transform.localPosition = new Vector3(0.042f, -2.59f, -10.5f);
        SpriteRenderer renderer = Paint.GetComponent<SpriteRenderer>();
        renderer.sprite = Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.LobbyPaint.png", 290f);
    }


}
[HarmonyPatch(typeof(HostInfoPanel), nameof(HostInfoPanel.SetUp))]
public static class HostInfoPanelUpdatePatch
{
    private static TextMeshPro HostText;
    public static void Postfix(HostInfoPanel __instance)
    {
        if (AmongUsClient.Instance.AmHost)
        {
            if (HostText == null)
                HostText = __instance.content.transform.FindChild("Name").GetComponent<TextMeshPro>();

            string htmlStringRgb = ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[__instance.player.ColorId]);
            string hostName = Main.HostNickName;
            string youLabel = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.HostYouLabel);

            // Set text in host info panel
            HostText.text = $"<color=#{htmlStringRgb}>{hostName}</color>  <size=90%><b><font=\"Barlow-BoldItalic SDF\" material=\"Barlow-BoldItalic SDF Outline\">({youLabel})";
        }
    }
}