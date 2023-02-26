using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    [SerializeField] private Slider gameSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider fxSlider;
    [SerializeField] private AudioMixerGroup mixer;

    private void Start()
    {
        gameSlider.value = PlayerPrefs.GetFloat("GameVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        fxSlider.value = PlayerPrefs.GetFloat("FXVolume", 1f);
    }

    private void ChangeVolume(string name, float volume)
    {
        mixer.audioMixer.SetFloat(name, SqrtInterpolation(-80, 0, volume));
        PlayerPrefs.SetFloat(name, volume);
    }

    private static float SqrtInterpolation(float a, float b, float t)
    {
        return a + (b - a) * (float)Math.Pow(t, 1.0 / 6.0);
    }

    public void OnGameSlider(float volume)
    {
        ChangeVolume("GameVolume", volume);
    }

    public void OnMusicSlider(float volume)
    {
        ChangeVolume("MusicVolume", volume);
    }

    public void OnFXSlider(float volume)
    {
        ChangeVolume("FXVolume", volume);
    }
}