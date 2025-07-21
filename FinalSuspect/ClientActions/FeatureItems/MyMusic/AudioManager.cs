using System;
using System.IO;
using System.Threading.Tasks;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using FinalSuspect.Modules.Resources;
using UnityEngine;

namespace FinalSuspect.ClientActions.FeatureItems.MyMusic;

#nullable enable
public static class AudioManager
{
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
            var files = Directory.GetFiles(GetLocalPath(LocalType.Resources) + "Musics");

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                if (EnumHelper.GetAllNames<SupportedMusics>().Skip(1).Any(x => fileName.Contains(x)))
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
        List<string> extensions = [".zip", ".wav", ".flac", ".aiff", ".mp3", ".aac", ".ogg", ".m4a"];

        while (!File.Exists(path))
        {
            var currectpath = path;
            var extensionsArray = extensions.ToArray();
            if (extensionsArray.Length == 0) return false;
            var matchingKey = extensions.FirstOrDefault(currectpath.Contains);
            if (matchingKey is null) return false;
            var currentIndex = Array.IndexOf(extensionsArray, matchingKey);
            if (currentIndex == -1) return false;

            var nextIndex = (currentIndex + 1) % extensionsArray.Length;
            path = path.Replace(matchingKey, extensionsArray[nextIndex]);
            extensions.Remove(matchingKey);
        }

        return true;
    }

    public static void PlaySound(byte playerID, Sounds sound)
    {
        if (PlayerControl.LocalPlayer.PlayerId != playerID) return;
        switch (sound)
        {
            case Sounds.KillSound:
                SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.KillSfx, false);
                break;
            case Sounds.TaskComplete:
                SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskCompleteSound,
                    false);
                break;
            case Sounds.TaskUpdateSound:
                SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskUpdateSound,
                    false);
                break;
            case Sounds.ImpTransform:
                SoundManager.Instance.PlaySound(
                    DestroyableSingleton<HnSImpostorScreamSfx>.Instance.HnSOtherImpostorTransformSfx, false, 0.8f);
                break;
            case Sounds.Yeehawfrom:
                SoundManager.Instance.PlaySound(
                    DestroyableSingleton<HnSImpostorScreamSfx>.Instance.HnSLocalYeehawSfx, false, 0.8f);
                break;
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

    // ## Mod Music
    // 专辑
    ChasingDawn__Slok,
    ReturnToSimplicity2__Slok,

    //
    Affinity__Slok,
    TidalSurge__Slok,
    ReturnToSimplicity__Slok,
    FinalSuspect__Slok,

    // 这里是EmberVeins的Demo曲
    TrailOfTruth__Slok,
    Interlude__Slok,
    Fractured__Slok, // 这首会有大用
    StruggleAgainstFadingFlame__Slok,
    ElegyOfFracturedVow__Slok,
    VestigiumSplendoris__Slok
}

public enum AudiosStates
{
    NotExist,
    IsDownLoading,
    Exist,
    IsPlaying,
    DownLoadSucceedNotice,
    DownLoadFailureNotice,
    IsLoading
}

public class FinalMusic
{
    public static readonly List<FinalMusic> musics = [];

    private static readonly object finalMusicsLock = new();
    public string Author;
    public AudioClip Clip;

    public SupportedMusics CurrectAudio;
    public AudiosStates CurrectAudioStates;
    public string FileName;

    public bool IsMainMenuMusic;
    public AudiosStates LastAudioStates;

    public string Name;
    public string Path;

    public bool UnOfficial;
    //public bool unpublished;


    public static void InitializeAll()
    {
        foreach (var file in EnumHelper.GetAllValues<SupportedMusics>().ToList()) CreateMusic(music: file);
    }

    public static void CreateMusic(string name = "", SupportedMusics music = SupportedMusics.UnOfficial)
    {
        var mus = new FinalMusic();
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
        _ = new MainThreadTask(() =>
        {
            LastAudioStates = CurrectAudioStates = AudiosStates.IsLoading;
            //MyMusicPanel.RefreshTagList();
        }, "Update Audio States");
        await task;
        _ = new MainThreadTask(() =>
        {
            if (task.Result)
                Clip = task.Result;
            LastAudioStates = CurrectAudioStates = Clip ? AudiosStates.Exist : AudiosStates.NotExist;
            //MyMusicPanel.RefreshTagList();
        }, "Update Audio States");
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
            AudioManager.CustomAudios.Remove(name);
            AudioManager.CustomAudios.Add(name);
            FileName = Name = name;
            Author = "";
        }

        UnOfficial = music == SupportedMusics.UnOfficial;
        CurrectAudio = music;
        Path = GetResourceFilesPath(FileType.Musics, FileName + ".wav");
        CurrectAudioStates = LastAudioStates =
            AudioManager.ConvertExtension(ref Path) ? AudiosStates.Exist : AudiosStates.NotExist;

        lock (finalMusicsLock)
        {
            var file = musics.Find(x => x.FileName == FileName);
            if (file != null)
            {
                file.Path = Path;
                if (file.CurrectAudioStates is AudiosStates.DownLoadFailureNotice or AudiosStates.DownLoadSucceedNotice
                    || CurrectAudioStates is AudiosStates.NotExist)
                    file.CurrectAudioStates = file.LastAudioStates = CurrectAudioStates;
            }
            else if (Name != string.Empty)
            {
                musics.Add(this);
            }
        }
    }
}