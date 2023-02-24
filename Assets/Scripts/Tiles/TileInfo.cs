using UnityEngine;

public enum TileType
{
    Snow, 
    Ice, 
    Ground, 
    Grace,
    Stone,
    Water,
    Bushes
}

public class TileInfo : MonoBehaviour
{
    [SerializeField] private TileType _type;
    
    [Header("MOVEMENT")]
    [SerializeField] private float _speedMultiplier = 1f;
    [SerializeField] private float _jumpMultiplier = 1f;
    [SerializeField] private float _buoyancyAcceleration = 0f;
    
    [Header("SOUND")]
    [SerializeField] private AudioSource _stepSound;
    [SerializeField] private AudioSource _landSound;
    [SerializeField] private AudioSource _jumpSound;

    public TileType Type => _type;

    public float SpeedMultiplier => _speedMultiplier;

    public float JumpMultiplier => _jumpMultiplier;

    public float BuoyancyAcceleration => _buoyancyAcceleration;

    public AudioSource StepSound => _stepSound;

    public AudioSource LandSound => _landSound;

    public AudioSource JumpSound => _jumpSound;
}
