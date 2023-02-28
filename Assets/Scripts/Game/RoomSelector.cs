using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public class RoomSelector : MonoBehaviour
    {
        private System.Random rand = new System.Random();

        [SerializeField] private Room[] Up;
        [SerializeField] private Room[] UpChangeDirection;

        [SerializeField] private Room[] Right;
        [SerializeField] private Room[] RightChangeDirection;

        [SerializeField] private Room[] Left;
        [SerializeField] private Room[] LeftChangeDirection;

        [SerializeField] private Room[] Down;
        [SerializeField] private Room[] DownChangeDirection;

        private Room GetRandomRoom(List<Room> selection)
        {
            var index = rand.Next(0, selection.Count);
            return selection[index];
        }

        public Room GenerateRoom(Room lastRoom)
        {
            var selection = new List<Room>();
            if (lastRoom.RoomDirection == Vector2.up)
            {
                selection.AddRange(Up);
                selection.AddRange(UpChangeDirection);
            }

            if (lastRoom.RoomDirection == Vector2.right)
            {
                selection.AddRange(Right);
                selection.AddRange(RightChangeDirection);
            }

            if (lastRoom.RoomDirection == Vector2.left)
            {
                selection.AddRange(Left);
                selection.AddRange(LeftChangeDirection);
            }

            if (lastRoom.RoomDirection == Vector2.down)
            {
                selection.AddRange(Down);
                selection.AddRange(DownChangeDirection);
            }

            return GetRandomRoom(selection);
        }
    }
}