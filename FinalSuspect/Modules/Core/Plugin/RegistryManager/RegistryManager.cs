using System;
using System.IO;
using FinalSuspect.Attributes;

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

    public static Version LangVersion
    {
        get => string.IsNullOrEmpty(PreferenceStore.GetString("Lang version"))
            ? new Version(0, 0, 0, 0)
            : new Version(PreferenceStore.GetString("Lang version"));
        set => PreferenceStore.SetString("Lang version", value.ToString());
    }

    [PluginModuleInitializer(InitializePriority.High)]
    public static void OnInitialization()
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