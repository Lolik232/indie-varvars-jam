using System;
using UnityEngine;

public class Health: MonoBehaviour
{
    public static int Hp { get; private set; }

    public static Timer Protected = new Timer(1);

    public static Health Instance { get; private set; }

    public static event Action KillEvent;
    public static event Action DamageEvent;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void Heal()
    {
        if (Hp != 3) Hp++;
    }

    public static void TakeDamage()
    {
        Hp--;
        DamageEvent?.Invoke();
        Protected.Set();
        if (Hp == 0)
        {
            Kill();
        }
    }

    public static void Kill()
    {
        KillEvent?.Invoke();
    }
}