using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class DisplayPancakeCount : MonoBehaviour
{
    private int _score;
    [SerializeField] private TextMeshProUGUI text;
    private void Start()
    {
       _score =  PlayerPrefs.GetInt("MaxScore", 0);
       text.text = _score.ToString();
    }

    // Update is called once per frame
    private void Update()
    {
        text.text = ScoreManager.Score.ToString();
    }

    private void OnDestroy()
    {
        ScoreManager.MaxScore = ScoreManager.Score;
        ScoreManager.ResetPoints();
    }
}
