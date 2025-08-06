using Hazel;

namespace FinalSuspect.Patches.System.Others;

[HarmonyPatch]
internal class HazelPatch
{
    [HarmonyPatch(typeof(MessageReader), nameof(MessageReader.ReadPackedUInt32))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool Read(MessageReader __instance)
    {
        return __instance.BytesRemaining >= 1;
    }
}