#if Android
using System;
using UnityEngine;

namespace FinalSuspect.Modules.Core.Plugin.RegistryManager;

public class AndroidPreferenceStore : IPreferenceStore
{
    private const string Prefix = "FS_";
   

    public string GetString(string key, string defaultValue = "")
    {
        try
        {
            var playerPrefsKey = Prefix + key;
            return PlayerPrefs.HasKey(playerPrefsKey) ? PlayerPrefs.GetString(playerPrefsKey, defaultValue) : "";
        }
        catch (Exception e)
        {
            Error($"[AndroidPreferenceStore] 读取偏好设置失败 {key}: {e.Message}", "Registry Manager");
            return defaultValue;
        }
    }

    public void SetString(string key, string value)
    {
        try
        {
            var playerPrefsKey = Prefix + key;
            PlayerPrefs.SetString(playerPrefsKey, value);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Error($"[AndroidPreferenceStore] 设置偏好设置失败 {key}: {e.Message}", "Registry Manager");
        }
    }

    public void Init()
    {
        try
        {
            Info("[AndroidPreferenceStore] Android偏好存储初始化完成", "Registry Manager");
        }
        catch (Exception e)
        {
            Error($"[AndroidPreferenceStore] 初始化失败: {e.Message}", "Registry Manager");
        }
    }

    public bool ContainsKey(string key)
    {
        try
        {
            return PlayerPrefs.HasKey(Prefix + key);
        }
        catch (Exception e)
        {
            Error($"[AndroidPreferenceStore] 检查键存在失败 {key}: {e.Message}", "Registry Manager");
            return false;
        }
    }

    public void DeleteKey(string key)
    {
        try
        {
            PlayerPrefs.DeleteKey(Prefix + key);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Error($"[AndroidPreferenceStore] 删除键失败 {key}: {e.Message}", "Registry Manager");
        }
    }
}

#endif