using System;
using System.Collections.Generic;

namespace FinalSuspect.DataHandling;

public static partial class XtremeGameData
{
    public class PlayerVersion
    {
        public static Dictionary<byte, PlayerVersion> playerVersion = new();
        public readonly Version version;
        public readonly string tag;
        public readonly string forkId;

        public PlayerVersion(string ver, string tag_str, string forkId) : this(Version.Parse(ver), tag_str, forkId)
        { }

        public PlayerVersion(Version ver, string tag_str, string forkId)
        {
            version = ver;
            tag = tag_str;
            this.forkId = forkId;
        }

    }
}