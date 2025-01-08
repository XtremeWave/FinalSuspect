using System;
using FinalSuspect.Modules.Core.Game;

namespace FinalSuspect.Modules.Core.Plugin;

public static class SystemEnvironment
{
    public static void SetEnvironmentVariables()
    {
        // 将最近打开的 FinalSuspect 应用程序文件夹的路径设置为用户环境变量
        Environment.SetEnvironmentVariable("FINAL_SUSPECT_DIR_ROOT", Environment.CurrentDirectory, EnvironmentVariableTarget.User);
        // 将日志文件夹的路径设置为用户环境变量
        Environment.SetEnvironmentVariable("FINAL_SUSPECT_DIR_LOGS", Utils.GetLogFolder().FullName, EnvironmentVariableTarget.User);
    }
}