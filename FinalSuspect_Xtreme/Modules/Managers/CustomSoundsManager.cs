using AmongUs.HTTP;
using Hazel;
using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using System.Runtime.InteropServices;
using static FinalSuspect_Xtreme.Modules.Managers.AudioManager;
using static FinalSuspect_Xtreme.Modules.Managers.FinalMusic;
using static FinalSuspect_Xtreme.Translator;
using System.Linq;
using UnityEngine;
using FinalSuspect_Xtreme.Modules.SoundInterface;
using TMPro;
using InnerNet;


namespace FinalSuspect_Xtreme.Modules.Managers;

public static class CustomSoundsManager
{

    public static void Play(FinalMusic audio)
    {

        if (audio.CurrectAudioStates is AudiosStates.NotExist or AudiosStates.IsPlaying) return;
        if (!Constants.ShouldPlaySfx()) return;
        if (!Directory.Exists(SOUNDS_PATH))
        {
            Directory.CreateDirectory(SOUNDS_PATH);
            return;
        }

        foreach (var file in finalMusics)
        {
            if (file.FileName != audio.FileName)
                file.CurrectAudioStates = file.LastAudioStates;
            else
                file.CurrectAudioStates = file.LastAudioStates = AudiosStates.IsPlaying;
        }
        
        StopPlay();
        ReloadTag(null);
        MyMusicPanel.RefreshTagList();
        AudioManagementPanel.RefreshTagList();
        StartPlayLoop(audio.Path);
        Logger.Msg($"播放声音：{audio.Name}", "CustomSounds");
    }

    [DllImport("winmm.dll")]
    public static extern bool PlaySound(string Filename, int Mod, int Flags);
    public static void StartPlayLoop(string path) => PlaySound(@$"{path}", 0, 9);
    public static void StopPlay()
    {
        PlaySound(null, 0, 0);
        finalMusics.Do(x => x.CurrectAudioStates = x.LastAudioStates);
        SoundManager.Instance.StopAllSound();
        new LateTask(() =>
        {
            MyMusicPanel.RefreshTagList();
            AudioManagementPanel.RefreshTagList();
        }, 0.01f);
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
    //    foreach (var audio in FinalMusic.finalMusics)
    //    {
    //        var music = audio.FileName;
    //        mus.Add(music);
    //    }
    //    if (MyMusicPanel.PlayMode == 2)
    //    {
    //        for (int i = 0; i < 10; i++)
    //        {
    //            var select = mus[rd.Next(0, mus.Count)];
    //            var path = @$"FinalSuspect_Data/Resources/Audios/{select}.wav";
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
    //            var path = @$"FinalSuspect_Data/Resources/Audios/{select}.wav";
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

    //public static void StartPlayInAmongUs(FinalMusic audio)
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
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySoundImmediate))]
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySound))]
public class AudioManagementPlaySoundPatch
{
    public static bool Prefix(SoundManager __instance, [HarmonyArgument(0)] AudioClip clip, [HarmonyArgument(1)] bool loop)
    {
        var ip = finalMusics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(ip || disableVanilla) || !loop;
    }

}
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayDynamicSound))]
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayNamedSound))]
public class AudioManagementPlayDynamicandNamedSoundPatch
{
    public static bool Prefix([HarmonyArgument(1)] AudioClip clip, [HarmonyArgument(2)] bool loop)
    {
        var ip = finalMusics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(ip || disableVanilla) || !loop;
    }
}

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.CrossFadeSound))]
public class AudioManagementCrossFadeSoundPatch
{
    public static bool Prefix([HarmonyArgument(0)] string name, [HarmonyArgument(1)] AudioClip clip)
    {
        var ip = finalMusics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(ip || disableVanilla);
    }
}
