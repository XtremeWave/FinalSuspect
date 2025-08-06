using System;
using System.IO;
using Microsoft.Win32;

namespace FinalSuspect.Modules.Core.Plugin;

# pragma warning disable CA1416
public static class RegistryManager
{
    private static RegistryKey Keys = SoftwareKeys.OpenSubKey("AU-FinalSuspect", true);
    private static Version LastVersion;
    private static RegistryKey SoftwareKeys => Registry.CurrentUser.OpenSubKey("Software", true);

    public static void Init()
    {
        if (Keys == null)
        {
            Info("Create FinalSuspect Registry Key", "Registry Manager");
            Keys = SoftwareKeys.CreateSubKey("AU-FinalSuspect", true);
        }

        if (Keys == null)
        {
            Error("Create Registry Failed", "Registry Manager");
            return;
        }

        LastVersion = Keys.GetValue("Last launched version") is not string regLastVersion
            ? new Version(0, 0, 0)
            : Version.Parse(regLastVersion);

        Keys.SetValue("Last launched version", Main.version.ToString());
        Keys.SetValue("Path", Path.GetFullPath("./"));

        List<string> FoldersNFileToDel = [];

        Info("上次启动的FinalSuspect版本：" + LastVersion, "Registry Manager");

#if RELEASE
        if (LastVersion < new Version(1, 2, 0))
        {
            Warn("v1.2 New Version Operation Needed", "Registry Manager");
            FoldersNFileToDel.Add("./BepInEx/config");
        }
#endif

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