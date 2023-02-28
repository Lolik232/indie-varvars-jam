using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static bool Chicken { get; private set; }

    public static Inventory Instance { get; private set; }

    public static event Action ChickenUseEvent;

    private void DropChicken(Room room)
    {
        Chicken = false;
    }
    
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Room.PlayerLeaveRoom += DropChicken;
    }

    private void OnDestroy()
    {
        Room.PlayerLeaveRoom -= DropChicken;
    }

    public static void PickChicken()
    {
        Chicken = true;
    }

    public static void UseChicken()
    {
        if (!Chicken) return;
        ChickenUseEvent?.Invoke();
        Chicken = false;
    }
}