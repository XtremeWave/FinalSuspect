using System;
using System.IO;
using FinalSuspect.Attributes;
using UnityEngine;
#if Windows
using System.Linq;
#endif

namespace FinalSuspect.Modules.Resources;

public static class PathManager
{
#if Android
    private static readonly string LocalPath_Data = Application.persistentDataPath + "/FinalSuspect_Data/";
    public static readonly string LANGUAGE_FOLDER_NAME = LocalPath_Data + "Language";
    private static readonly string DependsSavePath = LocalPath_Data + "Depend";
    public static readonly string BAN_LIST_PATH = LocalPath_Data + "BanList.txt";
#else
    private const string LocalPath_Data = "Final Suspect_Data/";
    public const string LANGUAGE_FOLDER_NAME = LocalPath_Data + "Language";
    private const string DependsSavePath = "BepInEx/core/";
    public const string BAN_LIST_PATH = LocalPath_Data + "BanList.txt";
#endif


#if Android
    public const string DownloadFileTempPath = "BepInEx/plugins/FinalSuspect.dll.temp";
#else
    public const string DownloadFileTempPath = "BepInEx/plugins/FinalSuspect.dll.temp";
#endif

    // 下载URL保持不变
    public const string DownloadUrl_Github =
        "https://github.com/Slok7565/FinalSuspect/releases/latest/download/FinalSuspect.dll";

    public const string DownloadUrl_GithubMirror =
        "https://hub.gitmirror.com/https://github.com/Slok7565/FinalSuspect/releases/latest/download/FinalSuspect.dll";

    public const string DownloadUrl_FangKuaiRemote =
        "https://dlhk.fangkuai.fun/FinalSuspect/FinalSuspect.dll";

    public static readonly string BANEDWORDS_FILE_PATH = GetBanFilesPath("BanWords.json");
    public static readonly string DENY_NAME_LIST_PATH = GetBanFilesPath("DenyName.json");

    public static string DownloadUrl_Gitee =
        "https://gitee.com/LezaiYa/FinalSuspectAssets/releases/download/v{showVer}/FinalSuspect.dll";

    private static IReadOnlyList<string> URLs
    {
        get
        {
            var urls = new List<string>
            {
                "https://raw.githubusercontent.com/Slok7565/FinalSuspect_Assets/FinalAsset/",
                "https://raw.githubusercontent.com/Slok7565/FinalSuspect/FinalSus/",
                "https://hub.gitmirror.com/https://github.com/Slok7565/FinalSuspect/raw/FinalSus/",
                "https://hub.gitmirror.com/https://github.com/Slok7565/FinalSuspect_Assets/raw/FinalAsset/",
                "https://gitee.com/LezaiYa/FinalSuspectAssets/raw/main/",
                "https://dlhk.fangkuai.fun/FinalSuspect/",
            };

#if DEBUG && Windows
            // 只有在 Windows 调试模式下才添加桌面路径
            urls.Add($"file:///{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))}/");
#endif
            return urls.AsReadOnly();
        }
    }

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
            RemoteType.FangKuaiRemote => "dlhk.fangkuai.fun/FinalSuspect/",
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
        CheckAndDeleteSLK(LocalPath_Data);
        CheckAndDeleteSLK(DependsSavePath);
    }

    private static void CheckAndCreate(
        string path,
        bool hidden = true,
        bool isFile = false)
    {
        if (path == null) return;

        switch (isFile)
        {
            case true when !File.Exists(path):
                try
                {
                    File.Create(path).Close();
                }
                catch (Exception e)
                {
                    Error($"创建文件失败: {path}, 错误: {e.Message}", "PathManager");
                }

                break;
            case false when !Directory.Exists(path):
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    Error($"创建目录失败: {path}, 错误: {e.Message}", "PathManager");
                }

                break;
        }

#if Windows
        // 只在 Windows 上设置隐藏属性
        try
        {
            var attributes = File.GetAttributes(path);
            File.SetAttributes(path, hidden
                ? attributes | FileAttributes.Hidden
                : attributes & ~FileAttributes.Hidden);
        }
        catch (Exception e)
        {
            Warn($"设置文件属性失败: {path}, 错误: {e.Message}", "PathManager");
        }
#endif
    }

    private static void CheckAndDeleteSLK(string targetFolder)
    {
        if (!Directory.Exists(targetFolder)) return;
        try
        {
            var filesToDelete = Directory.GetFiles(targetFolder, "*.slk", SearchOption.AllDirectories);

            foreach (var file in filesToDelete)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Warn($"删除文件失败: {file}, 错误: {e.Message}", "PathManager");
                }
            }
        }
        catch (Exception e)
        {
            Warn($"删除SLK文件时出错: {e.Message}", "PathManager");
        }
    }

    public static IReadOnlyList<string> GetInfoFileUrlList(bool allowDesktop = false)
    {
        var list = new List<string>(URLs);

#if Android
        allowDesktop = false;
#endif

        if (!allowDesktop)
        {
            list.RemoveAll(url => url.StartsWith("file://"));
        }

        if (IsChineseUser)
            list.Reverse();

        return list.AsReadOnly();
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
    FangKuaiRemote,
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