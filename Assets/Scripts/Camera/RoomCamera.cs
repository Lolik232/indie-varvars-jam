using System;
using Cinemachine;
using UnityEngine;

namespace Camera
{
    [RequireComponent(typeof(Collider2D),
                      typeof(Rigidbody2D))]
    public class RoomCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _roomCamera;

        private void Awake()
        {
            _roomCamera = GetComponent<CinemachineVirtualCamera>();

            if (_roomCamera == null)
            {
                throw new InvalidProgramException();
            }
        }

        private void Start()
        {
            _roomCamera.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            _roomCamera.enabled = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _roomCamera.enabled = false;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (_roomCamera.enabled == false) _roomCamera.enabled = true;
        }
    }
}