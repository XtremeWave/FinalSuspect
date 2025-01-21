using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using static FinalSuspect.Modules.SoundInterface.SoundManager;
using static FinalSuspect.Modules.SoundInterface.FinalMusic;


namespace FinalSuspect.Modules.SoundInterface;

public static class CustomSoundsManager
{

    public static void Play(FinalMusic audio)
    {

        if (audio.CurrectAudioStates is AudiosStates.NotExist or AudiosStates.IsPlaying) return;
        if (!Constants.ShouldPlaySfx()) return;

        foreach (var file in finalMusics)
        {
            if (file.FileName != audio.FileName)
                file.CurrectAudioStates = file.LastAudioStates;
            else
                file.CurrectAudioStates = file.LastAudioStates = AudiosStates.IsPlaying;
        }
        
        StopPlayMod();
        StopPlayVanilla();
        ReloadTag();
        MyMusicPanel.RefreshTagList();
        SoundManagementPanel.RefreshTagList();
        StartPlayLoop(audio.Path);
        XtremeLogger.Msg($"播放声音：{audio.Name}", "CustomSounds");
    }

    [DllImport("winmm.dll")]
    public static extern bool PlaySound(string Filename, int Mod, int Flags);
    public static void StartPlayLoop(string path) => PlaySound(@$"{path}", 0, 9);
    public static void StopPlayMod()
    {
        PlaySound(null, 0, 0);
        finalMusics.Do(x => x.CurrectAudioStates = x.LastAudioStates);

        new LateTask(() =>
        {
            MyMusicPanel.RefreshTagList();
            SoundManagementPanel.RefreshTagList();
        }, 0.01f, "Refresh Tag List");
        if (Main.DisableVanillaSound.Value)
            StopPlayVanilla();
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
        var isPlaying = finalMusics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        if (isPlaying) return;
        if (XtremeGameData.GameStates.IsLobby)
            global::SoundManager.Instance.CrossFadeSound("MapTheme", LobbyBehaviour.Instance.MapTheme, 0.07f);
        else if (XtremeGameData.GameStates.IsNotJoined)
            global::SoundManager.Instance.CrossFadeSound("MainBG", DestroyableSingleton<JoinGameButton>.Instance.IntroMusic, 0.07f);
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
[HarmonyPatch(typeof(global::SoundManager), nameof(global::SoundManager.PlaySoundImmediate))]
[HarmonyPatch(typeof(global::SoundManager), nameof(global::SoundManager.PlaySound))]
public class AudioManagementPlaySoundPatch
{
    public static bool Prefix(global::SoundManager __instance, [HarmonyArgument(0)] AudioClip clip, [HarmonyArgument(1)] bool loop)
    {
        var isPlaying = finalMusics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(isPlaying || disableVanilla) || !loop;
    }

}
[HarmonyPatch(typeof(global::SoundManager), nameof(global::SoundManager.PlayDynamicSound))]
[HarmonyPatch(typeof(global::SoundManager), nameof(global::SoundManager.PlayNamedSound))]
public class AudioManagementPlayDynamicandNamedSoundPatch
{
    public static bool Prefix([HarmonyArgument(1)] AudioClip clip, [HarmonyArgument(2)] bool loop)
    {
        var isPlaying = finalMusics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(isPlaying || disableVanilla) || !loop;
    }
}

[HarmonyPatch(typeof(global::SoundManager), nameof(global::SoundManager.CrossFadeSound))]
public class AudioManagementCrossFadeSoundPatch
{
    public static bool Prefix([HarmonyArgument(0)] string name, [HarmonyArgument(1)] AudioClip clip)
    {
        var isPlaying = finalMusics.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(isPlaying || disableVanilla);
    }
}
