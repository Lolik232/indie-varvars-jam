using System;
using System.Collections.Generic;
using UnityEngine;

public class Activator : MonoBehaviour
{
    private readonly List<IActivated> _activatedList = new();

    private void Awake()
    {
        GetComponents(_activatedList);
    }

    private void Start()
    {
        Invoke(nameof(Activate), 0.5f);
    }

    private void Activate()
    {
        foreach (var activated in _activatedList)
        {
            activated.Activate();
        }
    }
}