using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Activator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour, IPlayerController, IActivated
{
    // private float _currentXSpeed, _currentYSpeed;

    private float _currentXSpeed;

    #region Input

    public Vector3 Velocity { get; private set; }
    public Vector3 RawMovement { get; private set; }
    public FrameInput Input { get; private set; }
    public bool JumpingThisFrame { get; private set; }
    public bool LandingThisFrame { get; private set; }
    public bool Grounded => _collisionDown;

    private readonly Trigger JumpDownTrigger = new();
    private readonly Trigger JumpUpTrigger = new();
    private bool _dashInput;
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
        _dashInput = input.performed;
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

            UseItemDown = UseItemDownTrigger,

            DashX = _dashInput ? _moveInput.x : 0
        };

        if (Input.UseItemDown)
        {
            Inventory.UseChicken();
            UseItemDownTrigger.Reset();
        }

        if (Input.JumpDown)
        {
            _lastJumpPressed = Time.time;
            _jumpBufferTimer.Set();
            JumpDownTrigger.Reset();
        }
    }

    #endregion

    #region Unity

    [SerializeField] private Collider2D _collider;
    [SerializeField] private Collider2D _groundChecker;
    [SerializeField] private Collider2D _tileChecker;
    [SerializeField] private Collider2D _boundChecker;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;


    private void Awake()
    {
        Health.DamageEvent += () => { _animator.SetTrigger(Damaged); };
        Inventory.ChickenUseEvent += () => { _flyTimer.Set(); };
        _dashTimer = new Timer(_dashTime);
        _flyTimer = new Timer(_flyTime);
        _dashCooldown = new Timer(_dashCooldownTime);
        _dashTimer.ResetEvent += () =>
        {
            _dashCooldown.Set();
            _animator.SetBool(Dashed, false);
        };
        _environmentFilter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = true,
            layerMask = _waterFilter.layerMask | _groundFilter.layerMask | _bushesFilter.layerMask |
                        _springFilter.layerMask
        };
        _pool = new ObjectPool<SpriteRenderer>(
            () => Instantiate(_spriteRendererPrefab),
            source => { source.gameObject.SetActive(true); },
            source => { source.gameObject.SetActive(false); },
            source => { Destroy(source.gameObject); },
            false, 5, 15
        );
    }

    private void Start()
    {
        _baseGravity = _rigidbody.gravityScale;
    }

    private void Update()
    {
        if (!_activated) return;

        GatherInput();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!_activated) return;

        Velocity = _rigidbody.velocity;
        // _currentXSpeed = Velocity.x;
        // _currentYSpeed = Velocity.y;

        ApplyChickenFly();

        if (_isFlying)
        {
            return;
        }

        if (!TryCacheTileInfo(_tileChecker, _environmentFilter))
        {
            TryCacheTileInfo(_groundChecker, _environmentFilter);
        }

        CheckCollisions();
        CalculateDash();

        if (!_dashTimer)
        {
            CalculateWalk();
            CalculateJumpApex();
            CalculateGravity();
            CalculateJump();
        }


        MoveCharacter();
    }

    #endregion

    #region Collisions

    [Header("COLLISION")] [SerializeField] private ContactFilter2D _groundFilter;
    [SerializeField] private ContactFilter2D _ceilFilter;
    [SerializeField] private ContactFilter2D _waterFilter;
    [SerializeField] private ContactFilter2D _springFilter;
    [SerializeField] private ContactFilter2D _bushesFilter;

    private ContactFilter2D _environmentFilter;

    private bool _collisionDown;

    private float _timeLeftGrounded;

    private void CheckCollisions()
    {
        LandingThisFrame = false;
        var groundedCheck = _groundChecker.IsTouchingLayers(_groundFilter.layerMask);

        if (_collisionDown & !groundedCheck)
        {
            _timeLeftGrounded = Time.time;
        }
        else if (!_collisionDown && groundedCheck)
        {
            LandingThisFrame = true;
            PlayLandSound();
        }

        _collisionDown = groundedCheck;

        if (groundedCheck)
        {
            _coyoteUsable = true;
            if (!JumpingThisFrame)
            {
                RestoreJumps();
            }

            _canDash = true;
        }
    }

    #endregion

    #region TileInfo

    [SerializeField] private CachedTile _cachedTile;
    private bool _cachedTileThisFrame;

    private float TileSpeedMultiplier => _cachedTile.SpeedMultiplier;
    private float TileAccelerationMultiplier => _cachedTile.AccelerationMultiplier;
    private float TileDecelerationMultiplier => _cachedTile.DecelerationMultiplier;
    private float TileJumpMultiplier => _cachedTile.JumpMultiplier;
    private float TileBuoyancyAcceleration => _cachedTile.BuoyancyAcceleration;

    private readonly Collider2D[] _colliderBuffer = new Collider2D[8];

    private bool TryCacheTileInfo(Collider2D checker, ContactFilter2D filter)
    {
        var count = checker.GetContacts(filter, _colliderBuffer);

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

        return _cachedTileThisFrame;
    }

    private int GetComponentLayerPriority(Component component)
    {
        var layer = component.gameObject.layer;
        if ((_waterFilter.layerMask >> layer & 1) == 1) return 0;
        if ((_bushesFilter.layerMask >> layer & 1) == 1) return 1;
        if ((_springFilter.layerMask >> layer & 1) == 1) return 2;
        if ((_groundFilter.layerMask >> layer & 1) == 1) return 3;

        return 4;
    }

    #endregion

    #region Gravity

    [Header("GRAVITY")] [SerializeField] private float _failClamp = -40f;

    private float _baseGravity;

    private void CalculateGravity()
    {
        SetGravity(_baseGravity - TileBuoyancyAcceleration);

        if (Grounded) return;

        if (CurrentYVelocity < _failClamp) SetYVelocity(_failClamp);

        if (_endedJumpEarly && CurrentYVelocity > 0) SetYVelocity(CurrentYVelocity / _jumpEndEarlyGravityModifier);
    }

    #endregion

    #region Fly

    [Header("CHICKEN FLY")] [SerializeField]
    private float _flyTime = 2f;

    [SerializeField] private float _flySpeed = 12f;
    private Timer _flyTimer;
    private bool _isFlying;

    private void ApplyChickenFly()
    {
        switch (_isFlying)
        {
            case false when _flyTimer:
            {
                _animator.SetBool(Chicken1, true);
                _rigidbody.gravityScale = 0;
                _collider.isTrigger = true;
                _boundChecker.enabled = true;
                _isFlying = true;
                break;
            }
            case true when _flyTimer || _collider.OverlapCollider(_groundFilter, _colliderBuffer) > 0:
                var input = new Vector2(Input.X, Input.Y).normalized;
                SetXVelocity(input.x * _flySpeed);
                SetYVelocity(input.y * _flySpeed);
                break;
            case true when _collider.OverlapCollider(_groundFilter, _colliderBuffer) == 0 && !_flyTimer:
                _animator.SetBool(Chicken1, false);
                _collider.isTrigger = false;
                _isFlying = false;
                _boundChecker.enabled = false;
                break;
        }
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
    private bool Staying => Mathf.Abs(CurrentXVelocity) < 0.1f;

    private void CalculateWalk()
    {
        if (Input.X != 0 && (Input.X > 0 == CurrentXVelocity > 0 || Staying))
        {
            SetXVelocity(CurrentXVelocity + Input.X * Acceleration * Time.fixedDeltaTime);
            SetXVelocity(Mathf.Clamp(CurrentXVelocity, -MoveClamp, MoveClamp));

            var apexBonus = Mathf.Sign(Input.X) * _apexBonus * _apexPoint;
            SetXVelocity(CurrentXVelocity + apexBonus * Time.fixedDeltaTime);
            SetXVelocity(CurrentXVelocity * TileSpeedMultiplier);
        }
        else
        {
            SetXVelocity(Mathf.MoveTowards(CurrentXVelocity, 0,
                Deceleration + (Input.X == 0 ? 0 : Acceleration) * Time.fixedDeltaTime));
        }
    }

    #endregion

    #region Dash

    [Header("DASH")] [SerializeField] private float _dashVelocity = 80f;
    [SerializeField] private float _dashCooldownTime = 2f;
    [SerializeField] private float _dashTime = 0.1f;
    [SerializeField] private SpriteRenderer _spriteRendererPrefab;

    private static ObjectPool<SpriteRenderer> _pool;

    [SerializeField] private AudioClip[] _dashSound;

    private Timer _dashTimer;
    private Timer _dashCooldown;
    private bool _canDash;

    private void CalculateDash()
    {
        if (_canDash && !_dashTimer && !_dashCooldown && Input.DashX != 0)
        {
            _animator.SetBool(Dashed, true);
            _dashTimer.Set();
            _canDash = false;
            _facingLeft = Input.DashX < 0;
            _spriteRenderer.flipX = _facingLeft;
            SetYVelocity(0);
            SetXVelocity(Input.DashX * _dashVelocity);
            SetGravity(0);
            if (_dashSound.Length != 0)
            {
                AudioManager.PlaySound(_dashSound[Random.Range(0, _dashSound.Length)]);
            }
        }
        else if (_dashTimer)
        {
            StartCoroutine(Fade());
        }
    }

    private IEnumerator Fade()
    {
        var sprite = _pool.Get();
        sprite.sprite = _spriteRenderer.sprite;
        sprite.transform.position = transform.position;
        sprite.enabled = true;
        var color = sprite.color;
        sprite.color = new Color(color.r, color.g, color.b, 0.5f);
        for (var i = 0; i < 20; i++)
        {
            color = sprite.color;
            sprite.color = new Color(color.r, color.g, color.b, color.a - 0.025f);
            yield return null;
        }

        _pool.Release(sprite);
    }

    #endregion

    #region Jump

    [Header("JUMPING")] [SerializeField] private float _jumpHeight = 30f;
    [SerializeField] private float _jumpApexThreshold = 10f;
    [SerializeField] private float _jumpEndEarlyGravityModifier = 3f;
    [SerializeField] private float _coyoteTimeThreshold = 0.1f;
    [SerializeField] private float _jumpBuffer = 0.1f;
    [SerializeField] private int _totalJumps = 2;
    private Timer _jumpBufferTimer = new(0.1f);
    private bool _coyoteUsable;
    private bool _endedJumpEarly = true;
    private float _apexPoint;
    private float _lastJumpPressed;
    private int _jumpsLeft;

    private bool CanUseCoyote =>
        _coyoteUsable && !_collisionDown && _timeLeftGrounded + _coyoteTimeThreshold > Time.time;

    private bool CanJump => _jumpsLeft > 0;

    private bool HasBufferedJump => _lastJumpPressed + _jumpBuffer > Time.time;

    private void CalculateJumpApex()
    {
        if (!_collisionDown)
        {
            _apexPoint = Mathf.InverseLerp(_jumpApexThreshold, 0, Mathf.Abs(Velocity.y));
        }
        else
        {
            _apexPoint = 0;
        }
    }

    private void CalculateJump()
    {
        if (HasBufferedJump && CanJump)
        {
            _jumpBufferTimer.Reset();
            PlayJumpSound();
            SetYVelocity(_jumpHeight * TileJumpMultiplier);
            _endedJumpEarly = false;
            _coyoteUsable = false;
            _timeLeftGrounded = float.MinValue;
            _lastJumpPressed = float.MinValue;
            JumpingThisFrame = true;
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

        // if (_collisionUp && _currentYSpeed > 0) _currentYSpeed = 0;
    }

    private void RestoreJumps()
    {
        _jumpsLeft = _totalJumps;
    }

    #endregion

    #region Move

    private float CurrentXVelocity => _rigidbody.velocity.x;
    private float CurrentYVelocity => _rigidbody.velocity.y;

    private void SetXVelocity(float xVelocity)
    {
        _rigidbody.velocity = new Vector2(xVelocity, _rigidbody.velocity.y);
    }

    private void SetYVelocity(float yVelocity)
    {
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, yVelocity);
    }

    private void SetGravity(float gravity)
    {
        _rigidbody.gravityScale = gravity;
    }

    private void MoveCharacter()
    {
        // RawMovement = new Vector3(_currentXSpeed, _currentYSpeed);
        // _rigidbody.velocity = RawMovement;
    }

    #endregion

    #region Activation

    private bool _activated;

    public void Activate()
    {
        _activated = true;
    }

    #endregion

    #region Animation

    private static readonly int XVelocityAnimFloat = Animator.StringToHash("xVelocity");
    private static readonly int YVelocityAnimFloat = Animator.StringToHash("yVelocity");
    private static readonly int GroundedAnimBool = Animator.StringToHash("grounded");
    private static readonly int InputX = Animator.StringToHash("inputX");

    private bool _facingLeft;
    private static readonly int Damaged = Animator.StringToHash("damaged");
    private static readonly int Dashed = Animator.StringToHash("dashed");
    private static readonly int Chicken1 = Animator.StringToHash("chicken");

    private void UpdateAnimation()
    {
        _facingLeft = Input.X < 0 || Input.X == 0 && _facingLeft;
        _spriteRenderer.flipX = _facingLeft;

        _animator.SetFloat(XVelocityAnimFloat, Mathf.Abs(Velocity.x));
        _animator.SetBool(InputX, Input.X != 0);
        _animator.SetFloat(YVelocityAnimFloat, Velocity.y);
        _animator.SetBool(GroundedAnimBool, Grounded);
    }

    public void PlayStepSound()
    {
        var clip = _cachedTile.GetStepSound();
        if (clip != null)
        {
            AudioManager.PlaySound(clip);
        }
    }

    public void PlayJumpSound()
    {
        var clip = _cachedTile.GetJumpSound();
        if (clip != null)
        {
            AudioManager.PlaySound(clip);
        }
    }

    public void PlayLandSound()
    {
        var clip = _cachedTile.GetLandSound();
        if (clip != null)
        {
            AudioManager.PlaySound(clip);
        }
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
    public float BuoyancyAcceleration;
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
            BuoyancyAcceleration = Info.BuoyancyAcceleration;
        }
        else
        {
            SpeedMultiplier = 1f;
            AccelerationMultiplier = 1f;
            DecelerationMultiplier = 1f;
            JumpMultiplier = 1f;
            BuoyancyAcceleration = 0f;
        }
    }

    public AudioClip GetStepSound()
    {
        return Cached ? Info.GetStepSound() : null;
    }

    public AudioClip GetJumpSound()
    {
        return Cached ? Info.GetJumpSound() : null;
    }

    public AudioClip GetLandSound()
    {
        return Cached ? Info.GetStepSound() : null;
    }
}