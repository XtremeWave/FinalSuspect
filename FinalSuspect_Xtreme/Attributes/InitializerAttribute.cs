using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FinalSuspect_Xtreme.Modules;

namespace FinalSuspect_Xtreme.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public abstract class InitializerAttribute<T> : Attribute
{
    /// <summary>所有初始化方法</summary>
    private static MethodInfo[] allInitializers = null;
    private static LogHandler logger = Logger.Handler(nameof(InitializerAttribute<T>));

    public InitializerAttribute() : this(InitializePriority.Normal) { }
    public InitializerAttribute(InitializePriority priority)
    {
        this.priority = priority;
    }

    private readonly InitializePriority priority = InitializePriority.Normal;
    /// <summary>在初始化时调用的方法</summary>
    private MethodInfo targetMethod;

    private static void FindInitializers()
    {
        var initializers = new HashSet<InitializerAttribute<T>>(32);

        // 在 TownOfNewEpic_Xtreme.dll 中
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
                if (attribute != null)
                {
                    // 如果获取到了，则注册
                    attribute.targetMethod = method;
                    initializers.Add(attribute);
                }
            }
        }
        // 将找到的初始化方法按照优先级排序并转换为数组
        allInitializers = initializers.OrderBy(initializer => initializer.priority).Select(initializer => initializer.targetMethod).ToArray();

    }
    public static void InitializeAll()
    {
        // 在首次初始化时查找初始化方法
        if (allInitializers == null)
        {
            FindInitializers();
        }
        foreach (var initializer in allInitializers)
        {
            logger.Info($"初始化: {initializer.DeclaringType.Name}.{initializer.Name}");
            initializer.Invoke(null, null);
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
    VeryLow,
}
