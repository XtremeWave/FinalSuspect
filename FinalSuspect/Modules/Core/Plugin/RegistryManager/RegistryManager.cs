using System;
using System.IO;

namespace FinalSuspect.Modules.Core.Plugin.RegistryManager;

public static class RegistryManager
{
    private static IPreferenceStore PreferenceStore
    {
        get
        {
#if Windows
            return new WindowsPreferenceStore();
#elif Android
            return new AndroidPreferenceStore();
#endif
        }
    }

    public static string LastStartVersion
    {
        get => PreferenceStore.GetString("Last launched version");
        set => PreferenceStore.SetString("Last launched version", value);
    }

    public static void Init()
    {
        try
        {
            PreferenceStore.Init();

            Info("上次启动的FinalSuspect版本：" + LastStartVersion, "Registry Manager");

            PerformCleanup();

            Info("RegistryManager 初始化完成", "Registry Manager");
        }
        catch (Exception e)
        {
            Error($"RegistryManager 初始化失败: {e.Message}", "Registry Manager");
        }
    }

    private static void PerformCleanup()
    {
        var itemsToDelete = new List<string>
        {
            "./Final Suspect_Data/Sounds",
            "./Final Suspect_Data/ModNews"
        };

        foreach (var item in itemsToDelete)
        {
            try
            {
                if (Directory.Exists(item))
                {
                    Directory.Delete(item, true);
                    Warn("删除无用目录: " + item, "Registry Manager");
                }
                else if (File.Exists(item))
                {
                    File.Delete(item);
                    Warn("删除无用文件: " + item, "Registry Manager");
                }
            }
            catch (Exception e)
            {
                Warn($"删除失败 {item}: {e.Message}", "Registry Manager");
            }
        }
    }
}