using System;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace FinalSuspect.ClientActions.FeatureItems.MyMusic;

public static class AudioPlayer
{
    public static int? TempTimeSample;
    public static float? TempTime;

    private static FinalMusic _currentMusic;

    public static FinalMusic CurrentMusic
    {
        get
        {
            return _currentMusic ?? FinalMusic.Musics?.FirstOrDefault(x =>
                x.CurrentAudioStates is AudiosStates.Playing or AudiosStates.Pausing);
        }
        private set => _currentMusic = value;
    }

    public static async void Play(FinalMusic audio, bool asMainMenuMusic = false)
    {
        try
        {
            if (audio.CurrentAudioStates is AudiosStates.NotExist or AudiosStates.Playing) return;
            if (!Constants.ShouldPlaySfx()) return;
            if (FinalMusic.Musics.Any(x => x.CurrentAudioStates is AudiosStates.Parsing)) return;
            var isPlaying = FinalMusic.Musics.Any(x => x.CurrentAudioStates is AudiosStates.Playing);

            if (isPlaying && asMainMenuMusic) return;

            _ = new MainThreadTask(() => { StopPlayMod(true); }, "Playing Sfx-Stop Play Other Sfx");

            await audio.Load();

            _ = new MainThreadTask(() =>
            {
                foreach (var file in FinalMusic.Musics.Where(file => file.FileName == audio.FileName))
                {
                    file.CurrentAudioStates = AudiosStates.Playing;
                    file.PlayAsMainMenuMusic = asMainMenuMusic;
                }

                MyMusicPanel.RefreshTagList();
                if (SceneManager.GetActiveScene().name is "SplashIntro")
                {
                    ModManager.Instance.StartCoroutine(CreateTemporaryAudioSources(audio.FileName, audio.Clip));
                }
                else
                {
                    SoundManager.Instance.CrossFadeSound(audio.FileName, audio.Clip, 0.7f);
                }

                CurrentMusic = audio;
                Msg($"播放声音：{audio.Name}", "CustomSounds");
            }, "Playing Sfx-Start Play");
        }
        catch
        {
            /* ignored */
        }
    }

    public static void StopPlayMod(bool playNew = false)
    {
        if (SceneManager.GetActiveScene().name is "SplashIntro") return;
        FinalMusic.Musics.Do(x =>
        {
            Object.Destroy(x.Clip);
            x.Clip = null;
            x.CurrentAudioStates = x.LastAudioStates;
            x.PlayAsMainMenuMusic = false;
            SoundManager.Instance.StopNamedSound(x.FileName);
            GC.Collect();
        });
        CurrentMusic = null;
        _ = new MainThreadTask(MyMusicPanel.RefreshTagList, "Refresh Tag List");
        if (Main.DisableVanillaSound.Value || playNew)
            StopPlayVanilla();
        else
            StartPlayVanilla();
    }

    public static void StopPlayVanilla()
    {
        if (SceneManager.GetActiveScene().name is "SplashIntro") return;
        SoundManager.Instance.StopNamedSound("MapTheme");
        SoundManager.Instance.StopNamedSound("MainBG");
    }

    public static void StartPlayVanilla()
    {
        if (SceneManager.GetActiveScene().name is "SplashIntro") return;
        var isPlaying = FinalMusic.Musics.Any(x => x.CurrentAudioStates == AudiosStates.Playing);
        if (isPlaying) return;
        if (IsLobby)
            SoundManager.Instance.CrossFadeSound("MapTheme", LobbyBehaviour.Instance.MapTheme, 0.07f);
    }

    private static IEnumerator CreateTemporaryAudioSources(string fileName, AudioClip clip)
    {
        var go = new GameObject("TempAudio");
        Object.DontDestroyOnLoad(go);
        var audioSource = go.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = null;


        audioSource.playOnAwake = false;
        audioSource.volume = 0.07f;
        audioSource.loop = true;
        audioSource.clip = clip;

        audioSource.Play();
        while (SceneManager.GetActiveScene().name is "SplashIntro")
        {
            yield return null;
        }

        var timeSamples = audioSource.timeSamples;
        var time = audioSource.time;
        TempTimeSample = timeSamples;
        TempTime = time;
        SoundManager.Instance.CrossFadeSound(fileName, clip, 0.7f);
        audioSource.Stop();
        Object.Destroy(go);
    }

