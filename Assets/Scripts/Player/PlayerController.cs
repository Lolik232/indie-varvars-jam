using UnityEngine;
using UnityEngine.InputSystem;

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

    private Collider2D _collider;
    private Rigidbody2D _rigidbody;
    private Transform _transform;
    private AudioSource _audioSource;

    private void Awake()
    {
        _transform = transform;
        _collider = GetComponent<Collider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!_activated) return;

        GatherInput();
    }
    
    //TODO: nahui
    private float _deltaTime = 0.5f;
    private float _lastStepTime;

    private void FixedUpdate()
    {
        if (!_activated) return;

        Velocity = _rigidbody.velocity;

        //TODO: Remove нахуй
        if (Velocity.x != 0 && _colliderCached && _lastStepTime + _deltaTime < Time.time)
        {
            TileInfo.PlaySound(_audioSource, _cachedTileInfo.StepSound);
            _lastStepTime = Time.time;
        }
        //END TODO:

        CheckCollisions();

        CalculateWalk();
        CalculateJumpApex();
        CalculateGravity();
        CalculateJump();

        CheckTiles();
        MoveCharacter();
    }

    #endregion

    #region Collisions

    [Header("COLLISION")] [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _detectionRayLength = 0.1f;
    [SerializeField] private float _rectBuffer = 0.05f;

    private Rect _rectDown, _rectUp;
    private bool _collisionDown, _collisionUp;

    private float _timeLeftGrounded;

    private void CheckCollisions()
    {
        CalculateRays();

        LandingThisFrame = false;
        var groundedCheck = RunDetection(_rectDown);

        if (_collisionDown & !groundedCheck) _timeLeftGrounded = Time.time;
        else if (!_collisionDown && groundedCheck)
        {
            _coyoteUsable = true;
            LandingThisFrame = true;

            RestoreJumps();
        }

        _collisionDown = groundedCheck;
        _collisionUp = RunDetection(_rectUp);

        bool RunDetection(Rect rect)
        {
            return Physics2D.OverlapBox(rect.center, rect.size, 0, _groundLayer);
        }
    }

    private void CalculateRays()
    {
        var b = _collider.bounds;
        _rectDown = new Rect(b.min.x + _rectBuffer / 2, b.min.y - _detectionRayLength + _rectBuffer,
            b.size.x - _rectBuffer, _detectionRayLength);
        _rectUp = new Rect(b.min.x + _rectBuffer / 2, b.max.y - _rectBuffer,
            b.size.x - _rectBuffer, _detectionRayLength);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.yellow;
        var bounds = _collider.bounds;

        Gizmos.DrawWireCube(_rectDown.center, _rectDown.size);
        Gizmos.DrawWireCube(_rectUp.center, _rectUp.size);

        Gizmos.color = Color.red;
        var move = new Vector3(_currentXSpeed, _currentYSpeed) * Time.fixedDeltaTime;
        Gizmos.DrawWireCube(bounds.center + move, bounds.size);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(bounds.center, new Vector2(bounds.center.x, bounds.min.y - _tileCheckingRayLenght));
    }

    #endregion

    #region TileInfo

    [Header("TILE INFO")] [SerializeField] private float _tileCheckingRayLenght = 0.1f;
    private Collider2D _cachedTileCollider;
    private TileInfo _cachedTileInfo;
    private bool _colliderCached;

    private readonly RaycastHit2D[] _hitsBuffer = new RaycastHit2D[4];

    private void CheckTiles()
    {
        var bounds = _collider.bounds;
        var size = Physics2D.LinecastNonAlloc(bounds.center, new Vector2(bounds.center.x, bounds.min.y - _tileCheckingRayLenght), _hitsBuffer, _groundLayer);
        _colliderCached = false;
        for (var i = 0; i < size; i++)
        {
            var currentTileCollider = _hitsBuffer[i].collider;
            if (currentTileCollider == _cachedTileCollider)
            {
                _colliderCached = true;
                continue;
            }

            if (!currentTileCollider.TryGetComponent(out _cachedTileInfo)) continue;
            
            _cachedTileCollider = currentTileCollider;
            _colliderCached = true;
        }
    }

    #endregion

    #region Gravity

    [Header("GRAVITY")] [SerializeField] private float _fallClamp = -40f;
    [SerializeField] private float _minFallSpeed = 80f;
    [SerializeField] private float _maxFallSpeed = 120f;
    private float _fallSpeed;

    private void CalculateGravity()
    {
        if (_collisionDown)
        {
            if (_currentYSpeed < 0) _currentYSpeed = 0;
            return;
        }

        var fallSpeed = _endedJumpEarly && _currentYSpeed > 0
            ? _fallSpeed * _jumpEndEarlyGravityModifier
            : _fallSpeed;

        _currentYSpeed -= fallSpeed * Time.fixedDeltaTime;

        if (_currentYSpeed < _fallClamp) _currentYSpeed = _fallClamp;
    }

    #endregion

    #region Walk

    [Header("WALKING")] [SerializeField] private float _groundedAcceleration = 50f;
    [SerializeField] private float _groundedMoveClamp = 4;
    [SerializeField] private float _groundedDeceleration = 30f;
    [SerializeField] private float _apexBonus = 2;

    [Header("FLYING")] [SerializeField] private float _inAirAcceleration = 90f;
    [SerializeField] private float _inAirMoveClamp = 13;
    [SerializeField] private float _inAirDeceleration = 60f;

    private float Acceleration => Grounded ? _groundedAcceleration : _inAirAcceleration;
    private float Deceleration => Grounded ? _groundedDeceleration : _inAirDeceleration;
    private float MoveClamp => Grounded ? _groundedMoveClamp : _inAirMoveClamp;

    private void CalculateWalk()
    {
        if (Input.X != 0)
        {
            _currentXSpeed += Input.X * Acceleration * Time.fixedDeltaTime;
            _currentXSpeed = Mathf.Clamp(_currentXSpeed, -MoveClamp, MoveClamp);

            var apexBonus = Mathf.Sign(Input.X) * _apexBonus * _apexPoint;
            _currentXSpeed += apexBonus * Time.fixedDeltaTime;
        }
        else
        {
            _currentXSpeed = Mathf.MoveTowards(_currentXSpeed, 0, Deceleration * Time.fixedDeltaTime);
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
            _currentYSpeed = _jumpHeight * (_colliderCached ? _cachedTileInfo.JumpMultiplier : 1);
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
        RawMovement = new Vector3(_currentXSpeed * (_colliderCached ? _cachedTileInfo.SpeedMultiplier : 1),
            _currentYSpeed);
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