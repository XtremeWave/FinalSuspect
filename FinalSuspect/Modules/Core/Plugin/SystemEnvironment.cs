using System;
using System.Threading.Tasks;
using FinalSuspect.Modules.Core.Game;

namespace FinalSuspect.Modules.Core.Plugin;

public static class SystemEnvironment
{
    public static async Task SetEnvironmentVariablesAsync()
    {
        // 将最近打开的 FinalSuspect 应用程序文件夹的路径设置为用户环境变量
        await Task.Run(() => Environment.SetEnvironmentVariable("FINAL_SUSPECT_DIR_ROOT", Environment.CurrentDirectory, EnvironmentVariableTarget.User));
        await Task.Run(()=> XtremeLogger.Info("ROOT SET COMPLETE", "SetEnvironmentVariables"));
        // 将日志文件夹的路径设置为用户环境变量
        var logFolderPath = await Task.Run(() => Utils.GetLogFolder().FullName);
        await Task.Run(() => Environment.SetEnvironmentVariable("FINAL_SUSPECT_DIR_LOGS", logFolderPath, EnvironmentVariableTarget.User));
        await Task.Run(()=> XtremeLogger.Info("LOGS SET COMPLETE", "SetEnvironmentVariables"));
    }
}