using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using FinalSuspect.Modules.Resources;
using FinalSuspect.Modules.SoundInterface;
using static FinalSuspect.Modules.Managers.AudioManager;
using static FinalSuspect.Translator;
using UnityEngine;
using HarmonyLib;
using TMPro;

namespace FinalSuspect.Modules.Managers;

#nullable enable
public static class AudioManager
{
    public static readonly string TAGS_PATH = @"Final Suspect_Data/Resources/AudioNames.txt";

    public static List<string> CustomAudios = new();
    public static void ReloadTag(bool official = true)
    {
        CustomAudios = new();
#nullable disable
        if (official)
        {
            Init();
            return;
        }
        

        try
        {
            using StreamReader sr = new(TAGS_PATH);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                new FinalMusic(line);
                Logger.Info($"Audio Loaded: {line}", "AudioManager");
            }

        }
        catch (Exception ex)
        {
            Logger.Error($"Load Audios Failed\n" + ex.ToString(), "AudioManager", false);
        }
    }
    public static void Init()
    {
        if (!File.Exists(TAGS_PATH)) File.Create(TAGS_PATH).Close();
        if (!Directory.Exists(PathManager.SOUNDS_PATH)) Directory.CreateDirectory(PathManager.SOUNDS_PATH);

        FileAttributes attributes = File.GetAttributes(TAGS_PATH);
        File.SetAttributes(TAGS_PATH, attributes | FileAttributes.Hidden);

        FinalMusic.InitializeAll();
        
    }
    public static bool ConvertExtension(ref string path)
    {
        if (path == null) return false;
        List<string> extensions = new() { ".wav", ".flac", ".aiff", ".mp3", ".aac", ".ogg", ".m4a" };


        while (!File.Exists(path))
        {
            var currectpath = path;
            var extensionsArray = extensions.ToArray();
            if (extensionsArray.Length == 0) return false;
            string matchingKey = extensions.FirstOrDefault(currectpath.Contains);
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

    public static void PlaySound(byte playerID, Sounds sound)
    {
        if (PlayerControl.LocalPlayer.PlayerId == playerID)
        {
            switch (sound)
            {
                case Sounds.KillSound:
                    SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.KillSfx, false, 1f);
                    break;
                case Sounds.TaskComplete:
                    SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskCompleteSound, false, 1f);
                    break;
                case Sounds.TaskUpdateSound:
                    SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskUpdateSound, false, 1f);
                    break;
                case Sounds.ImpTransform:
                    SoundManager.Instance.PlaySound(DestroyableSingleton<HnSImpostorScreamSfx>.Instance.HnSOtherImpostorTransformSfx, false, 0.8f);
                    break;
                case Sounds.Yeehawfrom:
                    SoundManager.Instance.PlaySound(DestroyableSingleton<HnSImpostorScreamSfx>.Instance.HnSLocalYeehawSfx, false, 0.8f);
                    break;
            }
        }
    }
}
public enum FSAudios
{
    UnOfficial,
    GongXiFaCai__Andy_Lau,
    NeverGonnaGiveYouUp__Rick_Astley,
    TidalSurge__Slok,
    TrailOfTruth__Slok, 
    Interlude__Slok, 
    Fractured__Slok, 
    ElegyOfFracturedVow__Slok, 
    VestigiumSplendoris__Slok,
    ReturnToSimplicity__Slok
}
public enum AudiosStates
{
    NotExist,
    IsDownLoading,
    Exist,
    IsPlaying,
    DownLoadSucceedNotice,
    DownLoadFailureNotice,
}

public class FinalMusic
{
    public static List<FinalMusic> finalMusics = new();

    public string Name;
    public string FileName;
    public string Author;
    public string Path;

    public FSAudios CurrectAudio;
    public AudiosStates CurrectAudioStates;
    public AudiosStates LastAudioStates;

    public bool UnOfficial;
    public bool unpublished;


    public static void InitializeAll()
    {
        foreach (var file in EnumHelper.GetAllValues<FSAudios>().ToList())
        {
            new FinalMusic(music: file);
        }

        var soundnum = 0;
        try
        {
            using StreamReader sr = new(TAGS_PATH);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line != null)
                {
                    new FinalMusic(line);
                    Logger.Info($"Sound Loaded: {line}", "AudioManager");
                }
                soundnum++;
            }

        }
        catch (Exception ex)
        {
            Logger.Error($"Load Audio Failed\n" + ex.ToString(), "AudioManager", false);
        }


        Logger.Msg($"{soundnum} Sounds Loaded", "AudioManager");
    }
    private static readonly object finalMusicsLock = new();

    public FinalMusic(string name = "", FSAudios music = FSAudios.UnOfficial)
    {
        if (music != FSAudios.UnOfficial)
        {
            var Part = music.ToString().Split("__");
            FileName = Part[0];
            Name = GetString($"Mus.{Part[0]}");
            Author = Part[1].Replace("_", " ");
        }
        else
        {
            CustomAudios.Remove(name);
            CustomAudios.Add(name);
            FileName = Name = name;
            Author = "";
        }
        UnOfficial = music == FSAudios.UnOfficial;
        CurrectAudio = music;
        Path = PathManager.SOUNDS_PATH + "/" + FileName + ".wav";
        CurrectAudioStates = LastAudioStates = ConvertExtension(ref Path) ? AudiosStates.Exist : AudiosStates.NotExist;

        lock (finalMusicsLock)
        {
            var file = finalMusics.Find(x => x.FileName == FileName);
            if (file != null)
            {
                Logger.Info($"Replace {file.FileName}", "FinalMusic");
                var index = finalMusics.IndexOf(file);
                if (file.CurrectAudioStates == AudiosStates.IsPlaying) CurrectAudioStates = AudiosStates.IsPlaying;
                finalMusics.RemoveAt(index);
                finalMusics.Insert(index, this);

            }
            else if (Name != string.Empty)
                finalMusics.Add(this);
        }
    }
}