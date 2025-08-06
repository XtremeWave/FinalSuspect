using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using NAudio.Wave;

// ReSharper disable RedundantAssignment

namespace FinalSuspect.ClientActions.FeatureItems.MyMusic;

/// <summary>
/// 跨平台异步音频加载器
/// 支持后缀：.wav .mp3 .ogg .aiff .aif .flac
/// WebGL 下自动回退（不支持 NAudio）
/// </summary>
public static class AudioLoader
{
    #region Public API

    /// <summary>异步加载任意受支持的音频文件</summary>
    public static async Task<AudioClip> LoadAudioClipAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var ext = Path.GetExtension(filePath).ToLowerInvariant();

        switch (ext)
        {
            case ".wav":
                try
                {
                    return await LoadWavAsync(filePath);
                }
                catch (Exception e)
                {
                    Test(e.Message);
                    return null;
                }
            case ".mp3":
            case ".aiff":
            case ".aif":
            case ".flac":
                try
                {
                    return await LoadWithNAudioAsync(filePath);
                }
                catch (Exception e)
                {
                    Test(e.Message);
                    return null;
                }
            default:
                Error($"[AudioLoader] Unsupported extension: {ext}", "AudioLoader");
                return null;
        }
    }

    #endregion

    #region IO Helper

    private static async Task<byte[]> ReadAllBytesAsync(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        var buffer = new byte[fs.Length];
        _ = await fs.ReadAsync(buffer, 0, (int)fs.Length);
        return buffer;
    }

    #endregion

    #region WAV (native parser)

    private static async Task<AudioClip> LoadWavAsync(string path)
    {
        var bytes = await ReadAllBytesAsync(path);
        if (TryParseWavHeader(bytes, out var info))
            return await CreateClipFromPcm(info.data, info.sampleRate, info.channels, info.bitDepth);

        Warn("[AudioLoader] WAV header invalid, trying raw 16-bit fallback", "AudioLoader");
        return await CreateClipFromRaw(bytes);
    }

    private static bool TryParseWavHeader(byte[] data,
        out (byte[] data, int sampleRate, int channels, int bitDepth) info)
    {
        info = default;
        if (data.Length < 44 ||
            Encoding.ASCII.GetString(data, 0, 4) != "RIFF" ||
            Encoding.ASCII.GetString(data, 8, 4) != "WAVE")
            return false;

        try
        {
            var fmt = FindChunk(data, "fmt ");
            if (fmt < 0) return false;

            int format = BitConverter.ToInt16(data, fmt + 8);
            if (format != 1 && format != 3) return false;

            int channels = BitConverter.ToInt16(data, fmt + 10);
            var sampleRate = BitConverter.ToInt32(data, fmt + 12);
            int bitDepth = BitConverter.ToInt16(data, fmt + 22);

            var dataChunk = FindChunk(data, "data");
            if (dataChunk < 0) return false;

            var dataSize = BitConverter.ToInt32(data, dataChunk + 4);
            var dataStart = dataChunk + 8;

            var pcm = new byte[dataSize];
            Buffer.BlockCopy(data, dataStart, pcm, 0, dataSize);

            info = (pcm, sampleRate, channels, bitDepth);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static int FindChunk(byte[] data, string id)
    {
        var idx = 12;
        while (idx < data.Length - 8)
        {
            if (Encoding.ASCII.GetString(data, idx, 4) == id)
                return idx;
            idx += 8 + BitConverter.ToInt32(data, idx + 4);
        }

        return -1;
    }

    #endregion

    #region NAudio (AIFF/FLAC/WebGL fallback)

    private static async Task<AudioClip> LoadWithNAudioAsync(string path)
    {
        using var reader = new AudioFileReader(path); // 自动识别 AIFF/FLAC/MP3/WAV
        return await BuildClipFromNAudio(reader);
    }

    private static async Task<AudioClip> BuildClipFromNAudio(AudioFileReader reader)
    {
        var fmt = reader.WaveFormat;
        var channels = fmt.Channels;
        var sampleRate = fmt.SampleRate;

        // 将耗时操作移到后台线程
        var (clip, samples) = await Task.Run(() =>
        {
            var totalSamples = reader.Length / (fmt.BitsPerSample / 8);
            var frames = totalSamples / channels;
            var samples = new float[totalSamples];

            var read = reader.Read(samples, 0, samples.Length);
            if (read != samples.Length)
            {
                Warn("[AudioLoader] NAudio read length mismatch.", "AudioLoader");
            }

            return (AudioClip.Create("NAudioClip", (int)frames, channels, sampleRate, false), samples);
        });

        // 在主线程中设置数据
        clip.SetData(samples, 0);
        return clip;
    }

    #endregion

    #region PCM → AudioClip

    private static readonly HashSet<int> SupportedBits = [8, 16, 24, 32, 64];

    private static async Task<AudioClip> CreateClipFromPcm(byte[] pcm, int sr, int ch, int bits)
    {
        if (!SupportedBits.Contains(bits))
        {
            Debug.LogError($"[AudioLoader] Unsupported bit depth: {bits}");
            return null;
        }

        var data = await Task.Run(() => ConvertBytesToFloats(pcm, bits));
        var frames = data.Length / ch;
        var clip = AudioClip.Create("LoadedAudioClip", frames, ch, sr, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static async Task<AudioClip> CreateClipFromRaw(byte[] raw)
    {
        const int ch = 2, sr = 44100;
        var data = await Task.Run(() => ConvertBytesToFloats(raw, 16));
        var frames = data.Length / ch;
        var clip = AudioClip.Create("LoadedAudioClip", frames, ch, sr, false);
        clip.SetData(data, 0);
        return clip;
    }

    #endregion

    #region Bit Depth Conversion

    private static float[] ConvertBytesToFloats(byte[] src, int bits)
    {
        return bits switch
        {
            8 => Convert8Bit(src),
            16 => Convert16Bit(src),
            24 => Convert24Bit(src),
            32 => Convert32BitInt(src),
            64 => Convert32Float(src),
            _ => throw new NotSupportedException()
        };
    }

    private static float[] Convert8Bit(byte[] src)
    {
        var dst = new float[src.Length];
        for (var i = 0; i < src.Length; i++)
            dst[i] = (src[i] - 128) / 128f;
        return dst;
    }

    private static float[] Convert16Bit(byte[] src)
    {
        if (src.Length % 2 != 0) throw new ArgumentException("odd length");
        var dst = new float[src.Length / 2];
        unsafe
        {
            fixed (byte* pByte = src)
            fixed (float* pFloat = dst)
            {
                var pShort = (short*)pByte;
                for (var i = 0; i < dst.Length; i++)
                    pFloat[i] = pShort[i] / 32768f;
            }
        }

        return dst;
    }

    private static float[] Convert24Bit(byte[] src)
    {
        if (src.Length % 3 != 0) throw new ArgumentException("length not multiple of 3");
        var dst = new float[src.Length / 3];
        for (var i = 0; i < dst.Length; i++)
        {
            var o = i * 3;
            var s = src[o] | (src[o + 1] << 8) | (src[o + 2] << 16);
            if ((s & 0x800000) != 0) s |= unchecked((int)0xFF000000);
            dst[i] = s / 8388608f;
        }

        return dst;
    }

    private static float[] Convert32BitInt(byte[] src)
    {
        if (src.Length % 4 != 0) throw new ArgumentException("odd length");
        var dst = new float[src.Length / 4];
        unsafe
        {
            fixed (byte* pByte = src)
            fixed (float* pFloat = dst)
            {
                var pInt = (int*)pByte;
                for (var i = 0; i < dst.Length; i++)
                    pFloat[i] = pInt[i] / 2147483648f;
            }
        }

        return dst;
    }

    private static float[] Convert32Float(byte[] src)
    {
        if (src.Length % 4 != 0) throw new ArgumentException("odd length");
        var dst = new float[src.Length / 4];
        Buffer.BlockCopy(src, 0, dst, 0, src.Length);
        return dst;
    }

    #endregion
}