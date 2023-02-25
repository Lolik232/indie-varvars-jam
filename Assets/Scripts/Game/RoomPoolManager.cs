using System;
using System.Collections.Generic;
using Events;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class RoomPoolManager : MonoBehaviour
    {
        public UnityEvent<Action> RoomUnloaded;
        

        [Header("Channels")]
        [SerializeField] private GameObjectEventChannelSO _loadedRoomEventChannelSO;


        //TODO: int -> Room
        private LinkedList<GameObject> _loadedRooms;

        [Header("Settings")]
        [SerializeField] private int _maxLoadedRooms = 3;

        private void Awake()
        {
            if (_loadedRoomEventChannelSO == null)
            {
                throw new ArgumentNullException($"Loaded room event channel is null");
            }
        }

        private void OnEnable()
        {
            _loadedRoomEventChannelSO.OnEventRaised += LoadNewRoom;
        }

        private void OnDisable()
        {
            _loadedRoomEventChannelSO.OnEventRaised -= LoadNewRoom;
        }

        public void UnloadLastRoom()
        {
            if (_loadedRooms.Count > _maxLoadedRooms)
            {
                UnloadLastRoom();
            }

            var roomToUnload = _loadedRooms.First.Value;

            
            
            _loadedRooms.RemoveFirst();
            roomToUnload.SetActive(false);
            Destroy(roomToUnload, 0.2f);
        }

        private void LoadNewRoom(GameObject newRoom)
        {
            _loadedRooms.AddLast(newRoom);
        }
    }
}