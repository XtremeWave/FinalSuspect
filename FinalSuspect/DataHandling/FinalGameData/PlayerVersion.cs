using System;

namespace FinalSuspect.DataHandling.FinalGameData;

public static partial class FinalGameData
{
    public class PlayerVersion(Version ver, string tag_str, string forkId)
    {
        public static Dictionary<int, PlayerVersion> PlayerVersions = new();
        public readonly string ForkId = forkId;
        public readonly string Tag = tag_str;
        public readonly Version Version = ver;
    }
}