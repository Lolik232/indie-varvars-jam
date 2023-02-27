using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider2D))]
public class Chicken : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;


    [SerializeField] private Collider2D _pickupChecker;
    [SerializeField] private LayerMask _playerLayer;

    private void Update()
    {
        if (_pickupChecker.IsTouchingLayers(_playerLayer))
        {
            OnPickup();
        }
    }

    private void OnPickup()
    {
        Inventory.PickChicken();
        if (_clip != null)
        {
            AudioManager.PlaySound(_clip);
        }

        Destroy(gameObject);
    }
}