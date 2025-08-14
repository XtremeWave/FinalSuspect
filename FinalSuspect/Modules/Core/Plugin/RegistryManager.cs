using System.IO;
using Microsoft.Win32;

namespace FinalSuspect.Modules.Core.Plugin;

# pragma warning disable CA1416
public static class RegistryManager
{
    private static RegistryKey Keys
    {
        get
        {
            var getKey = SoftwareKeys.OpenSubKey("AU-FinalSuspect", true);
            if (getKey != null) return getKey;
            Info("Create FinalSuspect Registry Key", "Registry Manager");
            return SoftwareKeys.CreateSubKey("AU-FinalSuspect", true);
        }
    }

    public static string LastStartVersion
    {
        get => Keys.GetValue("Last launched version")?.ToString() ?? "";
        set => Keys.SetValue("Last launched version", value);
    }

    private static RegistryKey SoftwareKeys => Registry.CurrentUser.OpenSubKey("Software", true);

    public static void Init()
    {
        if (Keys == null)
        {
            Error("Create Registry Failed", "Registry Manager");
            return;
        }

        Keys.SetValue("Path", Path.GetFullPath("./"));

        List<string> FoldersNFileToDel = [];

        Info("上次启动的FinalSuspect版本：" + LastStartVersion, "Registry Manager");
        FoldersNFileToDel.Add("./Final Suspect_Data/Sounds");
        FoldersNFileToDel.Add("./Final Suspect_Data/ModNews");
        FoldersNFileToDel.DoIf(Directory.Exists, p =>
        {
            Warn("Delete Useless Directory:" + p, "Registry Manager");
            Directory.Delete(p, true);
        });
        FoldersNFileToDel.DoIf(File.Exists, p =>
        {
            Warn("Delete Useless File:" + p, "Registry Manager");
            File.Delete(p);
        });
    }
}