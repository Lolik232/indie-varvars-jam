using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIInput : MonoBehaviour
{
    [SerializeField] private Button startButton;

    private void Start()
    {
        startButton.Select();
    }

    public void OnPlayButton()
    {
        throw new NotImplementedException();
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
