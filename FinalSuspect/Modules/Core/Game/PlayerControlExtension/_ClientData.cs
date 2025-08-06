using InnerNet;

namespace FinalSuspect.Modules.Core.Game.PlayerControlExtension;

public static class _ClientData
{
    public static ClientData GetClient(this PlayerControl player)
    {
        try
        {
            var client = AmongUsClient.Instance.allClients
                .ToArray().FirstOrDefault(cd => cd.Character.PlayerId == player.PlayerId);
            return client;
        }
        catch
        {
            return null;
        }
    }

    public static int GetClientId(this PlayerControl player)
    {
        if (!player) return -1;
        var client = player.GetClient();
        return client?.Id ?? -1;
    }

    public static bool IsHost(this PlayerControl player)
    {
        try
        {
            return AmongUsClient.Instance.GetHost().Id == player.GetClient().Id;
        }
        catch
        {
            return false;
        }
    }

    public static string GetPlatform(this PlayerControl player)
    {
        try
        {
            var color = "";
            var name = "";
            string text;
            switch (player.GetClient().PlatformData.Platform)
            {
                case Platforms.StandaloneEpicPC:
                    color = "#905CDA";
                    name = "Epic";
                    break;
                case Platforms.StandaloneSteamPC:
                    color = "#4391CD";
                    name = "Steam";
                    break;
                case Platforms.StandaloneMac:
                    color = "#e3e3e3";
                    name = "Mac.";
                    break;
                case Platforms.StandaloneWin10:
                    color = "#0078d4";
                    name = GetString("Platform.MicrosoftStore");
                    break;
                case Platforms.StandaloneItch:
                    color = "#E35F5F";
                    name = "Itch";
                    break;
                case Platforms.IPhone:
                    color = "#e3e3e3";
                    name = GetString("Platform.IPhone");
                    break;
                case Platforms.Android:
                    color = "#1EA21A";
                    name = GetString("Platform.Android");
                    break;
                case Platforms.Switch:
                    name = "<color=#00B2FF>Nintendo</color><color=#ff0000>Switch</color>";
                    break;
                case Platforms.Xbox:
                    color = "#07ff00";
                    name = "Xbox";
                    break;
                case Platforms.Playstation:
                    color = "#0014b4";
                    name = "PlayStation";
                    break;
                case Platforms.Unknown:
                default:
                    break;
            }

            if (color != "" && name != "")
                text = $"<color={color}>{name}</color>";
            else
                text = name;
            return text;
        }
        catch
        {
            return "";
        }
    }
}