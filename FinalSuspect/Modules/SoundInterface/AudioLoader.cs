using System;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using NAudio.Flac;
using NAudio.Wave;
using UnityEngine.Networking;

namespace FinalSuspect.Modules.SoundInterface;

public class AudioLoader
{
    /// <summary>
    /// 异步加载音频文件并转换为 AudioClip（不阻塞主线程）
    /// </summary>
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
        
        float[] floatData = ConvertBytesToFloats(audioData);
        audioData = null; // 立即释放 50% 内存

        AudioClip audioClip = AudioClip.Create("LoadedAudioClip", floatData.Length, 2, 44100, false); // 注意参数修正
        audioClip.SetData(floatData, 0);
    
        floatData = null; // 立即释放剩余 50% 内存
    
        // 建议手动触发垃圾回收（谨慎使用）
        GC.Collect(0, GCCollectionMode.Optimized); 
    
        return audioClip;
    }

// 优化后的字节读取（减少内存拷贝）
    private static async Task<byte[]> ReadAllBytesAsync(string filePath)
    {
        using (FileStream sourceStream = new FileStream(filePath,
                   FileMode.Open, FileAccess.Read, FileShare.Read,
                   bufferSize: 4096, useAsync: true))
        {
            byte[] buffer = new byte[sourceStream.Length];
            await sourceStream.ReadAsync(buffer, 0, (int)sourceStream.Length);
            return buffer;
        }
    }
    private static float[] ConvertBytesToFloats(byte[] audioBytes)
    {
        // 假设音频为 16 位 PCM
        int floatCount = audioBytes.Length / 2;
        float[] floatData = new float[floatCount];

        for (int i = 0; i < floatCount; i++)
        {
            short raw = BitConverter.ToInt16(audioBytes, i * 2);
            floatData[i] = (float)raw / 32768.0f;
        }

        return floatData;
    }

}
