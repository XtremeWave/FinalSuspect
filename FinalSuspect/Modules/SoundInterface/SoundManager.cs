using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using FinalSuspect.Modules.Random;
using FinalSuspect.Modules.Resources;
using UnityEngine;
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
            while (sr.ReadLine() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                XtremeMusic.CreateMusic(line);
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

        XtremeMusic.InitializeAll();
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

public enum SupportedMusics
{
    UnOfficial,
    
    // ## World Music
    GongXiFaCai__Andy_Lau,
    NeverGonnaGiveYouUp__Rick_Astley,
    CountingStars__One_Republic,
    
    //20250214
    
    // ## Mod Music
    TidalSurge__Slok,
    TrailOfTruth__Slok, 
    Interlude__Slok, 
    Fractured__Slok, 
    ElegyOfFracturedVow__Slok, 
    VestigiumSplendoris__Slok,
    ReturnToSimplicity__Slok,

    //20250214
    Affinity__Slok,
    Inceps_Plus_InProgress__Slok
    
}
public enum AudiosStates
{
    NotExist,
    IsDownLoading,
    Exist,
    IsPlaying,
    DownLoadSucceedNotice,
    DownLoadFailureNotice,
    IsLoading,
}

public class XtremeMusic
{
    public static List<XtremeMusic> musics = [];

    public string Name;
    public string FileName;
    public string Author;
    public string Path;
    public AudioClip Clip;

    public SupportedMusics CurrectAudio;
    public AudiosStates CurrectAudioStates;
    public AudiosStates LastAudioStates;
    
    public bool UnOfficial;
    public bool unpublished;


    public static async void InitializeAll()
    {
        foreach (var file in EnumHelper.GetAllValues<SupportedMusics>().ToList())
        {
            CreateMusic(music: file);
        }
        
        var soundnum = 0;
        try
        {
            using StreamReader sr = new(TAGS_PATH);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                CreateMusic(line);
                XtremeLogger.Info($"Sound Loaded: {line}", "AudioManager");
                soundnum++;
            }
        }
        catch (Exception ex)
        {
            XtremeLogger.Error("Load Audio Failed\n" + ex, "AudioManager", false);
        }
        
        XtremeLogger.Msg($"{soundnum} Custom Sounds Loaded", "AudioManager");
    }
    
    private static readonly object finalMusicsLock = new();

    public static void CreateMusic(string name = "", SupportedMusics music = SupportedMusics.UnOfficial)
    {
        var mus = new XtremeMusic();
        mus.Create(name, music);
    }

    public static async Task LoadClip(SupportedMusics music = SupportedMusics.UnOfficial)
    {
        var mus = musics.FirstOrDefault(x => x.CurrectAudio == music);
        if (mus != null)
            await mus.Load();
    }

    private async Task Load()
    {
        if (CurrectAudioStates != AudiosStates.Exist) return;
        var task = AudioLoader.LoadAudioClipAsync(Path);
        _ = new LateTask(() =>
        {
            LastAudioStates = CurrectAudioStates = AudiosStates.IsLoading;
            MyMusicPanel.RefreshTagList();
        }, 0.01F, "");
        await task;
        _ = new LateTask(() =>
        {
            if (task.Result != null)
                Clip = task.Result;
            LastAudioStates = CurrectAudioStates = Clip != null ? AudiosStates.Exist : AudiosStates.NotExist;
            MyMusicPanel.RefreshTagList();
        }, 0.01F, "");
    }

    private void Create(string name, SupportedMusics music)
    {
        if (music != SupportedMusics.UnOfficial)
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

        UnOfficial = music == SupportedMusics.UnOfficial;
        CurrectAudio = music;
        Path = PathManager.GetResourceFilesPath(FileType.Sounds, FileName + ".wav");
        CurrectAudioStates = LastAudioStates = ConvertExtension(ref Path) ? AudiosStates.Exist : AudiosStates.NotExist;
        
        lock (finalMusicsLock)
        {
            var file = musics.Find(x => x.FileName == FileName);
            if (file != null)
            {
                file.Path = Path;
                if (file.CurrectAudioStates is AudiosStates.DownLoadFailureNotice or AudiosStates.DownLoadSucceedNotice 
                    || CurrectAudioStates is AudiosStates.NotExist)
                {
                    file.CurrectAudioStates = file.LastAudioStates = CurrectAudioStates;
                }
            }
            else if (Name != string.Empty)
                musics.Add(this);
        }
    }
}
