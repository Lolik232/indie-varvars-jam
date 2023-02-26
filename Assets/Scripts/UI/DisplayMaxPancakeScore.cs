using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayMaxPancakeScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    
    private void Start()
    {
        text.text = PlayerPrefs.GetInt("MaxScore", 0).ToString();
    }
}