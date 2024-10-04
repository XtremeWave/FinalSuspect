using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using FinalSuspect.Attributes;
using UnityEngine;
using Il2CppSystem.IO;

namespace FinalSuspect.Modules.Managers;

public static class ServerAddManager
{
    private static ServerManager serverManager = DestroyableSingleton<ServerManager>.Instance;

    [PluginModuleInitializer]
    public static void Init()
    {



        serverManager.AvailableRegions = ServerManager.DefaultRegions;
        List<IRegionInfo> regionInfos = new();
        //    regionInfos.Add(CreateHttp("154.21.201.164", $"XtremeWave[{Translator.GetString("HongKong")}]", 22023, false));
        regionInfos.Add(CreateHttp("au-us.niko233.me", "Niko233[NA_US]", 443, true));
        regionInfos.Add(CreateHttp("au-us2.niko233.me", "Niko233[NA_US2]", 443, true));

        if (Translator.IsChineseUser)
        {
            regionInfos.Add(CreateHttp("aucn.niko233.me", "Niko233[AS_CN]", 443, true));
            regionInfos.Add(CreateHttp("aucn2.niko233.m", "Niko233[AS_CN2]", 443, true));

            regionInfos.Add(CreateHttp("au-sh.pafyx.top", "梦服上海 (新)", 22000, false));
            regionInfos.Add(CreateHttp("124.222.148.195", "小猫私服", 22000, false));
            regionInfos.Add(CreateHttp("au.3q.fan", "小猫354[北京]", 22020, false));
            regionInfos.Add(CreateHttp("45yun.cn", "小猫服[北京]", 22000, false));
            regionInfos.Add(CreateHttp("player.fangkuai.fun", "方块服[北京]", 22024, true));

        }
        regionInfos.Add(CreateHttp("au-as.duikbo.at", "Modded Asia (MAS)", 443, true));
        regionInfos.Add(CreateHttp("www.aumods.xyz", "Modded NA (MNA)", 443, true));
        regionInfos.Add(CreateHttp("au-eu.duikbo.at", "Modded EU (MEU)", 443, true));




        var defaultRegion = serverManager.CurrentRegion;
        regionInfos.Where(x => !serverManager.AvailableRegions.Contains(x)).Do(serverManager.AddOrUpdateRegion);
        serverManager.SetRegion(defaultRegion);

        SetServerName(defaultRegion.Name);
    }
    public static void SetServerName(string serverName = "")
    {
        if (serverName == "") serverName = ServerManager.Instance.CurrentRegion.Name;
        var name = serverName switch
        {
            "Modded Asia (MAS)" => "MAS",
            "Modded NA (MNA)" => "MNA",
            "Modded EU (MEU)" => "MEU",
            "North America" => "NA",
            "梦服上海 (新)" => "梦服",
            "小猫私服" => "小猫",
            "小猫354[北京]" => "小猫354",
            "小猫服[北京]" => "小猫[北京]",
            "方块服[北京]" => "方块[北京]",
            "Niko233[AS_CN]" => "Niko[CN]",
            "Niko233[NA_US]" => "Niko[US]",
            "Niko233[AS_CN2]" => "Niko[CN2]",
            "Niko233[NA_US2]" => "Niko[US2]",
            "XtremeWave[HongKong]" => "XW[HK]",

            _ => serverName,
        };

        if ((TranslationController.Instance?.currentLanguage?.languageID ?? SupportedLangs.SChinese) is SupportedLangs.SChinese or SupportedLangs.TChinese)
        {
            name = name switch
            {
                "Asia" => "亚服",
                "Europe" => "欧服",
                "North America" => "北美服",
                "NA" => "北美服",
                "XW[HK]" => "XW[香港]",
                _ => name,
            };
        };

        Color32 color = serverName switch
        {
            "Asia" => new(58, 166, 117, 255),
            "Europe" => new(58, 166, 117, 255),
            "North America" => new(58, 166, 117, 255),
            "Modded Asia (MAS)" => new(255, 132, 0, 255),
            "Modded NA (MNA)" => new(255, 132, 0, 255),
            "Modded EU (MEU)" => new(255, 132, 0, 255),
            "梦服上海 (新)" => new(1, 182, 253, 255),
            "小猫私服" => new(181, 158, 204, 255),
            "小猫354[北京]" => new(181, 158, 204, 255),
            "小猫服[北京]" => new(181, 158, 204, 255),
            "方块服[北京]" => new(105, 105, 193, 255),
            "Niko233[AS_CN]" => new(255, 255, 0, 255),
            "Niko233[NA_US]" => new(255, 255, 0, 255),
            "Niko233[AS_CN2]" => new(255, 255, 0, 255),
            "Niko233[NA_US2]" => new(255, 255, 0, 255),
            "XtremeWave[HongKong]" => ColorHelper.TeamColor32,

            _ => new(255, 255, 255, 255),
        };
        Cloud.ServerName = name;
        PingTrackerUpdatePatch.ServerName = Utils.ColorString(color, name);
    }

    public static IRegionInfo CreateHttp(string ip, string name, ushort port, bool ishttps)
    {
        string serverIp = (ishttps ? "https://" : "http://") + ip;
        ServerInfo serverInfo = new ServerInfo(name, serverIp, port, false);
        ServerInfo[] ServerInfo = new ServerInfo[] { serverInfo };
        return new StaticHttpRegionInfo(name, (StringNames)1003, ip, ServerInfo).Cast<IRegionInfo>();
    }
}