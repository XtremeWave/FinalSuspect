using System;

namespace FinalSuspect.Helpers;

public static class EnumHelper
{
    /// <summary>
    ///     获取枚举的所有值
    /// </summary>
    /// <typeparam name="T">要获取值的枚举类型</typeparam>
    /// <returns>T 类型的所有值</returns>
    public static T[] GetAllValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)) as T[];
    }

    /// <summary>
    ///     获取枚举的所有名称
    /// </summary>
    /// <typeparam name="T">要获取名称的枚举类型</typeparam>
    /// <returns>T 类型的所有值的名称</returns>
    public static string[] GetAllNames<T>() where T : Enum
    {
        return Enum.GetNames(typeof(T));
    }

    /// <summary>添加枚举值（返回新值，原值不变）</summary>
    public static T AddFlag<T>(this T value, T flag) where T : Enum
    {
        // 仅支持整数基类型（byte、sbyte、short、ushort、int、uint、long、ulong）
        dynamic v = value;
        dynamic f = flag;
        return (T)(v | f);
    }

    /// <summary>移除枚举值（返回新值，原值不变）</summary>
    public static T RemoveFlag<T>(this T value, T flag) where T : Enum
    {
        dynamic v = value;
        dynamic f = flag;
        return (T)(v & ~f);
    }

    /// <summary>
    /// 根据 <paramref name="addFlag"/> 决定添加或移除某个标志位。
    /// true 时调用 AddFlag，false 时调用 RemoveFlag。
    /// </summary>
    public static T AddOrRemoveFlag<T>(this T value, T flag, bool addFlag) where T : Enum
    {
        return addFlag ? value.AddFlag(flag) : value.RemoveFlag(flag);
    }

    public static bool HasAnyFlag<T>(this T value, T flags) where T : Enum
    {
        var v = (int)(object)value;
        var f = (int)(object)flags;
        return (v & f) != 0;
    }
}