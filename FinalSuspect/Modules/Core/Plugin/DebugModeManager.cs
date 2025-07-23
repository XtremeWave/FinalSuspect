namespace FinalSuspect.Modules.Core.Plugin;

public static class DebugModeManager
{
    // 当此项启用时，将激活不影响正常游戏的调试功能（详细日志/游戏外调试显示等）。
    // 同时，可在游戏内选项菜单中启用调试模式。
    private static bool AmDebugger { get; set; }
#if DEBUG
         = true;
#endif
    public static bool IsDebugMode => AmDebugger;

    public static void Auth(HashAuth auth, string input)
    {
        // AmDebugger = 启用调试版本 || 通过调试密钥认证
        AmDebugger = AmDebugger || auth.CheckString(input);
    }
}