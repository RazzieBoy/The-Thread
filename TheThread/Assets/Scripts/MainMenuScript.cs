using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScript : MonoBehaviour{
    public GameObject mainMenu;
    public GameObject gameMode;
    public GameObject settingsMenu;

    public void PlayButton(){
        mainMenu.SetActive(false);
        gameMode.SetActive(true);
    }

    public void SettingsButton(){
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void QuitButtonn(){
        Application.Quit();
    }

    private void Start()
    {
        mainMenu.SetActive(true);
        gameMode.SetActive(false);
        settingsMenu.SetActive(false);
    }

}
