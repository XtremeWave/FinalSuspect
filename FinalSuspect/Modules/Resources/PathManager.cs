using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FinalSuspect.Attributes;
using FinalSuspect.Helpers;

namespace FinalSuspect.Modules.Resources;

public static class PathManager
{
    public const string LocalPath_Data = "Final Suspect_Data/";
    public static string DependsSavePath = "BepInEx/core/";
    public static string DownloadFileTempPath = "BepInEx/plugins/FinalSuspect.dll.temp";
    public static string downloadUrl_github = "https://github.com/XtremeWave/FinalSuspect/releases/latest/download/FinalSuspect.dll";
    public static string downloadUrl_gitee = "https://gitee.com/LezaiYa/FinalSuspectAssets/releases/download/v{showVer}/FinalSuspect.dll";
    public static string downloadUrl_xtremeapi = "https://api.xtreme.net.cn/download/FinalSuspect/FinalSuspect.dll";
    
    public static string GetFile(FileType fileType, RemoteType remoteType, string file)
    {
        return GetRemoteUrl(fileType, remoteType) + file;
    }
    public static string GetRemoteUrl(FileType fileType, RemoteType remoteType)
    {
        return "https://" + GetRemoteBase(remoteType) + fileType + "/";
    }

    public static string GetRemoteBase(RemoteType remoteType)
    {
        var remoteBase = "127.0.0.1";
        switch (remoteType)
        {
            case RemoteType.Github:
                remoteBase = "github.com/XtremeWave/FinalSuspect/raw/FinalSus/Assets/";
                break;
            case RemoteType.Gitee:
                remoteBase = "gitee.com/LezaiYa/FinalSuspectAssets/tree/main/Assets/";
                break;
            case RemoteType.XtremeApi:
                remoteBase = "api.xtreme.net.cn/download/FinalSuspect/Assets/";
                break;
        }
        return remoteBase;
    }

    public static string GetLocalFilePath(FileType fileType, string file)
    {
        return fileType switch
        {
            FileType.Depends => GetLocalPath(LocalType.BepInEx) + file,
            _ => GetResourceFilesPath(fileType, file),
        };
    }
    
    public static string GetLocalPath(LocalType localType)
    {
        if (localType == LocalType.BepInEx)
            return DependsSavePath;
        return  LocalPath_Data + localType + "/";
    }
    
    public static string GetResourceFilesPath(FileType fileType, string file)
    {
        return GetLocalPath(LocalType.Resources) + fileType + "/" + file;
    }
    
    public static string GetBanFilesPath(string file)
    {
        return GetLocalPath(LocalType.Ban) + file;
    }

    [PluginModuleInitializer(InitializePriority.High)]
    public static void InitializePaths()
    {
        CheckAndCreate(GetLocalPath(LocalType.Resources), false);
        CheckAndCreate(GetLocalPath(LocalType.Resources) + "Sounds", false);
        CheckAndCreate(GetLocalPath(LocalType.Resources) + "Images");
        CheckAndCreate(GetLocalPath(LocalType.Resources) + "ModNews");
        foreach (var lang in EnumHelper.GetAllNames<SupportedLangs>())
        {
            CheckAndCreate(GetLocalPath(LocalType.Resources) + $"ModNews/{lang}");
        }
        CheckAndCreate(GetLocalPath(LocalType.Resources) + "Languages");
        CheckAndCreate(GetLocalPath(LocalType.Ban));
        CheckAndCreate(GetLocalPath(LocalType.Bypass), false);
    }

    private static void CheckAndCreate(string path, bool hidden = true)
    {
        if (path == null) return;
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        var attributes = File.GetAttributes(path);
        File.SetAttributes(path, hidden
            ? attributes | FileAttributes.Hidden 
            : attributes & ~FileAttributes.Hidden);
    }
    
    public static string GetBypassFileType(FileType fileType, BypassType bypassType)
    {
        return GetLocalPath(LocalType.Bypass) + $"BypassCheck_{fileType}_{bypassType}.xwr";
    }
    
    private static IReadOnlyList<string> URLs => new List<string>
    {
#if DEBUG
        "https://raw.githubusercontent.com/XtremeWave/FinalSuspect_Dev/FS_Dev/",
        "https://api.xtreme.net.cn/download/FinalSuspect/",
        $"file:///{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))}/",
#endif

#if CANARY
        "https://raw.githubusercontent.com/XtremeWave/FinalSuspect_Dev/FS_Dev/",
        "https://api.xtreme.net.cn/download/FinalSuspect/",
        $"file:///{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))}/",
#else        
        "https://raw.githubusercontent.com/XtremeWave/FinalSuspect/FinalSus/",
        "https://gitee.com/LezaiYa/FinalSuspect/raw/FinalSus/",
        "https://api.xtreme.net.cn/download/FinalSuspect/",
#endif
    };
    
    public static IReadOnlyList<string> GetInfoFileUrlList(bool allowDesktop = false)
    {
        var list = URLs.ToList();
        if (!allowDesktop && DebugModeManager.AmDebugger) list.RemoveAt(2);
        if (IsChineseUser) list.Reverse();
        return list;
    }
}

public enum FileType
{
    Images,
    Sounds,
    Depends,
    ModNews,
    Languages
}

public enum RemoteType
{
    Github,
    Gitee,
    XtremeApi
}
public enum LocalType
{
    Ban,
    Resources,
    BepInEx,
    Bypass
}

public enum BypassType
{
    Once,
    Longterm,
}