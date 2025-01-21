using System;

namespace FinalSuspect.Helpers;

public static class EnumHelper
{
    /// <summary>
    /// 获取枚举的所有值
    /// </summary>
    /// <typeparam name="T">要获取值的枚举类型</typeparam>
    /// <returns>T 类型的所有值</returns>
    public static T[] GetAllValues<T>() where T : Enum => Enum.GetValues(typeof(T)) as T[];


    /// <summary>
    /// 获取枚举的所有名称
    /// </summary>
    /// <typeparam name="T">要获取名称的枚举类型</typeparam>
    /// <returns>T 类型的所有值的名称</returns>
    public static string[] GetAllNames<T>() where T : Enum => Enum.GetNames(typeof(T));
}
