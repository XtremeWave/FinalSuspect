using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using FinalSuspect.Attributes;
using FinalSuspect.Patches.Game_Vanilla;
using LogLevel = BepInEx.Logging.LogLevel;

namespace FinalSuspect.Modules.Core.Plugin;

internal static class FinalLogger
{
    private static bool isEnable;
    private static readonly List<string> disableList = [];
    private static readonly List<string> sendToGameList = [];
    public static bool isDetail;
    public static bool isAlsoInGame = false;

    [PluginModuleInitializer(InitializePriority.VeryHigh)]
    public static void OnInitialization()
    {
        Main.Logger = BepInEx.Logging.Logger.CreateLogSource("FinalSuspect");
        Enable();
        Disable("SwitchSystem");
        Disable("ModNews");
        Disable("CancelPet");
        Disable("Get Remote");
        Disable(("SetModAnnouncements"));
        if (!DebugModeManager.IsDebugMode)
        {
            Disable("Download Resources");
            Disable("GetAnnouncements");
            Disable("GetConfigs");
            Disable("Downloader");
        }

        isDetail = DebugModeManager.IsDebugMode;
    }

    public static void Enable()
    {
        isEnable = true;
    }

    public static void Enable(string tag, bool toGame = false)
    {
        disableList.Remove(tag);
        if (toGame && !sendToGameList.Contains(tag)) sendToGameList.Add(tag);
        else sendToGameList.Remove(tag);
    }

    public static void Disable()
    {
        isEnable = false;
    }

    public static void Disable(string tag)
    {
        if (!disableList.Contains(tag)) disableList.Add(tag);
    }

    public static void SendInGame(string text)
    {
        if (!isEnable) return;
        NotificationPopperPatch.NotificationPop(text);
    }

    private static void SendToFile(string text, LogLevel level = LogLevel.Info, string tag = "", bool escapeCRLF = true,
        int lineNumber = 0, string fileName = "")
    {
        if (!isEnable || disableList.Contains(tag)) return;
        var logger = Main.Logger;
        var t = DateTime.Now.ToString("HH:mm:ss");
        if (sendToGameList.Contains(tag) || isAlsoInGame) SendInGame($"[{tag}]{text}");
        if (escapeCRLF)
            text = text.Replace("\r", "\\r").Replace("\n", "\\n");
        var log_text = $"[{t}][{tag}]{text}";
        if (isDetail && DebugModeManager.IsDebugMode)
        {
            StackFrame stack = new(2);
            var className = stack.GetMethod()?.ReflectedType?.Name ?? "NullClass";
            var memberName = stack.GetMethod()?.Name ?? "NullMember";
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
            case LogLevel.None:
            case LogLevel.All:
            default:
                logger.LogWarning("Error:Invalid LogLevel");
                logger.LogInfo(log_text);
                break;
        }
    }

    public static void Test(object content = null, string tag = "======= Test =======", bool escapeCRLF = true,
        [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "")
    {
        SendToFile((content ?? "Test Message").ToString(), LogLevel.Debug, tag, escapeCRLF, lineNumber, fileName);
    }

    public static void Info(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0,
        [CallerFilePath] string fileName = "")
    {
        SendToFile(text, LogLevel.Info, tag, escapeCRLF, lineNumber, fileName);
    }

    public static void Warn(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0,
        [CallerFilePath] string fileName = "")
    {
        SendToFile(text, LogLevel.Warning, tag, escapeCRLF, lineNumber, fileName);
    }

    public static void Error(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0,
        [CallerFilePath] string fileName = "")
    {
        SendToFile(text, LogLevel.Error, tag, escapeCRLF, lineNumber, fileName);
    }

    public static void Fatal(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0,
        [CallerFilePath] string fileName = "")
    {
        SendToFile(text, LogLevel.Fatal, tag, escapeCRLF, lineNumber, fileName);
    }

    public static void Msg(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0,
        [CallerFilePath] string fileName = "")
    {
        SendToFile(text, LogLevel.Message, tag, escapeCRLF, lineNumber, fileName);
    }

    public static void Exception(Exception ex, string tag, [CallerLineNumber] int lineNumber = 0,
        [CallerFilePath] string fileName = "")
    {
        SendToFile(ex.ToString(), LogLevel.Error, tag, false, lineNumber, fileName);
    }

    public static void CurrentMethod([CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "")
    {
        StackFrame stack = new(1);
        Msg(
            $"\"{stack.GetMethod()?.ReflectedType?.Name}.{stack.GetMethod()?.Name}\" Called in \"{Path.GetFileName(fileName)}({lineNumber})\"",
            "Method");
    }

    public static LogHandler.LogHandler Handler(string tag)
    {
        return new LogHandler.LogHandler(tag);
    }
}