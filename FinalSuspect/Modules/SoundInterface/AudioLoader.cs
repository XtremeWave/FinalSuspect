using System;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace FinalSuspect.Modules.SoundInterface;

public class AudioLoader
{
    // 静态构造函数用于预热
    static AudioLoader()
    {
        Warmup();
    }

    private static void Warmup()
    {
        // 预热ConvertBytesToFloats的JIT编译
        byte[] dummyBytes = new byte[2];
        ConvertBytesToFloats(dummyBytes);

        // 预热Unity音频相关的初始化
        var warmupClip = AudioClip.Create("Warmup", 1, 1, 44100, false);
        warmupClip.SetData(new float[] { 0 }, 0);
        UnityEngine.Object.Destroy(warmupClip);
    }

    public static async Task<AudioClip> LoadAudioClipAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File does not exist: " + filePath);
            return null;
        }

        byte[] audioData;

        try
        {
            audioData = await ReadAllBytesAsync(filePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to read file: " + filePath + "\n" + e.Message);
            return null;
        }

        // 在后台线程执行转换
        float[] floatData = await Task.Run(() => ConvertBytesToFloats(audioData));
        audioData = null; // 及时释放内存

        // 确保后续Unity操作在主线程执行
        var audioClip = AudioClip.Create("LoadedAudioClip", floatData.Length, 2, 44100, false);
        audioClip.SetData(floatData, 0);

        return audioClip;
    }

    private static async Task<byte[]> ReadAllBytesAsync(string filePath)
    {
        using (var sourceStream = new FileStream(
                   filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
        {
            byte[] buffer = new byte[sourceStream.Length];
            await sourceStream.ReadAsync(buffer, 0, (int)sourceStream.Length);
            return buffer;
        }
    }

    private static float[] ConvertBytesToFloats(byte[] audioBytes)
    {
        int floatCount = audioBytes.Length / 2;
        float[] floatData = new float[floatCount];

        // 使用unsafe代码加速转换（可选）
        unsafe
        {
            fixed (byte* bytePtr = audioBytes)
            fixed (float* floatPtr = floatData)
            {
                short* src = (short*)bytePtr;
                float* dst = floatPtr;
                for (int i = 0; i < floatCount; i++)
                {
                    dst[i] = src[i] / 32768.0f;
                }
            }
        }

        return floatData;
    }
}