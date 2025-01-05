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
        => Core.Plugin.Logger.Info(text, Tag, true);
    public void Warn(string text)
        => Core.Plugin.Logger.Warn(text, Tag, true);
    public void Error(string text)
        => Core.Plugin.Logger.Error(text, Tag, true);
    public void Fatal(string text)
        => Core.Plugin.Logger.Fatal(text, Tag, true);
    public void Msg(string text)
        => Core.Plugin.Logger.Msg(text, Tag, true);
    public void Exception(Exception ex)
        => Core.Plugin.Logger.Exception(ex, Tag);
}