using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider2D))]
public class Pancake : MonoBehaviour
{
   
    [SerializeField] private int _points;
    [SerializeField] private AudioClip _clip;

    public int Points => _points;

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
        ScoreManager.AddPoints(_points);
        if (_clip != null)
        {
            AudioManager.PlaySound(_clip);
        }
        Destroy(gameObject);
    }
}