using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject gameMode;
    public GameObject settingsMenu;
    public GameObject KeyMenu;

    private void Start()
    {
        mainMenu.SetActive(true);
        gameMode.SetActive(false);
        settingsMenu.SetActive(false);
        KeyMenu.SetActive(false);
    }

    public void PlayButton()
    {
        mainMenu.SetActive(false);
        gameMode.SetActive(true);
    }

    public void SettingsButton()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void KeyBindButton(){
        settingsMenu.SetActive(false);
        KeyMenu.SetActive(true);
    }

    public void QuitButtonn()
    {
        Application.Quit();
    }

    public void LoadSceneByName()
    {
        bool success = NetworkManager.Singleton.StartHost();
        Debug.Log($"StartHost success: {success}");
        if (success)
        {
            // Load your actual game scene, e.g. "GameScene"
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            Debug.LogError("Failed to start host.");
        }
    }
    public void JoinGame()
    {
        bool success = NetworkManager.Singleton.StartClient();
        if (!success)
        {
            Debug.LogError("Failed to start client. Check network configuration or server status.");
        }
        else {
            NetworkManager.Singleton.StartClient();
        }

    }
    public void LoadNextInBuild()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void BackButton()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        gameMode.SetActive(false);
        KeyMenu.SetActive(false);
    }

}