using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using FinalSuspect.Modules.Resources;
using static FinalSuspect.Modules.SoundInterface.SoundManager;

namespace FinalSuspect.Modules.SoundInterface;

#nullable enable
public static class SoundManager
{
    public static readonly string TAGS_PATH = PathManager.GetResourceFilesPath(FileType.Sounds, "SoundsName.txt");

    public static List<string> CustomAudios = [];
    public static void ReloadTag(bool official = true)
    {
        CustomAudios = [];
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
                XtremeLogger.Info($"Audio Loaded: {line}", "AudioManager");
            }

        }
        catch (Exception ex)
        {
            XtremeLogger.Error("Load Audios Failed\n" + ex, "AudioManager", false);
        }
    }
    public static void Init()
    {
        if (!File.Exists(TAGS_PATH)) File.Create(TAGS_PATH).Close();
        var attributes = File.GetAttributes(TAGS_PATH);
        File.SetAttributes(TAGS_PATH, attributes | FileAttributes.Hidden);

        FinalMusic.InitializeAll();
        
    }
    public static bool ConvertExtension(ref string path)
    {
        if (path == null) return false;
        List<string> extensions = [".wav", ".flac", ".aiff", ".mp3", ".aac", ".ogg", ".m4a"];


        while (!File.Exists(path))
        {
            var currectpath = path;
            var extensionsArray = extensions.ToArray();
            if (extensionsArray.Length == 0) return false;
            var matchingKey = extensions.FirstOrDefault(currectpath.Contains);
            if (matchingKey is null) return false;
            var currentIndex = Array.IndexOf(extensionsArray, matchingKey);
            if (currentIndex == -1)
            {
                return false;
            }
            var nextIndex = (currentIndex + 1) % extensionsArray.Length;
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
                    global::SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.KillSfx, false);
                    break;
                case Sounds.TaskComplete:
                    global::SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskCompleteSound, false);
                    break;
                case Sounds.TaskUpdateSound:
                    global::SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskUpdateSound, false);
                    break;
                case Sounds.ImpTransform:
                    global::SoundManager.Instance.PlaySound(DestroyableSingleton<HnSImpostorScreamSfx>.Instance.HnSOtherImpostorTransformSfx, false, 0.8f);
                    break;
                case Sounds.Yeehawfrom:
                    global::SoundManager.Instance.PlaySound(DestroyableSingleton<HnSImpostorScreamSfx>.Instance.HnSLocalYeehawSfx, false, 0.8f);
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
    ReturnToSimplicity__Slok,
    //Affinity__Slok
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
    public static List<FinalMusic> finalMusics = [];

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
                    XtremeLogger.Info($"Sound Loaded: {line}", "AudioManager");
                }
                soundnum++;
            }

        }
        catch (Exception ex)
        {
            XtremeLogger.Error("Load Audio Failed\n" + ex, "AudioManager", false);
        }


        XtremeLogger.Msg($"{soundnum} Sounds Loaded", "AudioManager");
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
        Path = PathManager.GetResourceFilesPath(FileType.Sounds, FileName + ".wav");
        CurrectAudioStates = LastAudioStates = ConvertExtension(ref Path) ? AudiosStates.Exist : AudiosStates.NotExist;

        lock (finalMusicsLock)
        {
            var file = finalMusics.Find(x => x.FileName == FileName);
            if (file != null)
            {
                XtremeLogger.Info($"Replace {file.FileName}", "FinalMusic");
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