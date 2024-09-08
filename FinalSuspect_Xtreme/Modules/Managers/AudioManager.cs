using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FinalSuspect_Xtreme.Modules.SoundInterface;
using UnityEngine;

namespace FinalSuspect_Xtreme.Modules.Managers;

#nullable enable
public static class AudioManager
{
    public static readonly string TAGS_DIRECTORY_PATH = @"FinalSuspect_Data/Resources/AudioNames";

    public static IReadOnlyDictionary<string, bool> FinalSuspectMusic => FinalSuspectOfficialMusic;

    public static IReadOnlyDictionary<string, bool> AllMusics => CustomMusic.Concat(FinalSuspectOfficialMusic)
        .ToDictionary(x => x.Key.ToString(), x => x.Value, StringComparer.OrdinalIgnoreCase);
    public static IReadOnlyDictionary<string, bool> AllSounds => FinalSuspectSounds;

    public static IReadOnlyDictionary<string, bool> AllFiles => AllSounds.Concat(AllMusics)
        .ToDictionary(x => x.Key.ToString(), x => x.Value, StringComparer.OrdinalIgnoreCase);
    public static IReadOnlyDictionary<string, bool> AllFinalSuspect => AllSounds.Concat(FinalSuspectMusic)
        .ToDictionary(x => x.Key.ToString(), x => x.Value, StringComparer.OrdinalIgnoreCase);


    private static Dictionary<string, bool> CustomMusic = new();
    public static Dictionary<string, bool> FinalSuspectOfficialMusic = new();
    public static Dictionary<string, bool> FinalSuspectSounds = new();


    public static List<string> FinalSuspectOfficialMusicList = new()
    {
        "GongXiFaCaiLiuDeHua",
        "NeverGonnaGiveYouUp",
                "RejoiceThisSEASONRespectThisWORLD",

        "SpringRejoicesinParallelUniverses",
        "AFamiliarPromise",
        "GuardianandDream",
        "HeartGuidedbyLight",
        "HopeStillExists",
        "Mendax",
        "MendaxsTimeForExperiment",
        "StarfallIntoDarkness",
        "StarsFallWithDomeCrumbles",
        "TheDomeofTruth",
        "TheTruthFadesAway",
        "unavoidable",



    };

    public static List<string> NotUp = new()
    {
                "RejoiceThisSEASONRespectThisWORLD",


    };

    public static List<string> FinalSuspectSoundList = new()
    {
    };
    public static void ReloadTag(string? sound)
    {
        if (sound == null)
        {
            Init();
            return;
        }

        CustomMusic.Remove(sound);

        string path = $"{TAGS_DIRECTORY_PATH}{sound}.json";
        if (ConvertExtension(ref path))
        {
            try { ReadTagsFromFile(path); }
            catch (Exception ex)
            {
                Logger.Error($"Load Sounds From: {path} Failed\n" + ex.ToString(), "AudioManager", false);
            }
        }
    }

    public static void Init()
    {
        CustomMusic = new();
        FinalSuspectOfficialMusic = new();
        foreach (var file in FinalSuspectOfficialMusicList)
        {
            FinalSuspectOfficialMusic.TryAdd(file, false);
        }
        FinalSuspectSounds = new();
        foreach (var file in FinalSuspectSoundList)
        {
            FinalSuspectSounds.TryAdd(file, false);
        }

        if (!Directory.Exists(TAGS_DIRECTORY_PATH)) Directory.CreateDirectory(TAGS_DIRECTORY_PATH);
        if (!Directory.Exists(CustomSoundsManager.SOUNDS_PATH)) Directory.CreateDirectory(CustomSoundsManager.SOUNDS_PATH);

        var files = Directory.EnumerateFiles(TAGS_DIRECTORY_PATH, "*.json", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            try { ReadTagsFromFile(file); }
            catch (Exception ex)
            {
                Logger.Error($"Load Tag From: {file} Failed\n" + ex.ToString(), "AudioManager", false);
            }
        }

        Logger.Msg($"{CustomMusic.Count} Sounds Loaded", "AudioManager");
    }
    public static void ReadTagsFromFile(string path)
    {
        if (path.ToLower().Contains("template")) return;
        string sound = Path.GetFileNameWithoutExtension(path);
        if (sound != null && !AllSounds.ContainsKey(sound) && !FinalSuspectMusic.ContainsKey(sound))
        {
            CustomMusic.TryAdd(sound, false);
            Logger.Info($"Sound Loaded: {sound}", "AudioManager");
        }
    }
#nullable disable
    public static bool ConvertExtension(ref string path)
    {
        if (path == null) return false;
        List<string> extensions = new() { ".wav", ".flac", ".aiff", ".mp3", ".aac", ".ogg", ".m4a" };


        while (!File.Exists(path))
        {
            var currectpath = path;
            var extensionsArray = extensions.ToArray();
            if (extensionsArray.Length == 0) return false;
            string matchingKey = extensions.FirstOrDefault(key => currectpath.Contains(key));
            if (matchingKey is null) return false;
            int currentIndex = Array.IndexOf(extensionsArray, matchingKey);
            if (currentIndex == -1)
            {
                return false;
            }
            int nextIndex = (currentIndex + 1) % extensionsArray.Length;
            path = path.Replace(matchingKey, extensionsArray[nextIndex]);
            extensions.Remove(matchingKey);
        }

        return true;
    }


    public static async Task<AudioClip> LoadAudioClipAsync(string filePath, string name)
    {
        if (!ConvertExtension(ref filePath))
        {
            Logger.Error("File does not exist: " + filePath, "LoadAudioClip");
            return null;
        }

        byte[] audioData;

        try
        {
            audioData = await ReadAllBytesAsync(filePath);
        }
        catch (Exception e)
        {
            Logger.Error("Failed to read file: " + filePath + "\n" + e.Message, "LoadAudioClip");
            return null;
        }

        AudioClip audioClip = AudioClip.Create(name, audioData.Length / 2, 2, 44100, false);
        float[] floatData = ConvertBytesToFloats(audioData);

        audioClip.SetData(floatData, 0);

        return audioClip;
    }

#nullable enable
    public static Dictionary<string, AudioClip> AllSoundClips = new();
    // 异步读取文件字节数据
    private static async Task<byte[]> ReadAllBytesAsync(string filePath)
    {
        using (FileStream sourceStream = new FileStream(filePath,
            FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true))
        {
            MemoryStream memoryStream = new MemoryStream();
            await sourceStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
    private static float[] ConvertBytesToFloats(byte[] audioBytes)
    {
        float[] floatData = new float[audioBytes.Length / 2];

        for (int i = 0; i < floatData.Length; i++)
        {
            floatData[i] = BitConverter.ToInt16(audioBytes, i * 2) / 32768.0f;
        }

        return floatData;
    }
}
#nullable disable