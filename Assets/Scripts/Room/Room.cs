using System;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;


[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
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


    private void Awake()
    {
        if (_enterCollider == null || _roomCollider == null || _outCollier == null)
        {
            throw new NullReferenceException();
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.IsTouching(_roomCollider) == false && PlayerInRoom)
        {
            _outCollier.isTrigger = false;
            PlayerInRoom          = false;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.IsTouching(_roomCollider) &&
            _roomCollider.isTrigger == true)
        {
            _enterCollider.isTrigger = false;
            PlayerInRoom             = true;
        }
    }
}