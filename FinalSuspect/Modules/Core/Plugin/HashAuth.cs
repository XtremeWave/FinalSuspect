using System.Security.Cryptography;
using System.Text;

namespace FinalSuspect.Modules.Core.Plugin;

public class HashAuth(string hashValue, string salt = null, HashAlgorithm algorithm = null)
{
    private readonly HashAlgorithm algorithm = algorithm ?? SHA256.Create();

    /// <summary>
    ///     验证字符串是否匹配哈希值
    /// </summary>
    public bool CheckString(string value)
    {
        var hash = CalculateHash(value);
        return hashValue == hash;
    }

    /// <summary>
    ///     计算字符串的哈希值
    /// </summary>
    private string CalculateHash(string source)
    {
        return CalculateHash(source, salt, algorithm);
    }

    /// <summary>
    ///     计算带盐值的哈希值
    /// </summary>
    /// <param name="source">源字符串</param>
    /// <param name="salt">盐值</param>
    /// <param name="algorithm">哈希算法实例</param>
    private static string CalculateHash(string source, string salt, HashAlgorithm algorithm = null)
    {
        // 初始化算法
        algorithm ??= SHA256.Create();

        // 添加盐值
        if (salt != null) source += salt;

        // 字符串转字节数组
        var sourceBytes = Encoding.UTF8.GetBytes(source);

        // 计算哈希值
        var hashBytes = algorithm.ComputeHash(sourceBytes);

        // 转换为十六进制字符串
        var sb = new StringBuilder();
        foreach (var b in hashBytes)
            sb.Append(b.ToString("x2")); // 每个字节转为2位十六进制

        return sb.ToString();
    }

    /// <summary>
    ///     通过未哈希值创建验证器（仅用于测试）
    /// </summary>
    /// <param name="value">原始值</param>
    /// <param name="salt">盐值</param>
    /// <remarks>
    ///     此方法会同时生成哈希值并输出日志，仅用于开发测试阶段
    /// </remarks>
    public static HashAuth CreateByUnhashedValue(string value, string salt = null)
    {
        // 计算哈希值
        var algorithm = SHA256.Create();
        var hashValue = CalculateHash(value, salt, algorithm);

        // 输出计算结果日志
        Info($"哈希值计算结果: {value} => {hashValue} {(salt == null ? "" : $"(salt: {salt})")}", "HashAuth");
        Warn("请将上方生成的值粘贴到源代码中", "HashAuth");

        // 返回新实例
        return new HashAuth(hashValue, salt, algorithm);
    }
}