using AmongUs.HTTP;
using Hazel;
using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Media;
using static FinalSuspect_Xtreme.Modules.Managers.AudioManager;
using System.Linq;
using static Il2CppSystem.Xml.XmlWellFormedWriter.AttributeValueCache;
//using Il2CppSystem.IO;
using UnityEngine;
using Steamworks;
using UnityEngine.Audio;
using FinalSuspect_Xtreme.Modules.SoundInterface;


namespace FinalSuspect_Xtreme.Modules.Managers;

public static class CustomSoundsManager
{
    public static readonly string SOUNDS_PATH = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}./FinalSuspect_Data/Sounds/";

    public static void Play(string sound, int playmode = 0)
    {
        if (!Constants.ShouldPlaySfx()) return;
        var path = SOUNDS_PATH + sound + ".wav";

        if (!Directory.Exists(SOUNDS_PATH)) Directory.CreateDirectory(SOUNDS_PATH);
        DirectoryInfo folder = new(SOUNDS_PATH);
        if ((folder.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            folder.Attributes = FileAttributes.Normal;

        if (!ConvertExtension(ref path)) return;
        switch (playmode)
        {
            case 0:
                StartPlayOnce(path);
                break;
            case 1:
                StartPlayInAmongUs(path,sound);
                break;
            case 2:
            case 3:
                AutoPlay(path, sound);
                break;

        }

        Logger.Msg($"播放声音：{sound}", "CustomSounds");
    }

    [DllImport("winmm.dll")]
    public static extern bool PlaySound(string Filename, int Mod, int Flags);
    public static void AutoPlay(string sound, string soundname)
    {
        Play(sound);
        MusicNow = soundname;
        MusicPlaybackCompletedHandler();
    }

    public static string MusicNow = "";
    // 播放音乐

    // 播放完成事件处理程序
    private static void MusicPlaybackCompletedHandler()
    {
        var rd = IRandom.Instance;
        List<string> mus = new();
        foreach (var music in AllMusics.Keys)
        {

            mus.Add(music);


        }
        if (MyMusicPanel.PlayMode == 2)
        {
            for (int i = 0; i < 10; i++)
            {
                var select = mus[rd.Next(0, mus.Count)];
                var path = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}./FinalSuspect_Data/Sounds/{select}.wav";
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
                var path = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}./FinalSuspect_Data/Sounds/{select}.wav";
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
    public static void StartPlayOnce(string path) => PlaySound(@$"{path}", 0, 1); //第3个形参，换为9，连续播放
    public static void StartPlayLoop(string path) => PlaySound(@$"{path}", 0, 9);
    public static void StartPlayWait(string path) => PlaySound(@$"{path}", 0, 17);
    public static bool isinternal = false;
    public static bool isModSound = false;
    public static void StopPlay()
    {
        isinternal = true;
        PlaySound(null, 0, 0);

        SoundManager.Instance.StopAllSound();
        isinternal = false;
        new LateTask(() =>
        {
            MyMusicPanel.RefreshTagList();
        }, 0.01f);

    }
    public static void StartPlayInAmongUs(string path = "", string mus = "")
    {

        AllSoundClips.TryGetValue(mus, out var audioClip);
        if (audioClip != null)
        {
            StopPlay();
            isModSound = true;
            SoundManager.Instance.PlaySound(audioClip, true, 1, null);
            isModSound = false;

        }
        else
        {
            var task = LoadAudioClipAsync(path, mus);
            task.ContinueWith(t =>
            {
                if (t.Result != null)
                {
                    audioClip = t.Result;
                    AllSoundClips.Remove(mus);
                    AllSoundClips.TryAdd(mus, audioClip);
                    StopPlay();
                    isModSound = true;
                    SoundManager.Instance.PlaySound(audioClip, true, 1, null);
                    isModSound = false;

                }
                else
                    Logger.Warn($"Failed to load AudioClip from path: {path}", "LoadAudioClip");
            });


        }
    }
}
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySoundImmediate))]
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySound))]
public class AudioManagementPlaySoundPatch
{
    public static bool Prefix(SoundManager __instance, [HarmonyArgument(0)] AudioClip clip, [HarmonyArgument(1)] bool loop)
    {
        var ip = AllSoundClips?.Values?.Any(SoundManager.Instance.SoundIsPlaying) ?? false;
        if (SoundManager.Instance != null && SoundManager.Instance.allSources != null)
        {
            foreach (var aso in SoundManager.Instance.allSources)
            {
                if (aso.Key != null && AllSoundClips.Values.Any(ac => ac != null && ac.name == aso.Key.name))
                {
                    ip = true;
                    break;
                }
            }
        }
        var disableVanilla = Main.DisableVanillaSound.Value;
        var isVanillaSound = !CustomSoundsManager.isModSound;
        if ((ip || disableVanilla) && isVanillaSound && loop) 
            return false;
        return true;
    }
    
}
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayDynamicSound))]
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayNamedSound))]
public class AudioManagementPlayDynamicandNamedSoundPatch
{
    public static bool Prefix([HarmonyArgument(1)] AudioClip clip, [HarmonyArgument(2)] bool loop)
    {
        var ip = AllSoundClips?.Values?.Any(SoundManager.Instance.SoundIsPlaying) ?? false;
        if (SoundManager.Instance != null && SoundManager.Instance.allSources != null)
        {
            foreach (var aso in SoundManager.Instance.allSources)
            {
                if (aso.Key != null && AllSoundClips.Values.Any(ac => ac != null && ac.name == aso.Key.name))
                {
                    ip = true;
                    break;
                }
            }
        }
        var disableVanilla = Main.DisableVanillaSound.Value;
        var isVanillaSound = !CustomSoundsManager.isModSound;
        if ((ip || disableVanilla) && isVanillaSound && loop)
            return false;
        return true;
    }
}

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.StopAllSound))]
public class AudioManagementStopPatch
{
    public static bool Prefix()
    {
        var ip = AllSoundClips?.Values?.Any(SoundManager.Instance.SoundIsPlaying) ?? false;
        if (SoundManager.Instance != null && SoundManager.Instance.allSources != null)
        {
            foreach (var aso in SoundManager.Instance.allSources)
            {
                if (aso.Key != null && AllSoundClips.Values.Any(ac => ac != null && ac.name == aso.Key.name))
                {
                    ip = true;
                    break;
                }
            }
        }
        var disableVanilla = Main.DisableVanillaSound.Value;

        if ((ip || disableVanilla) && !CustomSoundsManager.isinternal)
            return false;
        return true;
    }
}
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.CrossFadeSound))]
public class AudioManagementCrossFadeSoundPatch
{
    public static bool Prefix([HarmonyArgument(1)] AudioClip clip)
    {
        var ip = AllSoundClips?.Values?.Any(SoundManager.Instance.SoundIsPlaying) ?? false;
        if (SoundManager.Instance != null && SoundManager.Instance.allSources != null)
        {
            foreach (var aso in SoundManager.Instance.allSources)
            {
                if (aso.Key != null && AllSoundClips.Values.Any(ac => ac != null && ac.name == aso.Key.name))
                {
                    ip = true;
                    break;
                }
            }
        }
        var disableVanilla = Main.DisableVanillaSound.Value;
        var isVanillaSound = !CustomSoundsManager.isModSound;
        if ((ip || disableVanilla) && isVanillaSound)
            return false;
        return true;
    }
}
