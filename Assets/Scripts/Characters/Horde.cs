using System;
using UnityEngine;

namespace Characters
{
    public class Horde : MonoBehaviour
    {
        
        // private                  Transform _transform = default;
        [SerializeField] private Vector2   _direction = Vector2.right;
        
        public Transform Transform { get; private set; }
        
        private void Awake()
        {
            Transform = GetComponent<Transform>();
        }

        private void Update()
        {
        }

        private void FixedUpdate()
        {
            
        }
    }
}