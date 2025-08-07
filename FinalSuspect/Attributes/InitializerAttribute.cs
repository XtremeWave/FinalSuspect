using System;
using FinalSuspect.Modules.LogHandler;

namespace FinalSuspect.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public abstract class InitializerAttribute<T>(InitializePriority priority) : Attribute
{
    /// <summary>所有初始化方法</summary>
    // ReSharper disable once StaticMemberInGenericType
    private static MethodInfo[] _allInitializers;

    private static readonly LogHandler Logger = Handler(nameof(InitializerAttribute<T>));

    private readonly InitializePriority _priority = priority;

    /// <summary>在初始化时调用的方法</summary>
    private MethodInfo _targetMethod;

    protected InitializerAttribute() : this(InitializePriority.Normal)
    {
    }

    private static void FindInitializers()
    {
        var initializers = new HashSet<InitializerAttribute<T>>(32);

        var assembly = Assembly.GetExecutingAssembly();
        // 在所有类中
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            // 遍历所有方法
            var methods = type.GetMethods();
            foreach (var method in methods)
            {
                // 获取 InitializerAttribute
                var attribute = method.GetCustomAttribute<InitializerAttribute<T>>();
                if (attribute == null) continue;
                // 如果获取到了，则注册
                attribute._targetMethod = method;
                initializers.Add(attribute);
            }
        }

        // 将找到的初始化方法按照优先级排序并转换为数组
        _allInitializers =
        [
            .. initializers.OrderBy(initializer => initializer._priority)
                .Select(initializer => initializer._targetMethod)
        ];
    }

    public static void InitializeAll()
    {
        // 在首次初始化时查找初始化方法
        if (_allInitializers == null) FindInitializers();

        if (_allInitializers == null) return;
        foreach (var initializer in _allInitializers)
        {
            Logger.Info($"初始化: {initializer?.DeclaringType?.Name}.{initializer?.Name}");
            initializer?.Invoke(null, null);
        }
    }
}

public enum InitializePriority
{
    /// <summary>最高优先级，首先执行</summary>
    VeryHigh,

    /// <summary>在默认值之前执行</summary>
    High,

    /// <summary>默认值</summary>
    Normal,

    /// <summary>在默认值之后执行</summary>
    Low,

    /// <summary>最低优先级，最后执行</summary>
    VeryLow
}