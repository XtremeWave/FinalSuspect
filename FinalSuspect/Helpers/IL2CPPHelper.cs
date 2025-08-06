using Il2CppInterop.Runtime.InteropTypes;

namespace FinalSuspect.Helpers;

public static class IL2CPPHelper
{
    public static bool TryCast<T>(this Il2CppObjectBase obj, out T casted)
        where T : Il2CppObjectBase
    {
        casted = obj.TryCast<T>();
        return casted != null;
    }
}