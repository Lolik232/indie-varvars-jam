using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DisplayHP : MonoBehaviour
{
    [SerializeField] private Image img;
    [SerializeField] private List<Sprite> sprites;

    private void Load()
    {
        SceneManager.LoadScene(0);
    }

    private void Start()
    {
        Health.RestoreHp();
        img.sprite = sprites[0];
        Health.KillEvent += Load;
    }

    private void Update()
    {
        img.sprite = sprites[3 - Health.Hp];
    }

    private void OnDisable()
    {
        Health.KillEvent -= Load;
    }
}