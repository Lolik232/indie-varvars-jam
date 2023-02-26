using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DisplayActiveItem : MonoBehaviour
{
    [SerializeField] private GameObject activeItem;

    // Update is called once per frame
    private void Update()
    {
        activeItem.SetActive(Inventory.Chicken);
    }
    
    public void OnUseItemInput()
    {
        Inventory.UseChicken();
    }
}
