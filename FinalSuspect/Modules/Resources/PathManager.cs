using System;
using System.IO;
using FinalSuspect.Attributes;

namespace FinalSuspect.Modules.Resources;

public static class PathManager
{
    public const string LocalPath_Data = "Final Suspect_Data/";
    public static string DependsSavePath = "BepInEx/core/";
    
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
        string remoteBase = "127.0.0.1";
        switch (remoteType)
        {
            case RemoteType.Github:
                remoteBase = "github.com/XtremeWave/FinalSuspect/raw/FinalSus/Assets";
                break;
            case RemoteType.Gitee:
                remoteBase = "gitee.com/XtremeWave/FinalSuspect/raw/FinalSus/Assets";
                break;
            case RemoteType.XtremeApi:
                remoteBase = "api.xtreme.net.cn/download/FinalSuspect/";
                break;
        }

        return remoteBase;
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
    public static void Init()
    {
        CheckAndCreate(GetLocalPath(LocalType.Resources));
        CheckAndCreate(GetLocalPath(LocalType.Resources) + "Sounds");
        CheckAndCreate(GetLocalPath(LocalType.Resources) + "Images");
        CheckAndCreate(GetLocalPath(LocalType.Ban));
    }

    public static void CheckAndCreate(string path, bool hidden = true)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        if (!hidden) return;
        FileAttributes attributes= File.GetAttributes(path);
        File.SetAttributes(path, attributes | FileAttributes.Hidden);
    }
}

public enum FileType
{
    Images,
    Sounds,
    Depends,
    ModNews
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
    BepInEx
}
