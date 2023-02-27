using System;
using Events;
using UnityEngine;

namespace Characters
{
    public class Horde : MonoBehaviour
    {
        [SerializeField] private float _speed = 2f;

        private Timer _timer      = new Timer(2f);
        private bool  _runStarted = false;

        [SerializeField] private PlayerController _player;


        #region Private fields

        [SerializeField] private Vector2 _direction = Vector2.right;

        #endregion

        #region Props

        public Transform Transform { get; private set; }

        #endregion

        private void Awake()
        {
            Transform = GetComponent<Transform>();
        }


        private void OnEnable()
        {
            _timer.ResetEvent += Run;
            Room.PlayerEnterInRoom += (Room r) =>
            {
                Transform.position = new Vector3(Transform.position.x, r.transform.position.y, Transform.position.z);
            };
            _timer.Set();
        }

        private void FixedUpdate()
        {
            if (_runStarted == false) return;

            transform.position += Vector3.right * _speed;
        }

        private void Run()
        {
            _runStarted = true;
        }
    }
}