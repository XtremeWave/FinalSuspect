using FinalSuspect.Modules.Core.Plugin.UI;
using TMPro;
#if Windows
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace FinalSuspect.Modules.Resources;

[HarmonyPatch]
public class ModUpdater
{
    public static readonly Dictionary<SupportedLangs, string> announcement = new();

    public static void SetUpdateButtonStatus()
    {
        MainMenu.UpdateButton.SetActive(VersionChecker.IsChecked && VersionChecker.HasUpdate &&
                                        (VersionChecker.FirstStart || VersionChecker.ForceUpdate));
        MainMenu.PlayButton.SetActive(!MainMenu.UpdateButton.activeSelf);
        var buttonText = MainMenu.UpdateButton.transform.FindChild("FontPlacer").GetChild(0)
            .GetComponent<TextMeshPro>();
        buttonText.text =
            $"{(VersionChecker.CanUpdate ? GetString("UpdateRemind.updatePopup") : GetString("UpdateRemind.updateNotice"))}\nv{VersionChecker.ShowVer ?? " ???"}";
    }

    public static void StartUpdate(string url = "waitToSelect")
    {
#if Windows
        if (url == "waitToSelect")
        {
            List<(string, Action)> btns =
            [
                (GetString("UpdateSource.Github"), () => StartUpdate(DownloadUrl_Github)),
                (GetString("UpdateSource.Gitee"), () => StartUpdate(DownloadUrl_Gitee)),
            ];
            if (IsChineseLanguageUser)
            {
                btns.Add((GetString("UpdateSource.GithubMirror"), () => StartUpdate(DownloadUrl_GithubMirror)));
                btns.Add((GetString("UpdateSource.FangKuaiRemote"), () => StartUpdate(DownloadUrl_FangKuaiRemote)));
            }

            btns.Add((GetString(StringNames.Cancel), SetUpdateButtonStatus));
            CustomPopup.Show(GetString("UpdateRemind.updatePopup"), GetString("UpdateSource.Choose:"), btns);
            return;
        }

        var r = new Regex(
            @"^(http|https|ftp)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)?((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.[a-zA-Z]{2,4})(\:[0-9]+)?(/[^/][a-zA-Z0-9\.\,\?\'\\/\+&%\$#\=~_\-@]*)*$");
        if (!r.IsMatch(url))
        {
            CustomPopup.ShowLater(GetString("UpdateResult.Failed_Title"),
                string.Format(GetString("UpdateResult.Failed_Reason_NotFound"), "404 Not Found"),
                [(GetString(StringNames.Okay), SetUpdateButtonStatus)]);
            return;
        }

        CustomPopup.Show(GetString("UpdateRemind.updatePopup"), GetString("Tip.PleaseWait"), null);

        var task = DownloadDLL(url);
        task.ContinueWith(t =>
        {
            var (done, reason) = t.Result;
            var title = done ? GetString("updatePopupTitleDone") : GetString("UpdateResult.Failed_Title");
            var desc = done ? GetString("UpdateResult.Succeed_Text") : reason;
            CustomPopup.ShowLater(title, desc,
                [(GetString(done ? StringNames.ExitGame : StringNames.Okay), done ? Application.Quit : null)]);
            SetUpdateButtonStatus();
        });
    }

    public static void DeleteOldFiles()
    {
        try
        {
            foreach (var path in Directory.EnumerateFiles(
                         Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "*.*"))
            {
                if (!Path.GetExtension(path).Equals(".bak", StringComparison.OrdinalIgnoreCase)) continue;

                Info($"{Path.GetFileName(path)} Deleted", "DeleteOldFiles");
                File.Delete(path);
            }
        }
        catch (Exception e)
        {
            Error($"清除更新残留失败\n{e}", "DeleteOldFiles");
        }
    }

    private static async Task<(bool, string)> DownloadDLL(string url)
    {
        File.Delete(DownloadFileTempPath);
        File.Create(DownloadFileTempPath).Close();

        Msg("Start Downlaod From: " + url, "DownloadDLL");
        Msg("Save To: " + DownloadFileTempPath, "DownloadDLL");
        try
        {
            using var client = new HttpClientDownloadWithProgress(url, DownloadFileTempPath);
            client.ProgressChanged += OnDownloadProgressChanged;
            await client.StartDownload();
            Thread.Sleep(100);
            if (GetMD5HashFromFile(DownloadFileTempPath) != VersionChecker.MD5)
            {
                File.Delete(DownloadFileTempPath);
                return (false, GetString("UpdateResult.Failed_Reason_FileMd5Incorrect"));
            }

            var fileName = Assembly.GetExecutingAssembly().Location;
            File.Move(fileName, fileName + ".bak");
            File.Move(DownloadFileTempPath, fileName);
            return (true, null);
        }
        catch (Exception ex)
        {
            File.Delete(DownloadFileTempPath);
            Error($"更新失败\n{ex.Message}", "DownloadDLL", false);
            return (false, GetString("UpdateResult.Failed_Reason_Ping"));
        }
    }

    private static void OnDownloadProgressChanged(long? totalFileSize, long totalBytesDownloaded,
        double? progressPercentage)
    {
        if (progressPercentage == null) return;
        var msg =
            $"{GetString("Tip.Updating")}\n{totalFileSize / 1000}KB / {totalBytesDownloaded / 1000}KB  -  {(int)progressPercentage}%";
        Info(msg, "DownloadDLL");
        CustomPopup.UpdateTextLater(msg);
    }

    private static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(fileName);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        catch (Exception ex)
        {
            Exception(ex, "GetMD5HashFromFile");
            return "";
        }
#endif
    }
}