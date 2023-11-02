using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public interface IAudioManager
{
    public void PlaySound(SoundType _sound, Vector3 _location);
}

public class AudioManager : MonoBehaviour, IAudioManager
{
    [SerializeField] private AudioClip[] _audioclips;
    public static AudioManager Main;
    void Awake()
    {
        Main = this;
    }
    public void PlaySound(SoundType _sound, Vector3 _location)
    {
        AudioSource.PlayClipAtPoint(_audioclips[(int)_sound], _location);
    }
}
