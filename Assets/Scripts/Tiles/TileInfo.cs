using UnityEngine;

public enum TileType
{
    Snow,
    Ice,
    Ground,
    Grace,
    Stone,
    Bushes,
    Water,
}

public class TileInfo : MonoBehaviour
{
    [SerializeField] private TileType _type;

    [Header("MOVEMENT")] [SerializeField] private float _speedMultiplier = 1f;
    [SerializeField] private float _jumpMultiplier = 1f;
    [SerializeField] private float _buoyancyAcceleration = 0f;

    [Header("SOUND")] [SerializeField] private AudioClip[] _stepSound;
    [SerializeField] private AudioClip[] _landSound;
    [SerializeField] private AudioClip[] _jumpSound;

    public TileType Type => _type;

    public float SpeedMultiplier => _speedMultiplier;

    public float JumpMultiplier => _jumpMultiplier;

    public float BuoyancyAcceleration => _buoyancyAcceleration;

    public AudioClip[] StepSound => _stepSound;

    public AudioClip[] LandSound => _landSound;

    public AudioClip[] JumpSound => _jumpSound;

    public static void PlaySound(AudioSource source, AudioClip[] sounds)
    {
        source.PlayOneShot(sounds[Random.Range(0, sounds.Length - 1)]);
    }
}