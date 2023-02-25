using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Activator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour, IPlayerController, IActivated
{
    private float _currentXSpeed, _currentYSpeed;

    #region Input

    public Vector3 Velocity { get; private set; }
    public Vector3 RawMovement { get; private set; }
    public FrameInput Input { get; private set; }
    public bool JumpingThisFrame { get; private set; }
    public bool LandingThisFrame { get; private set; }
    public bool Grounded => _collisionDown;

    private readonly Trigger JumpDownTrigger = new();
    private readonly Trigger JumpUpTrigger = new();
    private readonly Trigger DashDownTrigger = new();
    private readonly Trigger DashUpTrigger = new();
    private readonly Trigger UseItemDownTrigger = new();
    private readonly Trigger UseItemUpTrigger = new();

    private Vector2 _moveInput;

    public void OnMoveInput(InputAction.CallbackContext input)
    {
        _moveInput = input.ReadValue<Vector2>();
    }

    public void OnJumpInput(InputAction.CallbackContext input)
    {
        if (input.started)
        {
            JumpDownTrigger.Set();
        }
        else if (input.canceled)
        {
            JumpUpTrigger.Set();
        }
    }

    public void OnDashInput(InputAction.CallbackContext input)
    {
        if (input.started)
        {
            DashDownTrigger.Set();
        }
        else if (input.canceled)
        {
            DashUpTrigger.Set();
        }
    }

    public void OnUseItemInput(InputAction.CallbackContext input)
    {
        if (input.started)
        {
            UseItemDownTrigger.Set();
        }
        else if (input.canceled)
        {
            UseItemUpTrigger.Set();
        }
    }

    private void GatherInput()
    {
        Input = new FrameInput
        {
            X = _moveInput.x,
            Y = _moveInput.y,

            JumpDown = JumpDownTrigger,
            JumpUp = JumpUpTrigger,

            DashDown = DashDownTrigger,
            DashUp = DashUpTrigger
        };

        if (Input.JumpDown) _lastJumpPressed = Time.time;
    }

    #endregion

    #region Unity

    [SerializeField] private Collider2D _collider;
    [SerializeField] private Collider2D _groundChecker;
    [SerializeField] private Collider2D _ceilChecker;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private AudioSource _audioSource;
    private Transform _transform;


    private void Awake()
    {
        _transform = transform;
        _environmentFilter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = true,
            layerMask = _waterFilter.layerMask | _groundFilter.layerMask | _bushesFilter.layerMask
        };
    }

    private void Update()
    {
        if (!_activated) return;

        GatherInput();
    }

    private void FixedUpdate()
    {
        if (!_activated) return;

        Velocity = _rigidbody.velocity;

        CheckCollisions();
        CacheTileInfo();

        CalculateWalk();
        CalculateJumpApex();
        CalculateGravity();
        CalculateJump();


        MoveCharacter();
    }

    #endregion

    #region Collisions

    [Header("COLLISION")] [SerializeField] private ContactFilter2D _groundFilter;
    [SerializeField] private ContactFilter2D _ceilFilter;
    [SerializeField] private ContactFilter2D _waterFilter;
    [SerializeField] private ContactFilter2D _bushesFilter;

    private ContactFilter2D _environmentFilter;

    private bool _collisionDown, _collisionUp;

    private float _timeLeftGrounded;

    private void CheckCollisions()
    {
        LandingThisFrame = false;
        var groundedCheck = _groundChecker.IsTouchingLayers(_groundFilter.layerMask);

        if (_collisionDown & !groundedCheck) _timeLeftGrounded = Time.time;
        else if (!_collisionDown && groundedCheck)
        {
            _coyoteUsable = true;
            LandingThisFrame = true;

            RestoreJumps();
        }

        _collisionDown = groundedCheck;
        _collisionUp = _ceilChecker.IsTouchingLayers(_groundFilter.layerMask);
    }

    #endregion

    #region TileInfo

    public CachedTile _cachedTile;
    private bool _cachedTileThisFrame;

    private float TileSpeedMultiplier => _cachedTile.SpeedMultiplier;
    private float TileAccelerationMultiplier => _cachedTile.AccelerationMultiplier;
    private float TileDecelerationMultiplier => _cachedTile.DecelerationMultiplier;
    private float TileJumpMultiplier => _cachedTile.JumpMultiplier;
    private float TileBuoyancySpeedAddon => _cachedTile.BuoyancySpeedAddon;

    private readonly Collider2D[] _colliderBuffer = new Collider2D[8];

    private void CacheTileInfo()
    {
        int count = _groundChecker.GetContacts(_environmentFilter, _colliderBuffer);
        
        _cachedTileThisFrame = false;
        for (int i = 0; i < count; i++)
        {
            switch (_cachedTileThisFrame)
            {
                case false when _cachedTile.Collider == _colliderBuffer[i]:
                    _cachedTileThisFrame = true;
                    break;
                case false when _cachedTile.Collider != _colliderBuffer[i]:
                case true when
                    GetComponentLayerPriority(_colliderBuffer[i]) < GetComponentLayerPriority(_cachedTile.Collider):
                    _cachedTile.Collider = _colliderBuffer[i];
                    _cachedTileThisFrame = _colliderBuffer[i].TryGetComponent(out _cachedTile.Info);
                    break;
            }
        }

        _cachedTile.Cached = _cachedTileThisFrame;
        _cachedTile.UpdateInfo();
    }

    private int GetComponentLayerPriority(Component component)
    {
        var layer = component.gameObject.layer;
        if (layer == _waterFilter.layerMask) return 0;
        if (layer == _bushesFilter.layerMask) return 1;
        if (layer == _groundFilter.layerMask) return 2;

        return 3;
    }

    #endregion

    #region Gravity

    [Header("GRAVITY")] [SerializeField] private float _fallClamp = -40f;
    [SerializeField] private float _minFallSpeed = 80f;
    [SerializeField] private float _maxFallSpeed = 120f;
    private float _fallSpeed;
    private float FallSpeed => _fallSpeed - TileBuoyancySpeedAddon;

    private void CalculateGravity()
    {
        if (_collisionDown)
        {
            if (_currentYSpeed < 0) _currentYSpeed = 0;
            return;
        }

        var fallSpeed = _endedJumpEarly && _currentYSpeed > 0
            ? FallSpeed * _jumpEndEarlyGravityModifier
            : FallSpeed;

        _currentYSpeed -= fallSpeed * Time.fixedDeltaTime;

        if (_currentYSpeed < _fallClamp) _currentYSpeed = _fallClamp;
    }

    #endregion

    #region Walk

    [Header("WALKING")] [SerializeField] private float _groundedAcceleration = 50f;
    [SerializeField] private float _groundedMoveClamp = 4;
    [SerializeField] private float _groundedDeceleration = 30f;

    [Header("FLYING")] [SerializeField] private float _inAirAcceleration = 90f;
    [SerializeField] private float _inAirMoveClamp = 13;
    [SerializeField] private float _inAirDeceleration = 60f;
    [SerializeField] private float _apexBonus = 2;

    private float Acceleration => Grounded ? _groundedAcceleration * TileAccelerationMultiplier : _inAirAcceleration;
    private float Deceleration => Grounded ? _groundedDeceleration * TileDecelerationMultiplier : _inAirDeceleration;
    private float MoveClamp => Grounded ? _groundedMoveClamp : _inAirMoveClamp;

    private void CalculateWalk()
    {
        if (Input.X != 0 && (Input.X > 0 == _currentXSpeed > 0 || _currentXSpeed == 0))
        {
            _currentXSpeed += Input.X * Acceleration * Time.fixedDeltaTime;
            _currentXSpeed = Mathf.Clamp(_currentXSpeed, -MoveClamp, MoveClamp);

            var apexBonus = Mathf.Sign(Input.X) * _apexBonus * _apexPoint;
            _currentXSpeed += apexBonus * Time.fixedDeltaTime;
            _currentXSpeed *= TileSpeedMultiplier;
        }
        else
        {
            _currentXSpeed = Mathf.MoveTowards(_currentXSpeed, 0,
                Deceleration + (Input.X == 0 ? 0 : Acceleration) * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region Jump

    [Header("JUMPING")] [SerializeField] private float _jumpHeight = 30f;
    [SerializeField] private float _jumpApexThreshold = 10f;
    [SerializeField] private float _jumpEndEarlyGravityModifier = 3f;
    [SerializeField] private float _coyoteTimeThreshold = 0.1f;
    [SerializeField] private float _jumpBuffer = 0.1f;
    [SerializeField] private float _jumpDeltaTime = 0.2f;
    [SerializeField] private int _totalJumps = 2;
    private bool _coyoteUsable;
    private bool _endedJumpEarly = true;
    private float _apexPoint;
    private float _lastJumpPressed;
    private float _lastJumpedTime;
    private int _jumpsLeft;

    private bool CanUseCoyote =>
        _coyoteUsable && !_collisionDown && _timeLeftGrounded + _coyoteTimeThreshold > Time.time;

    private bool HasBufferedJump => _collisionDown && _lastJumpPressed + _jumpBuffer > Time.time;

    private bool CanJumpInAir => !_collisionDown && _jumpsLeft > 0 && _lastJumpedTime + _jumpDeltaTime < Time.time;

    private void CalculateJumpApex()
    {
        if (!_collisionDown)
        {
            _apexPoint = Mathf.InverseLerp(_jumpApexThreshold, 0, Mathf.Abs(Velocity.y));
            _fallSpeed = Mathf.Lerp(_minFallSpeed, _maxFallSpeed, _apexPoint);
        }
        else
        {
            _apexPoint = 0;
        }
    }

    private void CalculateJump()
    {
        if (Input.JumpDown && (CanUseCoyote || CanJumpInAir) || HasBufferedJump)
        {
            _currentYSpeed = _jumpHeight * TileJumpMultiplier;
            _endedJumpEarly = false;
            _coyoteUsable = false;
            _timeLeftGrounded = float.MinValue;
            JumpingThisFrame = true;
            _lastJumpedTime = Time.time;
            _jumpsLeft--;
        }
        else
        {
            JumpingThisFrame = false;
        }

        if (!_collisionDown && Input.JumpUp && !_endedJumpEarly && Velocity.y > 0)
        {
            _endedJumpEarly = true;
        }

        if (_collisionUp && _currentYSpeed > 0) _currentYSpeed = 0;
    }

    private void RestoreJumps() => _jumpsLeft = _totalJumps;

    #endregion

    #region Move

    private void MoveCharacter()
    {
        RawMovement = new Vector3(_currentXSpeed, _currentYSpeed);
        _rigidbody.velocity = RawMovement;
    }

    #endregion

    #region Activation

    private bool _activated;

    public void Activate()
    {
        _activated = true;
    }

    #endregion
}

[Serializable]
public struct CachedTile
{
    public Collider2D Collider;
    public float SpeedMultiplier;
    public float AccelerationMultiplier;
    public float DecelerationMultiplier;
    public float JumpMultiplier;
    public float BuoyancySpeedAddon;
    public bool Cached;

    public TileInfo Info;

    public void UpdateInfo()
    {
        if (Cached)
        {
            SpeedMultiplier = Info.SpeedMultiplier;
            AccelerationMultiplier = Info.AccelerationMultiplier;
            DecelerationMultiplier = Info.DecelerationMultiplier;
            JumpMultiplier = Info.JumpMultiplier;
            BuoyancySpeedAddon = Info.BuoyancySpeedAddon;
        }
        else
        {
            SpeedMultiplier = 1f;
            AccelerationMultiplier = 1f;
            DecelerationMultiplier = 1f;
            JumpMultiplier = 1f;
            BuoyancySpeedAddon = 0f;
        }
    }

    public void PlayStepSound(AudioSource source) => Info.PlayStepSound(source);
    public void PlayLandSound(AudioSource source) => Info.PlayLandSound(source);
    public void PlayJumpSound(AudioSource source) => Info.PlayJumpSound(source);
}