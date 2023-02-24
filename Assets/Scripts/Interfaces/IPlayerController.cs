using UnityEngine;
using UnityEngine.InputSystem;

public interface IPlayerController
{
    public Vector3 Velocity { get; }
    public Vector3 RawMovement { get; }
    
    public FrameInput Input { get; }
    
    public bool JumpingThisFrame { get; }
    public bool LandingThisFrame { get; }
    public bool Grounded { get; }

    public void OnMoveInput(InputAction.CallbackContext input);

    public void OnJumpInput(InputAction.CallbackContext input);
    
    public void OnDashInput(InputAction.CallbackContext input);
    
    public void OnUseItemInput(InputAction.CallbackContext input);
}