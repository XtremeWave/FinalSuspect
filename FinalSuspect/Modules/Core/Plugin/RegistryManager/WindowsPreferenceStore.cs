#if Windows
#pragma warning disable CA1416
using System;
using Microsoft.Win32;

namespace FinalSuspect.Modules.Core.Plugin.RegistryManager;

public class WindowsPreferenceStore : IPreferenceStore
{
    public string GetString(string key, string defaultValue = "")
    {
        try
        {
            using var registryKey = GetRegistryKey(false);
            return registryKey?.GetValue(key)?.ToString() ?? defaultValue;
        }
        catch (Exception e)
        {
            Error($"[WindowsPreferenceStore] 读取注册表值失败 {key}: {e.Message}", "Registry Manager");
            return defaultValue;
        }
    }

    public void SetString(string key, string value)
    {
        try
        {
            using var registryKey = GetRegistryKey();
            registryKey?.SetValue(key, value);
        }
        catch (Exception e)
        {
            Error($"[WindowsPreferenceStore] 设置注册表值失败 {key}: {e.Message}", "Registry Manager");
        }
    }

    public void Init()
    {
        try
        {
            SetString("Path", System.IO.Path.GetFullPath("./"));
            Info("[WindowsPreferenceStore] 注册表初始化完成", "Registry Manager");
        }
        catch (Exception e)
        {
            Error($"[WindowsPreferenceStore] 初始化失败: {e.Message}", "Registry Manager");
        }
    }

    public bool ContainsKey(string key)
    {
        try
        {
            using var registryKey = GetRegistryKey(false);
            return registryKey?.GetValue(key) != null;
        }
        catch (Exception e)
        {
            Error($"[WindowsPreferenceStore] 检查键存在失败 {key}: {e.Message}", "Registry Manager");
            return false;
        }
    }

    public void DeleteKey(string key)
    {
        try
        {
            using var registryKey = GetRegistryKey();
            registryKey?.DeleteValue(key, false);
        }
        catch (Exception e)
        {
            Error($"[WindowsPreferenceStore] 删除键失败 {key}: {e.Message}", "Registry Manager");
        }
    }

    private static RegistryKey GetRegistryKey(bool writable = true)
    {
        try
        {
            var softwareKey = Registry.CurrentUser.OpenSubKey("Software", writable);

            var key = softwareKey?.OpenSubKey("AU-FinalSuspect", writable);
            return key ?? softwareKey?.CreateSubKey("AU-FinalSuspect", writable);
        }
        catch (Exception e)
        {
            Error($"[WindowsPreferenceStore] 获取注册表键失败: {e.Message}", "Registry Manager");
            return null;
        }
    }
}
#pragma warning restore CA1416
#endif