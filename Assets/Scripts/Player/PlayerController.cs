using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Activator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
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

    private void Awake()
    {
        _transform = transform;
        _collider = GetComponent<Collider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
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

        CalculateWalk();
        CalculateJumpApex();
        CalculateGravity();
        CalculateJump();

        MoveCharacter();
    }

    #endregion

    #region Collisions

    [Header("COLLISION")] [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _detectionRayLength = 0.1f;
    [SerializeField] private float _rectBuffer = 0.05f;

    private Rect _rectDown;
    private bool _collisionDown;

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
        }

        _collisionDown = groundedCheck;

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
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.yellow;
        var bounds = _collider.bounds;
        var position = _transform.position;

        Gizmos.DrawWireCube(_rectDown.center, _rectDown.size);

        Gizmos.color = Color.red;
        var move = new Vector3(_currentXSpeed, _currentYSpeed) * Time.fixedDeltaTime;
        Gizmos.DrawWireCube(position + bounds.center + move, bounds.size);
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

    [Header("WALKING")] [SerializeField] private float _acceleration = 90f;
    [SerializeField] private float _moveClamp = 13;
    [SerializeField] private float _deceleration = 60f;
    [SerializeField] private float _apexBonus = 2;

    private void CalculateWalk()
    {
        if (Input.X != 0)
        {
            _currentXSpeed += Input.X * _acceleration * Time.fixedDeltaTime;
            _currentXSpeed = Mathf.Clamp(_currentXSpeed, -_moveClamp, _moveClamp);

            var apexBonus = Mathf.Sign(Input.X) * _apexBonus * _apexPoint;
            _currentXSpeed += apexBonus * Time.fixedDeltaTime;
        }
        else
        {
            _currentXSpeed = Mathf.MoveTowards(_currentXSpeed, 0, _deceleration * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region Jump

    [Header("JUMPING")] [SerializeField] private float _jumpHeight = 30f;
    [SerializeField] private float _jumpApexThreshold = 10f;
    [SerializeField] private float _jumpEndEarlyGravityModifier = 3f;
    [SerializeField] private float _coyoteTimeThreshold = 0.1f;
    [SerializeField] private float _jumpBuffer = 0.1f;
    private bool _coyoteUsable;
    private bool _endedJumpEarly = true;
    private float _apexPoint;
    private float _lastJumpPressed;

    private bool CanUseCoyote =>
        _coyoteUsable && !_collisionDown && _timeLeftGrounded + _coyoteTimeThreshold > Time.time;

    private bool HasBufferedJump => _collisionDown && _lastJumpPressed + _jumpBuffer > Time.time;

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
        if (Input.JumpDown && CanUseCoyote || HasBufferedJump)
        {
            _currentYSpeed = _jumpHeight;
            _endedJumpEarly = false;
            _coyoteUsable = false;
            _timeLeftGrounded = float.MinValue;
            JumpingThisFrame = true;
        }
        else
        {
            JumpingThisFrame = false;
        }

        if (!_collisionDown && Input.JumpUp && !_endedJumpEarly && Velocity.y > 0)
        {
            _endedJumpEarly = true;
        }
    }

    #endregion

    #region Move

    private void MoveCharacter()
    {
        _rigidbody.velocity = new Vector3(_currentXSpeed, _currentYSpeed);
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