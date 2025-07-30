using System.IO;

namespace FinalSuspect.Patches.System.Others;

public static class FilePatch
{
    [HarmonyPatch(typeof(File), nameof(File.Delete))]
    public static bool Prefix([HarmonyArgument(0)] string path)
    {
        return Directory.Exists(path);
    }
}