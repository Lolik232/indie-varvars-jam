using System;
using UnityEngine;

public class Health: MonoBehaviour
{
    public static int Hp { get; private set; } = 3;

    public static Timer Protected = new(1);

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
        Debug.Log(Hp);
    }

    public void RestoreHp()
    {
        Hp = 3;
    }
    
    public static void TakeDamage()
    {
        Hp--;
        Debug.Log(Hp);
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