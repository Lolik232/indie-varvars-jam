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

    [Header("GRAVITY")] [SerializeField]
    private float buoyancyAcceleration;

    [Header("SOUND")] [SerializeField] private AudioClip[] _stepSound;
    [SerializeField] private AudioClip[] _landSound;
    [SerializeField] private AudioClip[] _jumpSound;



    public float SpeedMultiplier => _speedMultiplier;

    public float AccelerationMultiplier => accelerationMultiplier;

    public float DecelerationMultiplier => _decelerationMultiplier;

    public float JumpMultiplier => _jumpMultiplier;

    public float BuoyancyAcceleration => buoyancyAcceleration;

    public AudioClip GetStepSound()
    {
        return _stepSound.Length == 0 ? null : _stepSound[Random.Range(0, _stepSound.Length - 1)];
    }

    public AudioClip GetJumpSound()
    {
        return _jumpSound.Length == 0 ? null : _jumpSound[Random.Range(0, _jumpSound.Length - 1)];
    }
    
    public AudioClip GetLandSound()
    {
        return _landSound.Length == 0 ? null : _landSound[Random.Range(0, _landSound.Length - 1)];
    }
}