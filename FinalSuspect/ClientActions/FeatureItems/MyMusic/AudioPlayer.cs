using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalSuspect.ClientActions.FeatureItems.MyMusic;

public static class AudioPlayer
{
    public static async void Play(FinalMusic audio)
    {
        try
        {
            if (audio.CurrectAudioStates is AudiosStates.NotExist or AudiosStates.IsPlaying) return;
            if (!Constants.ShouldPlaySfx()) return;

            _ = new MainThreadTask(() => { StopPlayMod(true); }, "Playing Sfx");

            await FinalMusic.LoadClip(audio.CurrectAudio);

            _ = new MainThreadTask(() =>
            {
                foreach (var file in FinalMusic.musics.Where(file => file.FileName == audio.FileName))
                    file.CurrectAudioStates = AudiosStates.IsPlaying;

                AudioManager.ReloadTag();
                //MyMusicPanel.RefreshTagList();
                //SoundManagementPanel.RefreshTagList();
                SoundManager.Instance.CrossFadeSound(audio.FileName, audio.Clip, 0.7f);
                Msg($"播放声音：{audio.Name}", "CustomSounds");
            }, "Playing Sfx");
        }
        catch
        {
            /* ignored */
        }
    }

    public static void StopPlayMod(bool playNew = false)
    {
        FinalMusic.musics.Do(x =>
        {
            x.Clip = null;
            x.CurrectAudioStates = x.LastAudioStates;
            SoundManager.Instance.StopNamedSound(x.FileName);
        });
        _ = new MainThreadTask(() =>
        {
            //MyMusicPanel.RefreshTagList();
            //SoundManagementPanel.RefreshTagList();
        }, "Refresh Tag List");
        if (Main.DisableVanillaSound.Value || playNew)
            StopPlayVanilla();
        else
            StartPlayVanilla();
    }

    public static void StopPlayVanilla()
    {
        SoundManager.Instance.StopNamedSound("MapTheme");
        SoundManager.Instance.StopNamedSound("MainBG");
    }

    public static void StartPlayVanilla()
    {
        var isPlaying = FinalMusic.musics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        if (isPlaying) return;
        if (IsLobby)
            SoundManager.Instance.CrossFadeSound("MapTheme", LobbyBehaviour.Instance.MapTheme, 0.07f);
        else if (IsNotJoined)
            SoundManager.Instance.CrossFadeSound("MainBG", DestroyableSingleton<JoinGameButton>.Instance.IntroMusic,
                1f);
    }

    /*public static void AutoPlay(string sound, string name)
    {
        Play(sound);
        MusicNow = name;
        MusicPlaybackCompletedHandler();
    }

    public static string MusicNow = "";
    private static void MusicPlaybackCompletedHandler()
    {
        var rd = IRandom.Instance;
        List<string> mus = new();
        foreach (var audio in FinalMusic.musics)
        {
            var music = audio.FileName;
            mus.Add(music);
        }
        if (MyMusicPanel.PlayMode == 2)
        {
            for (int i = 0; i < 10; i++)
            {
                var select = mus[rd.Next(0, mus.Count)];
                var path = @$"Final Suspect_Data/Resources/Audios/{select}.wav";
                if (ConvertExtension(ref path))
                    StartPlayWait(path);
                else
                    i--;
            }

        }
        else if (MyMusicPanel.PlayMode == 3)
        {
            var musicn = mus.IndexOf(MusicNow);
            for (int i = 0; i < 10; i++)
            {
                int index = musicn;
                if (index > mus.Count - 2)
                    index = -1;
                var select = mus[index + 1];
                var path = @$"Final Suspect_Data/Resources/Audios/{select}.wav";
                if (ConvertExtension(ref path))
                {
                    StartPlayWait(path);
                    musicn++;

                }
                else
                    i--;
            }

        }
        new LateTask(() =>
        {
            MusicPlaybackCompletedHandler();
        }, 40f, "AddMusic");
    }
    public static void StartPlayOnce(string path) => PlaySound(@$"{path}", 0, 1); 第3个形参，换为9，连续播放

    public static void StartPlayInAmongUs(FinalMusic audio)
    {
        if (audio.Clip != null)
        {
            StopPlay();
            SoundManager.Instance.CrossFadeSound(audio.Name, audio.Clip, 0.5f);
        }
        else
        {
            Panel.Delete(audio);
        }
    }*/
}

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySoundImmediate))]
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySound))]
public class PlaySoundPatch
{
    public static bool Prefix(SoundManager __instance, [HarmonyArgument(0)] AudioClip clip,
        [HarmonyArgument(1)] bool loop)
    {
        var isPlaying = FinalMusic.musics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(isPlaying || disableVanilla) || !loop;
    }
}

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayDynamicSound))]
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayNamedSound))]
public class PlayDynamicAndNamedSoundPatch
{
    public static bool Prefix([HarmonyArgument(0)] string name, [HarmonyArgument(1)] AudioClip clip,
        [HarmonyArgument(2)] bool loop)
    {
        var isPlaying = FinalMusic.musics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var isModMusic = FinalMusic.musics.Any(x => x.FileName == name);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(isPlaying || disableVanilla) || !loop || isModMusic;
    }
}

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.CrossFadeSound))]
public class CrossFadeSoundPatch
{
    public static bool Prefix([HarmonyArgument(0)] string name, [HarmonyArgument(2)] float maxVolume)
    {
        var isPlaying = FinalMusic.musics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var isModMusic = FinalMusic.musics.Any(x => x.FileName == name);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(isPlaying || disableVanilla) || isModMusic;
    }
}

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.StopAllSound))]
public class StopAllSoundPatch
{
    public static bool Prefix(SoundManager __instance)
    {
        for (var i = __instance.soundPlayers.Count - 1; i >= 0; i--)
        {
            var matchingMusic = FinalMusic.musics.FirstOrDefault(x => x.Clip == __instance.soundPlayers[i].Player.clip);
            if (matchingMusic != null)
            {
                if (!matchingMusic.IsMainMenuMusic) continue;
                AudioPlayer.StopPlayMod();
            }

            Object.Destroy(__instance.soundPlayers[i].Player);
            __instance.soundPlayers.RemoveAt(i);
        }

        var keysToRemove = new List<AudioClip>();
        foreach (var (key, value) in __instance.allSources)
        {
            if (FinalMusic.musics.Any(x => x.Clip == key && !x.IsMainMenuMusic))
                continue;

            value.volume = 0f;
            value.Stop();
            Object.Destroy(value);
            keysToRemove.Add(key);
        }

        foreach (var key in keysToRemove) __instance.allSources.Remove(key);

        return false;
    }
}