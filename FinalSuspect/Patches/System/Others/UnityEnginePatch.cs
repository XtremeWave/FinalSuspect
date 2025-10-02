using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.System.Others;

[HarmonyPatch(typeof(Object), nameof(Object.Destroy), typeof(Object))]
public class UnityEnginePatch
{
    public static bool Prefix([HarmonyArgument(0)] Object obj)
    {
        bool Return;
        try
        {
            Return = obj.name is not "LobbyInfoPane" and not "GameStartManager" || IsFreePlay || IsNotJoined;
            if (obj.name is "IntroCutscene")
                IntroCutsceneOnDestroyPatch.Postfix();
        }
        catch
        {
            return true;
        }

        return Return;
    }
}