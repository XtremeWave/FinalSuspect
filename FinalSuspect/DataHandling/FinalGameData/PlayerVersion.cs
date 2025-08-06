using System;

namespace FinalSuspect.DataHandling.FinalGameData;

public static partial class FinalGameData
{
    public class PlayerVersion(Version ver, string tag_str, string forkId)
    {
        public static Dictionary<int, PlayerVersion> playerVersion = new();
        public readonly string forkId = forkId;
        public readonly string tag = tag_str;
        public readonly Version version = ver;
    }
}