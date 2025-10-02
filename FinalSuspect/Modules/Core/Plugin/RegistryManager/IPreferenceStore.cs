namespace FinalSuspect.Modules.Core.Plugin.RegistryManager;

public interface IPreferenceStore
{
    string GetString(string key, string defaultValue = "");
    void SetString(string key, string value);
    void Init();
    bool ContainsKey(string key);
    void DeleteKey(string key);
}