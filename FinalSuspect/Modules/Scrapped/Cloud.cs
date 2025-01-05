using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using AmongUs.Data;
using FinalSuspect.Attributes;
using FinalSuspect.DataHandling;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Core.Plugin;
using HarmonyLib;

namespace FinalSuspect.Modules.Scrapped;

internal class Cloud
{
    private static string IP;
    private static int LOBBY_PORT = 0;
    private static int FAC_PORT = 0;
    private static Socket ClientSocket;
    private static Socket EacClientSocket;
    private static long LastRepotTimeStamp = 0;

    [PluginModuleInitializer]
    public static void Init()
    {
        try
        {
            var content = GetResourcesTxt("FinalSuspect.Resources.Configs.Port.txt");
            string[] ar = content.Split('|');
            IP = ar[0];
            LOBBY_PORT = int.Parse(ar[1]);
            FAC_PORT = int.Parse(ar[2]);
        }
        catch (Exception e)
        {
            Core.Plugin.Logger.Exception(e, "Cloud Init");
        }
    }
    private static string GetResourcesTxt(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        stream.Position = 0;
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
    public static string ServerName ="";
    public static bool ShareLobby(bool command = false)
    {
        try
        {
            if (!Main.NewLobby || !XtremeGameData.GameStates.IsLobby) return false;
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance || AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) return false;

            if (IP == null || LOBBY_PORT == 0) throw new("Has no ip or port");
            
            string msg = $"{GameStartManager.Instance.GameRoomNameCode.text}|{Main.DisplayedVersion_Head}|{GameData.Instance.PlayerCount}|{TranslationController.Instance.currentLanguage.languageID}|{ServerName}|{DataManager.player.customization.name}";
            if (msg.Length <= 60)
            {
                byte[] buffer = Encoding.Default.GetBytes(msg);
                ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ClientSocket.Connect(IP, LOBBY_PORT);
                ClientSocket.Send(buffer);
                ClientSocket.Close();
            }
            Main.NewLobby = false; 

        }
        catch (Exception e)
        {
            Core.Plugin.Logger.Exception(e, "SentLobbyToQQ");
            throw;
        }
        return true;
    }

    private static bool connecting = false;
    public static void StartConnect()
    {
        if (connecting || EacClientSocket != null && EacClientSocket.Connected) return;
        connecting = true;
        _ = new LateTask(() =>
        {
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance || AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame)
            {
                connecting = false;
                return;
            }
            try
            {
                if (IP == null || FAC_PORT == 0) throw new("Has no ip or port");
                LastRepotTimeStamp = Utils.GetTimeStamp();
                EacClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                EacClientSocket.Connect(IP, FAC_PORT);
                Core.Plugin.Logger.Warn("已连接至FinalSuspect服务器", "FAC Cloud");
            }
            catch (Exception ex)
            {
                connecting = false;
                Core.Plugin.Logger.Error($"Connect To FAC Failed:\n{ex.Message}", "FAC Cloud", false);
            }
            connecting = false;
        }, 3.5f, "FAC Cloud Connect");
    }
    public static void StopConnect()
    {
        if (EacClientSocket != null && EacClientSocket.Connected)
            EacClientSocket.Close();
    }
    public static void SendData(string msg)
    {
        StartConnect();
        if (EacClientSocket == null || !EacClientSocket.Connected)
        {
            Core.Plugin.Logger.Warn("未连接至FinalSuspect服务器，报告被取消", "FAC Cloud");
            return;
        }
        EacClientSocket.Send(Encoding.Default.GetBytes(msg));
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class FACConnectTimeOut
    {
        public static void Postfix()
        {
            if (LastRepotTimeStamp != 0 && LastRepotTimeStamp + 8 < Utils.GetTimeStamp())
            {
                LastRepotTimeStamp = 0;
                StopConnect();
                Core.Plugin.Logger.Warn("超时自动断开与FinalSuspect服务器的连接", "FAC Cloud");
            }
        }
    }
}