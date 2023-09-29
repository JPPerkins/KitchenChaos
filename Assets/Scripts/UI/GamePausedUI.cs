using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePausedUI : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button optionsButton;
    
    private void Awake()
    {
        resumeButton.onClick.AddListener(() =>
        {
            KitchenGameManager.Instance.TogglePauseGame();
        });
        
        mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        
        optionsButton.onClick.AddListener(() =>
        {
            Hide();
            OptionsUI.Instance.Show(Show);
        });
    }
    private void Start()
    {
        KitchenGameManager.Instance.OnLocalGamePaused += KitchenLocalGameManager_OnLocalGamePaused;
        KitchenGameManager.Instance.OnLocalGameUnpaused += KitchenLocalGameManager_OnLocalGameUnpaused;
        Hide();
    }

    private void KitchenLocalGameManager_OnLocalGameUnpaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void KitchenLocalGameManager_OnLocalGamePaused(object sender, EventArgs e)
    {
        Show();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
        resumeButton.Select();
    }
}
