using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.System.Others;

[HarmonyPatch(typeof(Object), nameof(Object.Destroy), typeof(Object))]
public class UnityEnginePatch
{
    public static bool Prefix([HarmonyArgument(0)] Object obj)
    {
        try
        {
            return obj.name is not "LobbyInfoPane" and not "GameStartManager" || IsFreePlay || IsNotJoined;
        }
        catch
        {
            return true;
        }
    }
}