using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DisplayHP : MonoBehaviour
{
    [SerializeField] private Image img;
    [SerializeField] private List<Sprite> sprites;
    
    private void Start()
    {
        img.sprite = sprites[0];
    }
    
    private void Update()
    {
        img.sprite = sprites[3 - Health.Hp];
    }
}
