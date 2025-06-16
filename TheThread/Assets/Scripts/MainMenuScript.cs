using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Linq;

public class MainMenuScript : MonoBehaviour{
    public GameObject mainMenu;
    public GameObject gameMode;
    public GameObject settingsMenu;
    public GameObject KeyMenu;
    public GameObject mulitplayerMenu;

    public TMP_InputField joinCodeInput;
    public TMP_Text joinCodeDisplay;
    private bool isInitializing;

    private Stack<GameObject> menuStorage;

    private void Start(){
        menuStorage = new Stack<GameObject>();
        mainMenu.SetActive(true);
        gameMode.SetActive(false);
        settingsMenu.SetActive(false);
        KeyMenu.SetActive(false);
        joinCodeDisplay.text = "";
        isInitializing = false;
    }

    public void PlayButton(){
        menuStorage.Push(mainMenu);
        mainMenu.SetActive(false);
        gameMode.SetActive(true);
    }

    public void SettingsButton(){
        menuStorage.Push(settingsMenu);
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void KeyBindButton(){
        menuStorage.Push(KeyMenu);
        settingsMenu.SetActive(false);
        KeyMenu.SetActive(true);
    }

    public void MultiPlayerButton() {
        menuStorage.Push(gameMode);
        mulitplayerMenu.SetActive(true);
        gameMode.SetActive(false);
    }

    public void QuitButtonn(){
        Application.Quit();
    }

    public void BackButton() {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        gameMode.SetActive(false);
        KeyMenu.SetActive(false);
        mulitplayerMenu.SetActive(false);

        if (menuStorage.Count > 0) { 
            GameObject lastMenu = menuStorage.Pop();
            lastMenu.SetActive(true);
        }
        else {
            mainMenu.SetActive(true);
        }
    }

    public void LoadNextInBuild() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


    public void JoinGame(){
        bool success = NetworkManager.Singleton.StartClient();
        if (!success){
            Debug.LogError("Failed to start client. Check network configuration or server status.");
        }
        else {
            NetworkManager.Singleton.StartClient();
        }

    }

    public void LoadSceneByName(){
        bool success = NetworkManager.Singleton.StartHost();
        Debug.Log($"StartHost success: {success}");
        if (success){
            // Load your actual game scene, e.g. "GameScene"
            SceneManager.LoadScene("SampleScene");
        }
        else{
            Debug.LogError("Failed to start host.");
        }
    }
}