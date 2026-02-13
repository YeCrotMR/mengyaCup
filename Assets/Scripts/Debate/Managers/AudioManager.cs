using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    #region 变量定义

    [SerializeField] AudioSource BGMPlayer;
    [SerializeField] AudioSource SFXPlayer;
    [SerializeField] AudioSource VoicePlayer; 

    [Range(0f, 1f)] public float mainBGMVolume = 1f; 
    [Range(0f, 1f)] public float mainSFXVolume = 1f; 
    [Range(0f, 1f)] public float mainVoiceVolume = 1f; 

    public AudioData[] BGMList; 

    const float MIN_PITCH = 0.9f; 
    const float MAX_PITCH = 1.1f;  

    #endregion

    #region 背景音乐 (BGM) 方法

    /// <summary>
    /// 播放背景音乐（如果已经在播放则不重播）
    /// </summary>
    /// <param name="i">BGM索引</param>
    public void PlayBGM(int i)
    {
        BGMPlayer.clip = BGMList[i].audioClip;
        BGMPlayer.volume = BGMList[i].volume * mainBGMVolume;
        BGMPlayer.Play();
    }

    /// <summary>
    /// 停止BGM
    /// </summary>
    public void StopBGM()
    {
        BGMPlayer.Stop();
    }

    /// <summary>
    /// 切换BGM
    /// </summary>
    /// <param name="i">目标BGM索引</param>
    public void SwitchBGM(int i)
    {
        if (BGMPlayer.clip != BGMList[i].audioClip)
        {
            if (BGMPlayer.isPlaying)
            {
                BGMPlayer.Stop();
            }
            BGMPlayer.clip = BGMList[i].audioClip;
            BGMPlayer.volume = BGMList[i].volume * mainBGMVolume;
            BGMPlayer.Play();
        }
    }

    /// <summary>
    /// 切换指定BGM对象
    /// </summary>
    /// <param name="audio">目标BGM数据</param>
    public void SwitchBGM(AudioData audio)
    {
        if (BGMPlayer.isPlaying)
        {
            BGMPlayer.Stop();
        }

        BGMPlayer.clip = audio.audioClip;
        BGMPlayer.volume = audio.volume * mainBGMVolume;
        BGMPlayer.Play();
    }

    /// <summary>
    /// 设置BGM音量
    /// </summary>
    /// <param name="volume">音量大小</param>
    public void SettingBGMVolume(float volume)
    {
        if (BGMPlayer.volume > 0)
            BGMPlayer.volume = BGMPlayer.volume / mainBGMVolume * volume;
        else
            BGMPlayer.volume = volume;
            
        mainBGMVolume = volume;
    }

    #endregion

    #region 音效 (SFX) 方法

    /// <summary>
    /// 播放一次音效
    /// </summary>
    /// <param name="audio">目标音效</param>
    public void PlaySFXOnce(AudioData audio)
    {
        if (!SFXPlayer.isPlaying)
        {
            SFXPlayer.pitch = 1f;
            SFXPlayer.PlayOneShot(audio.audioClip, audio.volume * mainSFXVolume);
        }
    }

    /// <summary>
    /// 播放一次音效（随机音调）
    /// </summary>
    /// <param name="audio">目标音效</param>
    public void PlayRandomSFXOnce(AudioData audio)
    {
        if (!SFXPlayer.isPlaying)
        {
            SFXPlayer.pitch = Random.Range(MIN_PITCH, MAX_PITCH);
            SFXPlayer.PlayOneShot(audio.audioClip, audio.volume * mainSFXVolume);
        }
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="audio">目标音效</param>
    public void PlaySFX(AudioData audio)
    {
        SFXPlayer.pitch = 1f;
        SFXPlayer.PlayOneShot(audio.audioClip, audio.volume * mainSFXVolume);
    }

    /// <summary>
    /// 播放音效（随机音调）
    /// </summary>
    /// <param name="audio">目标音效</param>
    public void PlayRandomSFX(AudioData audio)
    {
        SFXPlayer.pitch = Random.Range(MIN_PITCH, MAX_PITCH);
        SFXPlayer.PlayOneShot(audio.audioClip, audio.volume * mainSFXVolume);
    }

    /// <summary>
    /// 设置音效音量
    /// </summary>
    /// <param name="volume">音量大小</param>
    public void SettingSFXVolume(float volume)
    {
        mainSFXVolume = volume;
        SFXPlayer.volume = volume;
    }

    /// <summary>
    /// 切换音效(停止当前音效并切换为指定音效)
    /// </summary>
    /// <param name="audio">目标音效</param>
    public void SwitchSFX(AudioData audio)
    {
        if (SFXPlayer.isPlaying)
        {
            SFXPlayer.Stop();
        }
        SFXPlayer.pitch = 1f;
        SFXPlayer.PlayOneShot(audio.audioClip, audio.volume * mainSFXVolume);
    }

    /// <summary>
    /// 停止播放音效
    /// </summary>
    public void StopSFX()
    {
        SFXPlayer.Stop();
    }

    #endregion

    #region 语音 (Voice) 方法

    /// <summary>
    /// 播放语音（直接通过Resources路径加载）
    /// </summary>
    /// <param name="path">Resources文件夹下的完整路径 (不含扩展名)</param>
    public void PlayVoice(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        string fullPath = path;

        AudioClip clip = Resources.Load<AudioClip>(fullPath);
        if (clip != null)
        {
            VoicePlayer.clip = clip;
            VoicePlayer.volume = mainVoiceVolume;
            VoicePlayer.Play();
        }
        else
        {
             Debug.LogWarning($"[Audio] Voice not found: {fullPath}");
        }
    }

    /// <summary>
    /// 停止播放语音
    /// </summary>
    public void StopVoice()
    {
        if (VoicePlayer.isPlaying)
        {
            VoicePlayer.Stop();
        }
    }

    /// <summary>
    /// 检查当前语音是否正在播放
    /// </summary>
    public bool IsVoicePlaying()
    {
        return VoicePlayer != null && VoicePlayer.isPlaying;
    }

    /// <summary>
    /// 设置语音音量
    /// </summary>
    public void SettingVoiceVolume(float volume)
    {
        mainVoiceVolume = volume;
        VoicePlayer.volume = volume;
    }

    #endregion
}
