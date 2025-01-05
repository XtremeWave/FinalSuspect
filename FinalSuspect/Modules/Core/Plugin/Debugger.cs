using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using FinalSuspect.Patches.Game_Vanilla;
using LogLevel = BepInEx.Logging.LogLevel;

namespace FinalSuspect.Modules.Core.Plugin;

class Webhook
{
    public static void Send(string text)
    {
        if (Main.WebhookURL.Value == "none") return;
        HttpClient httpClient = new();
        Dictionary<string, string> strs = new()
        {
            { "content", text },
            { "username", "FinalSuspect-Debugger" },
            { "avatar_url", "https://npm.elemecdn.com/hexo-static@1.0.1/img/avatar.webp" }
        };
        TaskAwaiter<HttpResponseMessage> awaiter = httpClient.PostAsync(
            Main.WebhookURL.Value, new FormUrlEncodedContent(strs)).GetAwaiter();
        awaiter.GetResult();
    }
}
class Logger
{
    public static bool isEnable;
    public static List<string> disableList = new();
    public static List<string> sendToGameList = new();
    public static bool isDetail = false;
    public static bool isAlsoInGame = false;
    public static void Enable() => isEnable = true;
    public static void Disable() => isEnable = false;
    public static void Enable(string tag, bool toGame = false)
    {
        disableList.Remove(tag);
        if (toGame && !sendToGameList.Contains(tag)) sendToGameList.Add(tag);
        else sendToGameList.Remove(tag);
    }
    public static void Disable(string tag) { if (!disableList.Contains(tag)) disableList.Add(tag); }
    public static void SendInGame(string text, bool isAlways = false)
    {
        if (!isEnable) return;
        NotificationPopperPatch.NotificationPop(text);
    }
    private static void SendToFile(string text, LogLevel level = LogLevel.Info, string tag = "", bool escapeCRLF = true, int lineNumber = 0, string fileName = "")
    {
        if (!isEnable || disableList.Contains(tag)) return;
        var logger = Main.Logger;
        string t = DateTime.Now.ToString("HH:mm:ss");
        if (sendToGameList.Contains(tag) || isAlsoInGame) SendInGame($"[{tag}]{text}");
        if (escapeCRLF)
            text = text.Replace("\r", "\\r").Replace("\n", "\\n");
        string log_text = $"[{t}][{tag}]{text}";
        if (isDetail && DebugModeManager.AmDebugger)
        {
            StackFrame stack = new(2);
            string className = stack.GetMethod().ReflectedType.Name;
            string memberName = stack.GetMethod().Name;
            log_text = $"[{t}][{className}.{memberName}({Path.GetFileName(fileName)}:{lineNumber})][{tag}]{text}";
        }
        switch (level)
        {
            case LogLevel.Info:
                logger.LogInfo(log_text);
                break;
            case LogLevel.Warning:
                logger.LogWarning(log_text);
                break;
            case LogLevel.Error:
                logger.LogError(log_text);
                break;
            case LogLevel.Fatal:
                logger.LogFatal(log_text);
                break;
            case LogLevel.Message:
                logger.LogMessage(log_text);
                break;
            case LogLevel.Debug:
                logger.LogFatal(log_text);
                break;
            default:
                logger.LogWarning("Error:Invalid LogLevel");
                logger.LogInfo(log_text);
                break;
        }
    }
    public static void Test(object content, string tag = "======= Test =======", bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "") =>
        SendToFile(content.ToString(), LogLevel.Debug, tag, escapeCRLF, lineNumber, fileName);
    public static void Info(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "") =>
        SendToFile(text, LogLevel.Info, tag, escapeCRLF, lineNumber, fileName);
    public static void Warn(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "") =>
        SendToFile(text, LogLevel.Warning, tag, escapeCRLF, lineNumber, fileName);
    public static void Error(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "") =>
        SendToFile(text, LogLevel.Error, tag, escapeCRLF, lineNumber, fileName);
    public static void Fatal(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "") =>
        SendToFile(text, LogLevel.Fatal, tag, escapeCRLF, lineNumber, fileName);
    public static void Msg(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "") =>
        SendToFile(text, LogLevel.Message, tag, escapeCRLF, lineNumber, fileName);
    public static void Exception(Exception ex, string tag, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "") =>
        SendToFile(ex.ToString(), LogLevel.Error, tag, false, lineNumber, fileName);
    public static void CurrentMethod([CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "")
    {
        StackFrame stack = new(1);
        Logger.Msg($"\"{stack.GetMethod().ReflectedType.Name}.{stack.GetMethod().Name}\" Called in \"{Path.GetFileName(fileName)}({lineNumber})\"", "Method");
    }

    public static LogHandler.LogHandler Handler(string tag)
        => new(tag);
}