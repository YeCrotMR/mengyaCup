using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "声音数据", fileName = "audio_")]
public class AudioData : ScriptableObject
{
    public AudioClip audioClip; //音频文件
    [Range(0f, 1f)] public float volume = 1f; //音频音量大小
}