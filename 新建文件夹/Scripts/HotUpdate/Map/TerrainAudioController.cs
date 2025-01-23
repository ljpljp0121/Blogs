using JKFrame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainAudioController : MonoBehaviour
{
    private AudioSource[] audioSources;

    private void Start()
    {
        audioSources = GetComponentsInChildren<AudioSource>();
        EventSystem.AddTypeEventListener<MusicVolumeChangedEvent>(OnMusicVolumeChangedEvent);
    }

    private void OnDestroy()
    {
        EventSystem.RemoveTypeEventListener<MusicVolumeChangedEvent>(OnMusicVolumeChangedEvent);
    }

    private void OnMusicVolumeChangedEvent(MusicVolumeChangedEvent @event)
    {
        foreach (var source in audioSources)
        {
            source.volume = AudioSystem.BGVolume;
        }
    }
}
