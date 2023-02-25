using System;
using System.Collections;
using System.Collections.Generic;
using Events;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class RoomPoolManager : Singleton<RoomPoolManager>
    {
        public  UnityEvent<Action> RoomUnloaded;
        private RoomSelector       _roomSelector;

        [Header("Channels")]
        [SerializeField] private GameObjectEventChannelSO _loadedRoomEventChannelSO;

        //TODO: int -> Room
        private LinkedList<Room> _loadedRooms;
        private Queue<Room>      _roomsToRemove;

        [Header("Settings")]
        [SerializeField] private int _maxLoadedRooms = 5;

        private void Awake()
        {
            if (_loadedRoomEventChannelSO == null)
            {
                throw new ArgumentNullException($"Loaded room event channel is null");
            }
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
            if (_roomsToRemove.Count < 2)
            {
                return null;
            }

            var roomToUnload = _roomsToRemove.Dequeue();

            _loadedRooms.RemoveFirst();
            roomToUnload.gameObject.SetActive(false);

            Destroy(roomToUnload, 0.2f);
            
            return null;
        }
        

        private IEnumerator LoadLastRoom()
        {
            var room = _roomSelector.GenerateRoom(_loadedRooms.Last.Value);

            var instantiatePosition = _loadedRooms.Last.Value.EndRoomPoint.position;
            instantiatePosition -= room.EndRoomPoint.position;


            var obj = Instantiate(room, instantiatePosition, new Quaternion());
            if (obj == null)
            {
                Debug.Log("Cracked... Room was not instantiated");
                throw new ArgumentNullException($"Room was not instantiated");
            }

            NewRoom(obj);

            return null;
        }

        private void NewRoom(Room newRoom)
        {
            _loadedRooms.AddLast(newRoom);
            _roomsToRemove.Enqueue(_loadedRooms.First.Value);
            _loadedRooms.RemoveFirst();
        }
    }
}