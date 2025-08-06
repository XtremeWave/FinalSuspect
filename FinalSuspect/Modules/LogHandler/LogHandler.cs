using System;

namespace FinalSuspect.Modules.LogHandler;

internal class LogHandler(string tag) : ILogHandler
{
    public string Tag { get; } = tag;

    public void Info(string text)
    {
        FinalLogger.Info(text, Tag);
    }

    public void Warn(string text)
    {
        FinalLogger.Warn(text, Tag);
    }

    public void Error(string text)
    {
        FinalLogger.Error(text, Tag);
    }

    public void Fatal(string text)
    {
        FinalLogger.Fatal(text, Tag);
    }

    public void Msg(string text)
    {
        FinalLogger.Msg(text, Tag);
    }

    public void Exception(Exception ex)
    {
        FinalLogger.Exception(ex, Tag);
    }
}