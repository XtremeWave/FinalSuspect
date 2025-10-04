namespace FinalSuspect.Helpers;

public static class AudioSourceHelper
{
    /// <summary>
    /// 获取当前播放位置（秒）
    /// </summary>
    public static float GetPlaybackTime(this AudioSource source)
    {
        return source.time;
    }

    /// <summary>
    /// 获取播放进度百分比
    /// </summary>
    public static float GetPlaybackProgress(this AudioSource source)
    {
        return source.clip != null ? Mathf.Clamp01(source.time / source.clip.length) : 0;
    }

    /// <summary>
    /// 获取剩余时间（秒）
    /// </summary>
    public static float GetRemainingTime(this AudioSource source)
    {
        return source.clip != null ? (source.clip.length - source.time) : 0;
    }

    /// <summary>
    /// 检查是否正在播放
    /// </summary>
    public static bool IsPlaying(this AudioSource source)
    {
        return source.isPlaying;
    }
}