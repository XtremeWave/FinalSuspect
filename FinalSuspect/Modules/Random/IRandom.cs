using System;

namespace FinalSuspect.Modules.Random;

public interface IRandom
{
    // == static ==
    // IRandomを実装するクラスのリスト
    public static readonly Dictionary<int, Type> randomTypes = new()
    {
        { 0, typeof(NetRandomWrapper) }, //Default
        { 1, typeof(NetRandomWrapper) },
        { 2, typeof(HashRandomWrapper) },
        { 3, typeof(Xorshift) },
        { 4, typeof(MersenneTwister) }
    };

    public static IRandom Instance { get; private set; }

    /// <summary>0以上maxValue未満の乱数を生成します。</summary>
    public int Next(int maxValue);

    /// <summary>minValue以上maxValue未満の乱数を生成します。</summary>
    public int Next(int minValue, int maxValue);

    public static void SetInstance(IRandom instance)
    {
        if (instance != null)
            Instance = instance;
    }

    public static void SetInstanceById(int id)
    {
        if (randomTypes.TryGetValue(id, out var type))
        {
            // 現在のインスタンスがnull または 現在のインスタンスの型が指定typeと一致しない
            if (Instance == null || Instance.GetType() != type)
                Instance = Activator.CreateInstance(type) as IRandom ?? Instance;
        }
        else
        {
            Warn($"无效ID: {id}", "IRandom.SetInstanceById");
        }
    }
}