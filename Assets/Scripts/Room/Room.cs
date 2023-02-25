using System;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;


public class Room : MonoBehaviour
{
    #region Props

    public Vector2 RoomDirection => _roomDirection;

    #endregion

    [Header("SETTINGS")]
    [SerializeField] private Transform _startRoomPoint;
    [SerializeField] private Transform _endRoomPoint;
    [Space]
    [SerializeField] private Vector2 _roomDirection = Vector2.right;

    [Header("SETUP")]

    #region Colliders

    [SerializeField] private Collider2D _enterCollider;
    [SerializeField] private Collider2D _roomCollider;
    [SerializeField] private Collider2D _outCollier;

    #endregion

    private bool PlayerInRoom { get; set; } = false;

    public Transform StartRoomPoint => _startRoomPoint;
    public Transform EndRoomPoint   => _endRoomPoint;

    private void Awake()
    {
        if (_enterCollider == null ||
            _roomCollider == null ||
            _outCollier == null)
        {
            throw new NullReferenceException();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!PlayerInRoom) return;

        if (other.IsTouching(_roomCollider) == false &&
            PlayerInRoom &&
            other.IsTouching(_outCollier) == false)
        {
            _outCollier.isTrigger = false;
            PlayerInRoom          = false;
#if UNITY_EDITOR
            Debug.Log("Player leave room");
#endif
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (PlayerInRoom) return;

        if (other.IsTouching(_roomCollider) &&
            !other.IsTouching(_enterCollider))
        {
            _enterCollider.isTrigger = false;
            PlayerInRoom             = true;
#if UNITY_EDITOR
            Debug.Log("Player in room");
#endif
        }
    }
}