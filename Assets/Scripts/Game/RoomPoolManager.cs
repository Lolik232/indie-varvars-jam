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
        [SerializeField] private Room _startRoom;

        private readonly LinkedList<Room> _loadedRooms   = new LinkedList<Room>();
        private readonly Queue<Room>      _roomsToRemove = new Queue<Room>();

        [Header("Settings")]
        [SerializeField] private int _maxLoadedRooms = 5;

        private void Awake()
        {
            _roomSelector = GetComponent<RoomSelector>();
        }

        private void OnEnable()
        {
            Room.PlayerLeaveRoom += RoomToRemove;

            _loadedRooms.AddLast(_startRoom);
        }

        private void OnDisable()
        {
            Room.PlayerLeaveRoom -= RoomToRemove;
        }

        private void RoomToRemove(Room leavedRoom)
        {
            if(leavedRoom != _loadedRooms.First.Value) return;
            
            var room = _loadedRooms.First.Value;
            _loadedRooms.RemoveFirst();
            _roomsToRemove.Enqueue(room);
        }

        private void Update()
        {
            if (_roomsToRemove.Count > 2)
            {
                UnloadFirstRoomCoroutine();
            }

            if (_loadedRooms.Count < _maxLoadedRooms)
            {
                LoadLastRoom();
            }
        }

        private void UnloadFirstRoomCoroutine()
        {
            if (_roomsToRemove.Count > 2)
            {
                var roomToUnload = _roomsToRemove.Dequeue();
             
                GameObject o;
                (o = roomToUnload.gameObject).SetActive(false);
                
                Destroy(o);
            }
        }

        private void LoadLastRoom()
        {
            var room = _roomSelector.GenerateRoom(_loadedRooms.Last.Value);
            var instRoom = Instantiate(room);
            
            var res = _loadedRooms.Last.Value.EndRoomPoint;
            res += instRoom.gameObject.transform.position - instRoom.StartRoomPoint;
          
            if (instRoom == null)
            {
                Debug.Log("Cracked... Room was not instantiated");
                throw new ArgumentNullException($"Room was not instantiated");
            }

            instRoom.transform.position = res;

            NewRoom(instRoom);
        }

        private void NewRoom(Room newRoom)
        {
            _loadedRooms.AddLast(newRoom);
        }
    }
}