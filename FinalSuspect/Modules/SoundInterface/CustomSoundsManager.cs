﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using static FinalSuspect.Modules.SoundInterface.SoundManager;
using static FinalSuspect.Modules.SoundInterface.XtremeMusic;
using Object = UnityEngine.Object;


namespace FinalSuspect.Modules.SoundInterface;

public static class CustomSoundsManager
{
    public static async void Play(XtremeMusic audio)
    {
        try
        {
            if (audio.CurrectAudioStates is AudiosStates.NotExist or AudiosStates.IsPlaying) return;
            if (!Constants.ShouldPlaySfx()) return;
        
            _ = new LateTask(() =>
            {
                StopPlayMod();
                StopPlayVanilla();
            }, 0.01F, "");
        
            await LoadClip(audio.CurrectAudio);
        
            _ = new LateTask(() =>
            {
                foreach (var file in musics.Where(file => file.FileName == audio.FileName))
                {
                    file.CurrectAudioStates = AudiosStates.IsPlaying;
                }
        
                ReloadTag();
                MyMusicPanel.RefreshTagList();
                SoundManagementPanel.RefreshTagList();
                global::SoundManager.Instance.CrossFadeSound(audio.FileName, audio.Clip, 1f);
                XtremeLogger.Msg($"播放声音：{audio.Name}", "CustomSounds");
            }, 0.01F, "");
        }
        catch 
        {
            //
        }
    }
 
    public static void StopPlayMod()
    {
        musics.Do(x =>
        {
            x.Clip = null;
            x.CurrectAudioStates = x.LastAudioStates;
            global::SoundManager.Instance.StopNamedSound(x.FileName);
        });

        _ = new LateTask(() =>
        {
            MyMusicPanel.RefreshTagList();
            SoundManagementPanel.RefreshTagList();
        }, 0.01f, "Refresh");
        if (Main.DisableVanillaSound.Value)
        {
            StopPlayVanilla();
        }
        else
        {
            StartPlayVanilla();
        }
    }

    public static void StopPlayVanilla()
    {
        global::SoundManager.Instance.StopNamedSound("MapTheme");
        global::SoundManager.Instance.StopNamedSound("MainBG");
    }

    public static void StartPlayVanilla()
    {
        var isPlaying = musics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        if (isPlaying) return;
        if (XtremeGameData.GameStates.IsLobby)
            global::SoundManager.Instance.CrossFadeSound("MapTheme", LobbyBehaviour.Instance.MapTheme, 0.07f);
        else if (XtremeGameData.GameStates.IsNotJoined)
            global::SoundManager.Instance.CrossFadeSound("MainBG", DestroyableSingleton<JoinGameButton>.Instance.IntroMusic, 1f);
    }
    /*
    //public static void AutoPlay(string sound, string name)
    //{
    //    Play(sound);
    //    MusicNow = name;
    //    MusicPlaybackCompletedHandler();
    //}

    //public static string MusicNow = "";
    //private static void MusicPlaybackCompletedHandler()
    //{
    //    var rd = IRandom.Instance;
    //    List<string> mus = new();
    //    foreach (var audio in XtremeMusic.musics)
    //    {
    //        var music = audio.FileName;
    //        mus.Add(music);
    //    }
    //    if (MyMusicPanel.PlayMode == 2)
    //    {
    //        for (int i = 0; i < 10; i++)
    //        {
    //            var select = mus[rd.Next(0, mus.Count)];
    //            var path = @$"Final Suspect_Data/Resources/Audios/{select}.wav";
    //            if (ConvertExtension(ref path))
    //                StartPlayWait(path);
    //            else
    //                i--;
    //        }

    //    }
    //    else if (MyMusicPanel.PlayMode == 3)
    //    {
    //        var musicn = mus.IndexOf(MusicNow);
    //        for (int i = 0; i < 10; i++)
    //        {
    //            int index = musicn;
    //            if (index > mus.Count - 2)
    //                index = -1;
    //            var select = mus[index + 1];
    //            var path = @$"Final Suspect_Data/Resources/Audios/{select}.wav";
    //            if (ConvertExtension(ref path))
    //            {
    //                StartPlayWait(path);
    //                musicn++;

    //            }
    //            else
    //                i--;
    //        }

    //    }
    //    new LateTask(() =>
    //    {
    //        MusicPlaybackCompletedHandler();
    //    }, 40f, "AddMusic");
    //}
    //public static void StartPlayOnce(string path) => PlaySound(@$"{path}", 0, 1); //第3个形参，换为9，连续播放

    //public static void StartPlayInAmongUs(XtremeMusic audio)
    //{
    //    if (audio.Clip != null)
    //    {
    //        StopPlay();
    //        SoundManager.Instance.CrossFadeSound(audio.Name, audio.Clip, 0.5f);
    //    }
    //    else
    //    {
    //        AudioManagementPanel.Delete(audio);
    //    }
    //}
    */
}
[HarmonyPatch(typeof(global::SoundManager), nameof(global::SoundManager.PlaySoundImmediate))]
[HarmonyPatch(typeof(global::SoundManager), nameof(global::SoundManager.PlaySound))]
public class AudioManagementPlaySoundPatch
{
    public static bool Prefix(global::SoundManager __instance, [HarmonyArgument(0)] AudioClip clip, [HarmonyArgument(1)] bool loop)
    {
        var isPlaying = musics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(isPlaying || disableVanilla) || !loop;
    }
}

[HarmonyPatch(typeof(global::SoundManager), nameof(global::SoundManager.PlayDynamicSound))]
[HarmonyPatch(typeof(global::SoundManager), nameof(global::SoundManager.PlayNamedSound))]
public class AudioManagementPlayDynamicandNamedSoundPatch
{
    public static bool Prefix([HarmonyArgument(0)] string name, [HarmonyArgument(1)] AudioClip clip, [HarmonyArgument(2)] bool loop)
    {
        var isPlaying = musics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var isModMusic = musics.Any(x => x.FileName == name);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(isPlaying || disableVanilla) || !loop || isModMusic;
    }
}

[HarmonyPatch(typeof(global::SoundManager), nameof(global::SoundManager.CrossFadeSound))]
public class AudioManagementCrossFadeSoundPatch
{
    public static bool Prefix([HarmonyArgument(0)] string name)
    {
        var isPlaying = musics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var isModMusic = musics.Any(x => x.FileName == name);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(isPlaying || disableVanilla) || isModMusic;
    }
}

[HarmonyPatch(typeof(global::SoundManager), nameof(global::SoundManager.StopAllSound))]
public class AudioManagementStopAllSoundPatch
{
    public static bool Prefix(global::SoundManager __instance)
    {
        for (var i = __instance.soundPlayers.Count - 1; i >= 0; i--)
        {
            if (musics.Any(x => x.Clip == __instance.soundPlayers[i].Player.clip)) 
                continue;
            
            Object.Destroy(__instance.soundPlayers[i].Player);
            __instance.soundPlayers.RemoveAt(i);
        }
        
        var keysToRemove = new List<AudioClip>();
        foreach (var (key, value) in __instance.allSources)
        {
            if (musics.Any(x => x.Clip == key)) 
            {
                continue; 
            }

            value.volume = 0f;
            value.Stop();
            Object.Destroy(value);
            keysToRemove.Add(key);
        }
        
        foreach (var key in keysToRemove)
        {
            __instance.allSources.Remove(key);
        }

        return false;
    }
}