    public static void PlayRandomTrack()
    {
        var randomIndex = HashRandom.Next(0, FinalMusic.Musics.Count - 1);
        StopPlayMod();
        Play(FinalMusic.Musics[randomIndex]);
    }

    public static void PlayByDirection(bool next)
    {
        if (next) PlayNextTrack();
        else PlayLastTrack();
    }

    private static void PlayNextTrack()
    {
        if (CurrentMusic == null) return;
        var currentIndex = FinalMusic.Musics.IndexOf(CurrentMusic);
        var nextIndex = (currentIndex + 1) % FinalMusic.Musics.Count;
        StopPlayMod();
        Play(FinalMusic.Musics[nextIndex]);
    }

    private static void PlayLastTrack()
    {
        if (CurrentMusic == null || FinalMusic.Musics == null || FinalMusic.Musics.Count == 0) return;
        var currentIndex = FinalMusic.Musics.IndexOf(CurrentMusic);
        var lastTrackIndex = (currentIndex - 1 + FinalMusic.Musics.Count) % FinalMusic.Musics.Count;
        StopPlayMod();
        Play(FinalMusic.Musics[lastTrackIndex]);
    }
}

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySoundImmediate))]
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySound))]
public class PlaySoundPatch
{
    public static bool Prefix(SoundManager __instance, [HarmonyArgument(0)] AudioClip clip,
        [HarmonyArgument(1)] bool loop)
    {
        var isPlaying = FinalMusic.Musics.Any(x => x.CurrentAudioStates == AudiosStates.Playing);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(isPlaying || disableVanilla) || !loop;
    }
}

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayDynamicSound))]
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayNamedSound))]
public class PlayDynamicAndNamedSoundPatch
{
    public static bool Prefix([HarmonyArgument(0)] string name,
        [HarmonyArgument(2)] bool loop)
    {
        var isPlaying = FinalMusic.Musics.Any(x => x.CurrentAudioStates == AudiosStates.Playing);
        var isModMusic = FinalMusic.Musics.Any(x => x.FileName == name);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(isPlaying || disableVanilla) || !loop || isModMusic;
    }
}

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.CrossFadeSound))]
public class CrossFadeSoundPatch
{
    public static bool Prefix([HarmonyArgument(0)] string name)
    {
        if (name is "MainBG") return false;
        var isPlaying = FinalMusic.Musics.Any(x => x.CurrentAudioStates == AudiosStates.Playing);
        var isModMusic = FinalMusic.Musics.Any(x => x.FileName == name);
        var disableVanilla = Main.DisableVanillaSound.Value;


        return !(isPlaying || disableVanilla) || isModMusic;
    }

    public static void Postfix(SoundManager __instance, [HarmonyArgument(0)] string name)
    {
        if (AudioPlayer.TempTimeSample == null || AudioPlayer.TempTime == null) return;
        var audio = __instance.soundPlayers.ToArray().ToList().First(x => x.Name == name).Player;
        audio.Pause();
        audio.time = AudioPlayer.TempTime.Value;
        audio.timeSamples = AudioPlayer.TempTimeSample.Value;
        audio.UnPause();
        AudioPlayer.TempTimeSample = null;
        AudioPlayer.TempTime = null;
    }
}

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.StopAllSound))]
public class StopAllSoundPatch
{
    public static bool Prefix(SoundManager __instance)
    {
        try
        {
            for (var i = __instance.soundPlayers.Count - 1; i >= 0; i--)
            {
                var matchingMusic =
                    FinalMusic.Musics.FirstOrDefault(x => x.Clip == __instance.soundPlayers[i].Player.clip);
                if (matchingMusic != null)
                {
                    if (!matchingMusic.PlayAsMainMenuMusic) continue;
                    AudioPlayer.StopPlayMod();
                }

                Object.Destroy(__instance.soundPlayers[i].Player);
                __instance.soundPlayers.RemoveAt(i);
            }

            var keysToRemove = new List<AudioClip>();
            foreach (var (key, value) in __instance.allSources)
            {
                if (FinalMusic.Musics.Any(x => x.Clip == key && !x.PlayAsMainMenuMusic))
                    continue;

                value.volume = 0f;
                value.Stop();
                Object.Destroy(value);
                keysToRemove.Add(key);
            }

            foreach (var key in keysToRemove) __instance.allSources.Remove(key);

            return false;
        }
        catch
        {
            return true;
        }
    }
}