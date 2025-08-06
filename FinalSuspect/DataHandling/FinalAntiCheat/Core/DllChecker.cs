using System;
using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Core;

internal static class DllChecker
{
    internal static void Init()
    {
        // SM的文件名是写死的
        string[] SuspiciousFiles = ["SickoMenu.dll", "version.dll"];
        // 获取当前Dll启动目录
        var DirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        // 获取游戏根目录
        var AmongUsPath = Environment.CurrentDirectory;
        // 针对基于BepInEx注入检测
        if (DirectoryPath != null)
            foreach (var path in Directory.EnumerateFiles(DirectoryPath, "*.*"))
            {
                var fileName = Path.GetFileName(path);

                if (fileName is "FinalSuspect.dll" or "PolarNight.dll") continue;
                Error($"检测到非法/模组文件: {fileName}！游戏将被强制终止。", "FAC");
                Application.Quit(1);
            }

        // 针对基于version注入检测
        foreach (var fileName in SuspiciousFiles)
        {
            var fullPath = Path.Combine(AmongUsPath, fileName);

            if (!File.Exists(fullPath)) continue;
            Error($"检测到非法文件: {fileName}！游戏将被强制终止。", "FAC");
            Application.Quit(1);
        }
    }
}

[HarmonyPatch(typeof(IL2CPPChainloader), nameof(IL2CPPChainloader.LoadPlugin))]
public static class DisableOtherPlugins
{
    public static bool Prefix([HarmonyArgument(0)] PluginInfo pluginInfo, [HarmonyArgument(1)] Assembly pluginAssembly)
    {
        return pluginInfo.Metadata.GUID
            is "com.sinai.unityexplorer"
            or "cn.slok.polarnight";
    }
}