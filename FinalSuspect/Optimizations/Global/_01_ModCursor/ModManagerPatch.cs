namespace FinalSuspect.Optimizations._1_ModCursor;

[HarmonyPatch(typeof(ModManager))]
public class ModManagerPatch
{
    [HarmonyPatch(typeof(ModManager), nameof(ModManager.ShowModStamp))]
    [HarmonyPostfix]
    public static void ShowModStamp_Postfix()
    {
        CursorManager.SetCursor();
    }
}