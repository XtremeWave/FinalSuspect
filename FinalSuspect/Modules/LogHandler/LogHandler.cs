using System;

namespace FinalSuspect.Modules.LogHandler;

class LogHandler : ILogHandler
{
    public string Tag { get; }
    public LogHandler(string tag)
    {
        Tag = tag;
    }

    public void Info(string text)
        => XtremeLogger.Info(text, Tag);
    public void Warn(string text)
        => XtremeLogger.Warn(text, Tag);
    public void Error(string text)
        => XtremeLogger.Error(text, Tag);
    public void Fatal(string text)
        => XtremeLogger.Fatal(text, Tag);
    public void Msg(string text)
        => XtremeLogger.Msg(text, Tag);
    public void Exception(Exception ex)
        => XtremeLogger.Exception(ex, Tag);
}