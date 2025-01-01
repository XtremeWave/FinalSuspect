using System;

namespace FinalSuspect.Modules.Resources;

public static class PathManager
{
    public static readonly string SOUNDS_PATH = @"Final Suspect_Data/Resources/Audios";
    public static string SavePath = "Final Suspect_Data/Resources/Audios";
    public static readonly string downloadUrl_github = VersionChecker.GithubUrl + "raw/FinalSus/Assets/Sounds/{{sound}}.wav";
    public static readonly string downloadUrl_gitee = VersionChecker.GiteeUrl + "raw/FinalSus/Assets/Sounds/{{sound}}.wav";
    public static readonly string downloadUrl_objectstorage = VersionChecker.ObjectStorageUrl + "Sounds/{{sound}}.wav";
    public static readonly string downloadUrl_aumodsite = VersionChecker.AUModSiteUrl + "Sounds/{{sound}}.wav";

    public const string LocalPath_Data = "Final Suspect_Data/";
    public const string LocalPath_Ban = LocalPath_Data + "Ban/";
    public const string LocalPath_Resource = LocalPath_Data + "Resources/";
    public const string LocalPath_Audio = LocalPath_Resource + "Audios/";
    public const string LocalPath_Image = LocalPath_Resource + "Images/";

    public static string GetRemoteUrl(FileType fileType, RemoteType remoteType)
    {
        return "https://" + GetRemoteBase(remoteType) + fileType.ToString() + "/" + remoteType.ToString();
    }

    public static string GetRemoteBase(RemoteType remoteType)
    {
        string remoteBase = "127.0.0.1";
        switch (remoteType)
        {
            case RemoteType.Github:
                remoteBase = "github.com/XtremeWave/FinalSuspect/";
                break;
            case RemoteType.Gitee:
                remoteBase = "github.com/XtremeWave/FinalSuspect/";
                break;
            case RemoteType.XtremeApi:
                remoteBase = "github.com/XtremeWave/FinalSuspect/";
                break;
        }

        return remoteBase;
    }
}

public enum FileType
{
    
}

public enum RemoteType
{
    Github,
    Gitee,
    XtremeApi
}