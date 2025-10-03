using FinalSuspect.Attributes;
using UnityEngine;

namespace FinalSuspect.DataHandling.FinalGameData;

public static partial class FinalGameData
{
    public static string LastResultText;
    public static string LastRoomCode;
    public static string LastServer;
    public static string LastGameData;
    public static string LastGameResult;
    public static Color LastLocalPlayerRoleColor;
    public static bool JoinedCompleted;
    public static bool IntroDestroyed;
    public static string HostNickName = "";

    [GameModuleInitializer]
    public static void Init()
    {
        IntroDestroyed = false;
        LastResultText = LastGameData = LastGameResult = LastRoomCode = LastServer = "";
    }
}

public enum VanillaDeathReason
{
    None,
    Exile,
    Kill,
    Disconnect
}