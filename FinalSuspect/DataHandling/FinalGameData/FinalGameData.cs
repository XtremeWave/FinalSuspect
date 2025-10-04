using FinalSuspect.Attributes;

namespace FinalSuspect.DataHandling.FinalGameData;

public static partial class FinalGameData
{
    public static bool JoinedCompleted;
    public static bool IntroDestroyed;
    public static string HostNickName = "";

    [GameModuleInitializer]
    public static void Init()
    {
        IntroDestroyed = false;
    }
}

public enum VanillaDeathReason
{
    None,
    Exile,
    Kill,
    Disconnect
}