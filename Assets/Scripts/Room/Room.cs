using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Vector2 = UnityEngine.Vector2;
using Vector3 = System.Numerics.Vector3;


public class Room : MonoBehaviour
{
    public static event Action<Room> PlayerEnterInRoom;
    public static event Action<Room> PlayerLeaveRoom;

    #region Props

    public Vector2 RoomDirection => _roomDirection;

    #endregion

    [Header("SETTINGS")]
    [SerializeField] private Transform _startRoomPoint;
    [SerializeField] private TilemapCollider2D _startRoomPointCol;
    [SerializeField] private Transform         _endRoomPoint;
    [SerializeField] private TilemapCollider2D _endRoomPointCol;
    [Space]
    [SerializeField] private Vector2 _roomDirection = Vector2.right;

    [Header("SETUP")]

    #region Colliders

    [SerializeField] private Collider2D _enterCollider;
    [SerializeField] private Collider2D _roomCollider;
    [SerializeField] private Collider2D _outCollier;

    #endregion

    private bool PlayerInRoom { get; set; } = false;

    public UnityEngine.Vector3 StartRoomPoint => _startRoomPointCol.bounds.center;
    public UnityEngine.Vector3 EndRoomPoint   => _endRoomPointCol.bounds.center;

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
            OnPlayerLeaveRoom();
#if UNITY_EDITOR
            Debug.Log("Player leave room");
#endif
        }
    }

    private void OnDrawGizmos()
    {
        var boundsEnter = _startRoomPointCol.bounds;
        var boundsExit  = _endRoomPointCol.bounds;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boundsEnter.center, boundsEnter.size);
        Gizmos.DrawWireCube(boundsExit.center, boundsExit.size);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (PlayerInRoom) return;

        if (other.IsTouching(_roomCollider) &&
            !other.IsTouching(_enterCollider))
        {
            _enterCollider.isTrigger = false;
            PlayerInRoom             = true;
            OnPlayerEnterInRoom();
#if UNITY_EDITOR
            Debug.Log("Player in room");
#endif
        }
    }

    private void OnPlayerLeaveRoom()
    {
        PlayerLeaveRoom?.Invoke(this);
    }

    private void OnPlayerEnterInRoom()
    {
        PlayerEnterInRoom?.Invoke(this);
    }
}