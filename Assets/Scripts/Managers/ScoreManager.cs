using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static int Score { get; private set; }
    
    public static int MaxScore { get; set; }

    public static ScoreManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void AddPoints(int points)
    {
        Score += points;
    }
    
    public static void ResetPoints()
    {
        Score = 0;
    }
}