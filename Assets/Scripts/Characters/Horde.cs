using System;
using Events;
using UnityEngine;

namespace Characters
{
    public class Horde : MonoBehaviour
    {
        #region Events

        [SerializeField] private IntEventChannelSO     _hungerLevelEventChannelSO;
        // ?? [SerializeField] private Vector2EventChannelSO _onDirectionUpdateEventChannelSO;
        
        #endregion

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
            _hungerLevelEventChannelSO.OnEventRaised += OnHungerLevelUpdated;
        }

        private void OnDisable()
        {
            _hungerLevelEventChannelSO.OnEventRaised -= OnHungerLevelUpdated;
        }

        private void Update()
        {
        }

        private void FixedUpdate()
        {
            
        }

        private void OnHungerLevelUpdated(int level)
        {
            throw new NotImplementedException("implement me");
        }
    }
}