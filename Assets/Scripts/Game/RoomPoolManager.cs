using System;
using System.Collections;
using System.Collections.Generic;
using Events;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class RoomPoolManager : MonoBehaviour
    {
        public  UnityEvent<Action> RoomUnloaded;
        private RoomSelector       _roomSelector;


        [Header("Channels")]
        [SerializeField] private GameObjectEventChannelSO _loadedRoomEventChannelSO;


        //TODO: int -> Room
        private LinkedList<Room> _loadedRooms;

        [Header("Settings")]
        [SerializeField] private int _maxLoadedRooms = 5;

        private void Awake()
        {
            if (_loadedRoomEventChannelSO == null)
            {
                throw new ArgumentNullException($"Loaded room event channel is null");
            }
        }

        private void OnEnable()
        {
            _loadedRoomEventChannelSO.OnEventRaised += NewRoom;
        }

        private void OnDisable()
        {
            _loadedRoomEventChannelSO.OnEventRaised -= NewRoom;
        }

        private void Update()
        {
            if (_loadedRooms.Count > _maxLoadedRooms)
            {
                CoroutineManager.StartRoutine(UnloadFirstRoom());
                CoroutineManager.StartRoutine(LoadLastRoom());
            }
        }

        private IEnumerator UnloadFirstRoom()
        {
            var roomToUnload = _loadedRooms.First.Value;

            _loadedRooms.RemoveFirst();
            roomToUnload.gameObject.SetActive(false);

            Destroy(roomToUnload, 0.2f);

            return null;
        }

        private IEnumerator LoadLastRoom()
        {
            var room = _roomSelector.GenerateRoom(_loadedRooms.Last.Value);

            var instantiatePosition = _loadedRooms.Last.Value.EndRoomPoint;
            
            
            Instantiate(room, _loadedRooms.Last.Value.);
        }

        private void NewRoom(Room newRoom)
        {
            _loadedRooms.AddLast(newRoom);
        }
    }
}