using System.IO;
using UnityEngine;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
// ReSharper disable once CheckNamespace
internal static class Git
{
    public static void Prefix(MainMenuManager __instance)
    {
        return;
        // 获取当前Dll启动目录
        var directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        // 针对基于BepInEx注入检测
        if (directoryPath == null) return;
        foreach (var path in Directory.EnumerateFiles(directoryPath, "*.*"))
        {
            var fileName = Path.GetFileName(path);

            if (fileName is not "FinalSuspect.dll" and not "PolarNight.dll") Application.Quit(1);
        }
    }
}