using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TileInfo : MonoBehaviour
{
    [Header("MOVEMENT")] [SerializeField] private float _speedMultiplier = 1f;
    [SerializeField, Range(0, 10f)] private float accelerationMultiplier;
    [SerializeField, Range(0, 10f)] private float _decelerationMultiplier;

    [Header("JUMP")] [SerializeField, Range(0.1f, 3f)]
    private float _jumpMultiplier = 1f;

    [Header("GRAVITY")] [SerializeField, Range(-120f, 120f)]
    private float buoyancySpeedAddon;

    [Header("SOUND")] [SerializeField] private AudioClip[] _stepSound;
    [SerializeField] private AudioClip[] _landSound;
    [SerializeField] private AudioClip[] _jumpSound;


    public float SpeedMultiplier => _speedMultiplier;

    public float AccelerationMultiplier => accelerationMultiplier;

    public float DecelerationMultiplier => _decelerationMultiplier;

    public float JumpMultiplier => _jumpMultiplier;

    public float BuoyancySpeedAddon => buoyancySpeedAddon;

    public void PlayStepSound(AudioSource source)
    {
        PlaySound(source, _stepSound);
    }

    public void PlayLandSound(AudioSource source)
    {
        PlaySound(source, _landSound);
    }

    public void PlayJumpSound(AudioSource source)
    {
        PlaySound(source, _jumpSound);
    }

    private static void PlaySound(AudioSource source, AudioClip[] sounds)
    {
        if (sounds.Length == 0) return;
        source.PlayOneShot(sounds[Random.Range(0, sounds.Length - 1)]);
    }
}