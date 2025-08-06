using System;
using System.IO;
using FinalSuspect.Attributes;

namespace FinalSuspect.Modules.Resources;

public static class PathManager
{
    private const string LocalPath_Data = "Final Suspect_Data/";
    public const string LANGUAGE_FOLDER_NAME = LocalPath_Data + "Language";
    private const string DependsSavePath = "BepInEx/core/";
    public const string DownloadFileTempPath = "BepInEx/plugins/FinalSuspect.dll.temp";
    public const string BAN_LIST_PATH = LocalPath_Data + "BanList.txt";

    public const string downloadUrl_github =
        "https://github.com/Slok7565/FinalSuspect/releases/latest/download/FinalSuspect.dll";

    public const string downloadUrl_githubMirror =
        "https://hub.gitmirror.com/https://github.com/Slok7565/FinalSuspect/releases/latest/download/FinalSuspect.dll";

    public static string downloadUrl_gitee =
        "https://gitee.com/LezaiYa/FinalSuspectAssets/releases/download/v{showVer}/FinalSuspect.dll";

    public static readonly string BANEDWORDS_FILE_PATH = GetBanFilesPath("BanWords.json");
    public static readonly string DENY_NAME_LIST_PATH = GetBanFilesPath("DenyName.json");


    private static IReadOnlyList<string> URLs => new List<string>
    {
#if DEBUG
        "https://raw.githubusercontent.com/Slok7565/FinalSuspect_Assets/FinalAsset/",
        "https://raw.githubusercontent.com/Slok7565/FinalSuspect/FinalSus/",
        "https://hub.gitmirror.com/https://github.com/Slok7565/FinalSuspect/FinalSus/",
        "https://hub.gitmirror.com/https://github.com/Slok7565/FinalSuspect_Assets/FinalAsset/",
        "https://gitee.com/LezaiYa/FinalSuspectAssets/raw/main/",
        $"file:///{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))}/",
#else
        "https://raw.githubusercontent.com/Slok7565/FinalSuspect/FinalSus/",
        "https://hub.gitmirror.com/https://github.com/Slok7565/FinalSuspect/raw/FinalSus/",
        "https://hub.gitmirror.com/https://github.com/Slok7565/FinalSuspect_Assets/raw/FinalAsset/",
        "https://gitee.com/LezaiYa/FinalSuspectAssets/raw/main/",
#endif
    };

    public static string GetFile(FileType fileType, RemoteType remoteType, string file)
    {
        return GetRemoteUrl(fileType, remoteType) + file;
    }

    public static string GetPackageFile(string packageName, RemoteType remoteType, string file)
    {
        return "https://" + GetRemoteBase(remoteType) + "Packages/" + packageName + "/" + file;
    }

    private static string GetRemoteUrl(FileType fileType, RemoteType remoteType)
    {
        return "https://" + GetRemoteBase(remoteType) + fileType + "/";
    }

    private static string GetRemoteBase(RemoteType remoteType)
    {
        var remoteBase = remoteType switch
        {
            RemoteType.GithubMirror => "hub.gitmirror.com/https://github.com/Slok7565/FinalSuspect/raw/FinalSus/",
            RemoteType.GithubMirror_Assets =>
                "hub.gitmirror.com/https://github.com/Slok7565/FinalSuspect_Assets/raw/FinalAsset/",
            RemoteType.Gitee => "gitee.com/LezaiYa/FinalSuspectAssets/raw/main/",
            RemoteType.Github => "github.com/Slok7565/FinalSuspect/raw/FinalSus/",
            RemoteType.Github_Assets => "github.com/Slok7565/FinalSuspect_Assets/raw/FinalAsset/",
            _ => "127.0.0.1"
        };

        return remoteBase + "Assets/";
    }

    public static string GetLocalFilePath(FileType fileType, string file)
    {
        return fileType switch
        {
            FileType.Depends => GetLocalPath(LocalType.BepInEx) + file,
            _ => GetResourceFilesPath(fileType, file)
        };
    }

    public static string GetLocalPath(LocalType localType)
    {
        if (localType == LocalType.BepInEx)
            return DependsSavePath;
        return LocalPath_Data + localType + "/";
    }

    public static string GetResourceFilesPath(FileType fileType, string file)
    {
        return GetLocalPath(LocalType.Resources) + fileType + "/" + file;
    }

    private static string GetBanFilesPath(string file)
    {
        return GetLocalPath(LocalType.Ban) + file;
    }

    [PluginModuleInitializer(InitializePriority.High)]
    public static void InitializePaths()
    {
        CheckAndCreate(GetLocalPath(LocalType.Resources), false);
        CheckAndCreate(GetLocalPath(LocalType.Resources) + "Musics", false);
        CheckAndCreate(GetLocalPath(LocalType.Resources) + "SoundEffects");
        CheckAndCreate(GetLocalPath(LocalType.Resources) + "Images");
        CheckAndCreate(GetLocalPath(LocalType.Resources) + "Languages", false);
        CheckAndCreate(LANGUAGE_FOLDER_NAME, false);

        CheckAndCreate(GetLocalPath(LocalType.Ban));
        CheckAndCreate(BANEDWORDS_FILE_PATH, false, true);
        CheckAndCreate(DENY_NAME_LIST_PATH, false, true);

        CheckAndCreate(GetLocalPath(LocalType.NameTag));

        // 防止崩溃的必要措施
        CheckAndDeleteXWR(LocalPath_Data);
        CheckAndDeleteXWR(DependsSavePath);
    }

    private static void CheckAndCreate(string path, bool hidden = true, bool isFile = false)
    {
        if (path == null) return;

        switch (isFile)
        {
            case true when !File.Exists(path):
                File.Create(path);
                break;
            case false when !Directory.Exists(path):
                Directory.CreateDirectory(path);
                break;
        }

        var attributes = File.GetAttributes(path);
        File.SetAttributes(path, hidden
            ? attributes | FileAttributes.Hidden
            : attributes & ~FileAttributes.Hidden);
    }

    private static void CheckAndDeleteXWR(string targetFolder)
    {
        if (!Directory.Exists(targetFolder)) return;
        try
        {
            var filesToDelete = Directory.GetFiles(targetFolder, "*.xwr", SearchOption.AllDirectories);

            foreach (var file in filesToDelete) File.Delete(file);
        }
        catch
        {
            /* ignored */
        }
    }

    public static IReadOnlyList<string> GetInfoFileUrlList(bool allowDesktop = false)
    {
        var list = URLs.ToList();
        if (!allowDesktop && DebugModeManager.IsDebugMode)
            list.RemoveAt(5);
        if (IsChineseUser) list.Reverse();
        return list;
    }
}

public enum FileType
{
    Unknown,
    Images,
    Musics,
    SoundEffects,
    Depends,
    ModNews,
    Languages
}

public enum RemoteType
{
    GithubMirror,
    GithubMirror_Assets,
    Gitee,
    Github_Assets,
    Github,
}

public enum LocalType
{
    Ban,
    Resources,
    BepInEx,
    NameTag
}