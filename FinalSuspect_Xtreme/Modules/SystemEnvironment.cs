using System;

namespace FinalSuspect_Xtreme.Modules;

public static class SystemEnvironment
{
    public static void SetEnvironmentVariables()
    {
        // ユーザ環境変数に最近開かれたFinalSuspect_Xtremeアモアスフォルダのパスを設定
        Environment.SetEnvironmentVariable("FINAL_SUSPECT__DIR_ROOT", Environment.CurrentDirectory, EnvironmentVariableTarget.User);
    }
}