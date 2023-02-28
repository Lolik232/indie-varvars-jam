using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class RoomInto : MonoBehaviour
{
    [FormerlySerializedAs("_roomEnter")] 
    [SerializeField] private RoomEnter _roomEnterBlock;

    [SerializeField] private bool _playerInRoom = false;

    private void OnTriggerStay2D(Collider2D other)
    {
        if(_playerInRoom) return;
        
        _roomEnterBlock.gameObject.SetActive(true);
        _playerInRoom      = true;
        Debug.Log("Player in room");
    }
}