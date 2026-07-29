using BepInEx;
using BepInEx.Unity.IL2CPP;
using FinalSuspect.Attributes;
#if Windows
using System;
using System.IO;
#endif

namespace FinalSuspect.DataHandling.FinalAntiCheat.Core;

public static class DllChecker
{
#if Windows
    private static readonly Type LinkedGuard = typeof(ExeChecker);
#endif

    [PluginModuleInitializer(InitializePriority.High)]
    internal static void OnInitialization()
    {
#if Windows
        // SM的文件名是写死的
        string[] suspiciousFiles = ["SickoMenu.dll", "version.dll"];
        // 获取当前Dll启动目录
        var directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        // 获取游戏根目录
        var amongUsPath = Environment.CurrentDirectory;
        // 针对基于BepInEx注入检测
        if (directoryPath != null)
            foreach (var path in Directory.EnumerateFiles(directoryPath, "*.*"))
            {
                var fileName = Path.GetFileName(path);

                if (fileName is "FinalSuspect.dll" or "PolarNight.dll") continue;
                Error($"检测到非法/模组文件: {fileName}！游戏将被强制终止。", "FAC");
                Application.Quit(1);
            }

        // 针对基于version注入检测
        foreach (var fileName in suspiciousFiles)
        {
            var fullPath = Path.Combine(amongUsPath, fileName);

            if (!File.Exists(fullPath)) continue;
            Error($"检测到非法文件: {fileName}！游戏将被强制终止。", "FAC");
            Application.Quit(1);
        }
#endif
    }
}

[HarmonyPatch(typeof(IL2CPPChainloader), nameof(IL2CPPChainloader.LoadPlugin))]
public static class DisableOtherPlugins
{
    public static bool Prefix([HarmonyArgument(0)] PluginInfo pluginInfo, [HarmonyArgument(1)] Assembly pluginAssembly)
    {
        return
#if Windows
            pluginInfo.Metadata.GUID is "com.sinai.unityexplorer";
#elif Android
            true;
#endif
    }
}