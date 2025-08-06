using FinalSuspect.Helpers;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(LoadingBarManager))]
public class LoadingBarManagerPatch
{
    private static GameObject AmongUsLogo;
    private static GameObject ModLogo;
    private static Vector3 LogoVector;
    private static bool hasSetVec;

    [HarmonyPatch(nameof(LoadingBarManager.ToggleLoadingBar))]
    public static void Prefix(LoadingBarManager __instance, ref bool on)
    {
        AmongUsLogo = __instance.loadingBar.transform.FindChild("Canvas").FindChild("Logo").gameObject;
        if (AmongUsLogo)
        {
            var trans = AmongUsLogo.GetComponent<RectTransform>();
            var cr = AmongUsLogo.GetComponent<CanvasRenderer>();
            if (!hasSetVec)
            {
                hasSetVec = true;
                LogoVector = trans.localPosition;
            }

            if (!ModLogo)
            {
                ModLogo = Object.Instantiate(AmongUsLogo, trans.parent);
                ModLogo.GetComponent<Image>().sprite = LoadSprite("FinalSuspect-Logo.png", 150f);
            }

            trans.localScale = new Vector3(0.3f, 0.3f, 1);
            cr.transform.localPosition = new Vector3(LogoVector.x, LogoVector.y + 60, LogoVector.z);
            trans.localPosition = new Vector3(LogoVector.x, LogoVector.y + 60, LogoVector.z);
            cr.transform.localScale = new Vector3(0.3f, 0.3f, 1);
            ModLogo.GetComponent<RectTransform>().localPosition =
                new Vector3(LogoVector.x, LogoVector.y - 80, LogoVector.z);
            ModLogo.GetComponent<CanvasRenderer>().transform.localPosition =
                new Vector3(LogoVector.x, LogoVector.y - 80, LogoVector.z);
            ModLogo.GetComponent<RectTransform>().localScale = new Vector3(1.4f, 1.4f, 1);
            ModLogo.GetComponent<CanvasRenderer>().transform.localScale = new Vector3(1.4f, 1.4f, 1);
        }

        __instance.loadingBar.barFill.color = ColorHelper.FSColor;
        __instance.loadingBar.crewmate.gameObject.SetActive(false);
        try
        {
            if (!IsNotJoined) return;
            on = false;
        }
        catch
        {
            on = false;
        }
    }
}