using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable RedundantAssignment

namespace FinalSuspect.ClientActions.FeatureItems.MyMusic;

public static class AudioLoader
{
    static AudioLoader()
    {
        Warmup();
    }

    private static void Warmup()
    {
        var dummyBytes = new byte[2];
        ConvertBytesToFloats(dummyBytes);

        var warmupClip = AudioClip.Create("Warmup", 1, 1, 44100, false);
        warmupClip.SetData(new float[] { 0 }, 0);
        Object.Destroy(warmupClip);
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
        catch (Exception e)
        {
            Debug.LogError("Failed to read file: " + filePath + "\n" + e.Message);
            return null;
        }

        try
        {
            // 优先尝试解析WAV文件头
            if (TryParseWavHeader(audioData, out var audioInfo))
            {
                return await CreateClipFromPcm(
                    audioInfo.data,
                    audioInfo.sampleRate,
                    audioInfo.channels,
                    audioInfo.bitDepth
                );
            }
            // 尝试其他格式或默认处理
            else
            {
                Debug.LogWarning("Unrecognized format, attempting default processing");
                return await CreateClipFromRaw(audioData);
            }
        }
        finally
        {
            // 确保及时释放内存
            audioData = null;
        }
    }

    private static async Task<AudioClip> CreateClipFromPcm(
        byte[] pcmData,
        int sampleRate,
        int channels,
        int bitDepth)
    {
        if (bitDepth != 16)
        {
            Debug.LogError($"Unsupported bit depth: {bitDepth}. Only 16-bit supported");
            return null;
        }

        var floatData = await Task.Run(() => ConvertBytesToFloats(pcmData));
        pcmData = null; // 立即释放PCM数据

        var samplesPerChannel = floatData.Length / channels;
        var audioClip = AudioClip.Create("LoadedAudioClip", samplesPerChannel, channels, sampleRate, false);
        audioClip.SetData(floatData, 0);
        return audioClip;
    }

    private static async Task<AudioClip> CreateClipFromRaw(byte[] rawData)
    {
        // 默认参数（双声道/44.1kHz）
        const int defaultChannels = 2;
        const int defaultSampleRate = 44100;

        var floatData = await Task.Run(() => ConvertBytesToFloats(rawData));
        rawData = null; // 立即释放原始数据

        var samplesPerChannel = floatData.Length / defaultChannels;
        var audioClip =
            AudioClip.Create("LoadedAudioClip", samplesPerChannel, defaultChannels, defaultSampleRate, false);
        audioClip.SetData(floatData, 0);
        return audioClip;
    }

    private static bool TryParseWavHeader(byte[] data,
        out (byte[] data, int sampleRate, int channels, int bitDepth) audioInfo)
    {
        audioInfo = default;

        // 基本WAV文件检查（RIFF头）
        if (data.Length < 44 ||
            System.Text.Encoding.ASCII.GetString(data, 0, 4) != "RIFF" ||
            System.Text.Encoding.ASCII.GetString(data, 8, 4) != "WAVE")
        {
            return false;
        }

        try
        {
            // 查找"fmt "块
            var fmtIndex = FindChunk(data, "fmt ");
            if (fmtIndex < 0) return false;

            _ = BitConverter.ToInt32(data, fmtIndex + 4);
            int format = BitConverter.ToInt16(data, fmtIndex + 8);
            if (format != 1) return false; // 仅支持PCM格式

            int channels = BitConverter.ToInt16(data, fmtIndex + 10);
            var sampleRate = BitConverter.ToInt32(data, fmtIndex + 12);
            int bitDepth = BitConverter.ToInt16(data, fmtIndex + 22);

            // 查找"data"块
            var dataIndex = FindChunk(data, "data");
            if (dataIndex < 0) return false;

            var dataSize = BitConverter.ToInt32(data, dataIndex + 4);
            var dataStart = dataIndex + 8;

            // 提取纯音频数据
            var audioData = new byte[dataSize];
            Buffer.BlockCopy(data, dataStart, audioData, 0, dataSize);

            audioInfo = (audioData, sampleRate, channels, bitDepth);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static int FindChunk(byte[] data, string chunkId)
    {
        const int headerSize = 12; // RIFF头大小
        var index = headerSize;

        while (index < data.Length - 8)
        {
            var id = System.Text.Encoding.ASCII.GetString(data, index, 4);
            var size = BitConverter.ToInt32(data, index + 4);

            if (id == chunkId)
            {
                return index;
            }

            index += 8 + size; // 移动到下一个区块
        }

        return -1;
    }

    private static async Task<byte[]> ReadAllBytesAsync(string filePath)
    {
        await using var sourceStream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true
        );

        var buffer = new byte[sourceStream.Length];
        _ = await sourceStream.ReadAsync(buffer, 0, (int)sourceStream.Length);
        return buffer;
    }

    private static float[] ConvertBytesToFloats(byte[] audioBytes)
    {
        if (audioBytes.Length % 2 != 0)
        {
            throw new ArgumentException("Audio byte array must be even length (16-bit samples)");
        }

        var floatCount = audioBytes.Length / 2;
        var floatData = new float[floatCount];

        unsafe
        {
            fixed (byte* bytePtr = audioBytes)
            fixed (float* floatPtr = floatData)
            {
                var src = (short*)bytePtr;
                for (var i = 0; i < floatCount; i++)
                {
                    floatPtr[i] = src[i] / 32768.0f;
                }
            }
        }

        return floatData;
    }
}