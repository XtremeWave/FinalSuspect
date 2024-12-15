﻿using System;

namespace FinalSuspect.Modules;

public static class SystemEnvironment
{
    public static void SetEnvironmentVariables()
    {
        // 将最近打开的 FinalSuspect 应用程序文件夹的路径设置为用户环境变量
        Environment.SetEnvironmentVariable("FINAL_SUSPECT_DIR_ROOT", Environment.CurrentDirectory, EnvironmentVariableTarget.User);
    }
}