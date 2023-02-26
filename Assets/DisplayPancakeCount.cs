using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class DisplayPancakeCount : MonoBehaviour
{
    private int _score;
    [SerializeField] private TextMeshProUGUI text;
    void Start()
    {
       _score =  PlayerPrefs.GetInt("Score", 0);
       text.text = _score.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = ScoreManager.Score.ToString();
    }
}
