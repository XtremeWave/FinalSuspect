using System;
using System.IO;
using System.Threading.Tasks;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using FinalSuspect.Modules.Resources;
using UnityEngine;

namespace FinalSuspect.ClientActions.FeatureItems.MyMusic;

public static class AudioManager
{
    public static readonly List<string> Extensions = [".zip", ".wav", ".flac", ".aif", ".aiff", ".mp3"];

    public static void ReloadTag()
    {
        try
        {
            Init();
            var files = Directory.GetFiles(GetLocalPath(LocalType.Resources) + "Musics");

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                if (EnumHelper.GetAllNames<SupportedMusics>().Skip(1).Any(x =>
                    {
                        var part = x.ToString().Split("__");
                        return fileName.Contains(part[0]);
                    }))
                    continue;

                if (string.IsNullOrWhiteSpace(fileName))
                    continue;

                FinalMusic.CreateMusic(fileName);
                Info($"Audio Loaded: {fileName}", "AudioManager");
            }
        }
        catch (Exception ex)
        {
            Error("Load Audios Failed\n" + ex, "AudioManager", false);
        }
    }

    private static void Init()
    {
        FinalMusic.InitializeAll();
    }

    public static bool ConvertExtension(ref string path)
    {
        if (path == null) return false;

        var extensions = Extensions.ToArray().ToList();

        while (!File.Exists(path))
        {
            var currentPath = path;
            var extensionsArray = extensions.ToArray();
            if (extensionsArray.Length == 0) return false;
            var matchingKey = extensions.FirstOrDefault(currentPath.Contains);
            if (matchingKey is null) return false;
            var currentIndex = Array.IndexOf(extensionsArray, matchingKey);
            if (currentIndex == -1) return false;

            var nextIndex = (currentIndex + 1) % extensionsArray.Length;
            path = path.Replace(matchingKey, extensionsArray[nextIndex]);
            extensions.Remove(matchingKey);
        }

        return true;
    }

    public static bool ConvertExtensionRemote(ref string path)
    {
        if (path == null) return false;

        var extensions = Extensions.ToArray().ToList();
        var ext = Path.GetExtension(path).ToLowerInvariant();

        var currentIndex = extensions.IndexOf(ext);

        var nextIndex = (currentIndex + 1) % extensions.Count;

        path = path.Replace(ext, extensions[nextIndex]);

        return nextIndex != 0;
    }

    public static void PlaySound(byte playerID, Sounds sound)
    {
        if (PlayerControl.LocalPlayer.PlayerId != playerID) return;

        var clip = sound switch
        {
            Sounds.KillSound => PlayerControl.LocalPlayer.KillSfx,
            Sounds.TaskComplete => DestroyableSingleton<HudManager>.Instance.TaskCompleteSound,
            Sounds.TaskUpdateSound => DestroyableSingleton<HudManager>.Instance.TaskUpdateSound,
            Sounds.ImpTransform => DestroyableSingleton<HnSImpostorScreamSfx>.Instance.HnSOtherImpostorTransformSfx,
            Sounds.Yeehawfrom => DestroyableSingleton<HnSImpostorScreamSfx>.Instance.HnSLocalYeehawSfx,
            _ => throw new ArgumentOutOfRangeException(nameof(sound), sound, null)
        };
        if (clip == null) return;
        SoundManager.Instance.PlaySound(clip, false);
    }
}

public enum SupportedMusics
{
    UnOfficial,
    FinalSuspect__Slok,

    // ## World Music
    GongXiFaCai__Andy_Lau,
    NeverGonnaGiveYouUp__Rick_Astley,
    CountingStars__One_Republic,

    // ## Mod Music
    // 专辑
    ChasingDawn__Slok,
    ReturnToSimplicity2__Slok,
    GatheredEchoes__Slok,

    // 回望末端时
    Trajectory__Slok,

    // 其他
    Affinity__Slok,
    TidalSurge__Slok,
    ReturnToSimplicity__Slok,

    // 这里是EmberVeins的Demo曲
    DawnmistEchoes__Slok,
    TrailOfTruth__Slok,
    Interlude__Slok,
    Fractured__Slok, // 这首会有大用
    StruggleAgainstFadingFlame__Slok,
    ElegyOfFracturedVow__Slok,
    VestigiumSplendoris__Slok,
    FinisAnteInitium__Slok,
}

public enum AudiosStates
{
    NotExist,
    DownLoading,
    Exist,
    Playing,
    Pausing,
    Parsing
}

public class FinalMusic
{
    public static readonly List<FinalMusic> Musics = [];

    private static readonly object finalMusicsLock = new();
    public string Author;
    public AudioClip Clip;

    public SupportedMusics CurrentAudio;
    public AudiosStates CurrentAudioStates;
    public string FileName;
    public string FilePath;
    public AudiosStates LastAudioStates;

    public string Name;

    public bool PlayAsMainMenuMusic;

    public bool UnOfficial;


    public static void InitializeAll()
    {
        foreach (var file in EnumHelper.GetAllValues<SupportedMusics>().ToList()) CreateMusic(music: file);
    }

    public static void CreateMusic(string name = "", SupportedMusics music = SupportedMusics.UnOfficial)
    {
        var mus = new FinalMusic();
        mus.Create(name, music);
    }

    public async Task Load()
    {
        if (CurrentAudioStates != AudiosStates.Exist || Clip != null) return;
        var task = AudioLoader.LoadAudioClipAsync(FilePath);
        _ = new MainThreadTask(() =>
        {
            LastAudioStates = CurrentAudioStates = AudiosStates.Parsing;
            MyMusicPanel.RefreshTagList();
        }, "Update Audio States Start");
        await task;
        _ = new MainThreadTask(() =>
        {
            if (task.Result)
                Clip = task.Result;
            LastAudioStates = CurrentAudioStates = Clip ? AudiosStates.Exist : AudiosStates.NotExist;
            MyMusicPanel.RefreshTagList();
        }, "Update Audio States Finish");
    }

    private void Create(string name, SupportedMusics music)
    {
        if (music != SupportedMusics.UnOfficial)
        {
            var part = music.ToString().Split("__");
            FileName = part[0] + ".wav";
            Name = GetString($"Mus.{part[0]}");
            Author = part[1].Replace("_", " ");
        }
        else
        {
            FileName = Name = name;
            Author = "";
        }

        UnOfficial = music == SupportedMusics.UnOfficial;
        CurrentAudio = music;
        FilePath = GetResourceFilesPath(FileType.Musics, FileName);

        CurrentAudioStates = LastAudioStates =
            AudioManager.ConvertExtension(ref FilePath) ? AudiosStates.Exist : AudiosStates.NotExist;

        var ext = Path.GetExtension(FilePath)?.ToLowerInvariant();

        if (!AudioManager.Extensions.Contains(ext)) return;

        lock (finalMusicsLock)
        {
            var file = Musics.Find(x => x.FileName == FileName);
            if (file != null)
            {
                file.FilePath = FilePath;
                if (CurrentAudioStates is AudiosStates.NotExist)
                    file.CurrentAudioStates = file.LastAudioStates = CurrentAudioStates;
            }
            else if (Name != string.Empty)
            {
                Musics.Add(this);
            }
        }
    }
}