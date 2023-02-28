using System;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class RoomEnter : MonoBehaviour
{
    public CompositeCollider2D col;

    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Player in room enter");
    }
}