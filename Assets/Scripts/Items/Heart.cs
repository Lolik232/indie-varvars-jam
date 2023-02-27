using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider2D))]
public class Heart : MonoBehaviour
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
        if (Health.Hp == 3) return;
        
        Health.Heal();
        if (_clip != null)
        {
            AudioManager.PlaySound(_clip);
        }

        Destroy(gameObject);
    }
}