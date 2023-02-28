using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class DisplayPancakeCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private void Start()
    {
        text.text = "0";
    }
    private void Update()
    {
        text.text = ScoreManager.Score.ToString();
    }

    private void OnDestroy()
    {
        var maxScore = Math.Max(PlayerPrefs.GetInt("MaxScore", 0), ScoreManager.Score);
        PlayerPrefs.SetInt("MaxScore", maxScore);
        ScoreManager.ResetPoints();
    }
}
