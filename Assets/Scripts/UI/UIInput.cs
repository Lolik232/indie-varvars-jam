using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

    public void OnMainMenuButton()
    {
        SceneManager.LoadScene(0);
    }

    public void OnContinueButton()
    {
        Time.timeScale = 1f;
        PlayerInput.all[0].SwitchCurrentActionMap("Player");
    }

    public void OnEscButton()
    {
        Time.timeScale = 0f;
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}