namespace FinalSuspect_Xtreme.Attributes;

/// <summary>
/// 用于在 <see cref="Main.Load"/> 中启动时初始化的方法
/// 在静态方法前面加上 [PluginModuleInitializer]，可以在启动时自动调用
/// 可以使用 [PluginModuleInitializer(InitializePriority.High)] 来指定调用顺序
/// </summary>
public sealed class PluginModuleInitializerAttribute : InitializerAttribute<PluginModuleInitializerAttribute>
{
    public PluginModuleInitializerAttribute(InitializePriority priority = InitializePriority.Normal) : base(priority) { }
}