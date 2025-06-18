using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Linq;

public class MainMenuScript : MonoBehaviour {
    public GameObject mainMenu;
    public GameObject gameMode;
    public GameObject settingsMenu;
    public GameObject KeyMenu;
    public GameObject mulitplayerMenu;

    public TMP_InputField joinCodeInput;
    public TMP_Text joinCodeDisplay;
    private bool isInitializing;

    private Stack<GameObject> menuStorage;

    private void Start() {
        menuStorage = new Stack<GameObject>();
        mainMenu.SetActive(true);
        gameMode.SetActive(false);
        settingsMenu.SetActive(false);
        KeyMenu.SetActive(false);
        mulitplayerMenu.SetActive(false);
        joinCodeDisplay.text = GetLocalIPAddress();
        isInitializing = false;
    }

    public void PlayButton() {
        menuStorage.Push(mainMenu);
        mainMenu.SetActive(false);
        gameMode.SetActive(true);
    }

    public void SettingsButton() {
        menuStorage.Push(settingsMenu);
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void KeyBindButton() {
        menuStorage.Push(KeyMenu);
        settingsMenu.SetActive(false);
        KeyMenu.SetActive(true);
    }

    public void MultiPlayerButton() {
        menuStorage.Push(gameMode);
        mulitplayerMenu.SetActive(true);
        gameMode.SetActive(false);
        joinCodeDisplay.text = GetLocalIPAddress();
    }

    public void QuitButtonn() {
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

    public void LoadSceneByName() {
        var networkManager = NetworkManager.Singleton;
        var transport = networkManager.GetComponent<UnityTransport>();
        if (transport == null) {
            Debug.LogError("UnityTransport component not found on NetworkManager");
            return;
        }

        transport.ConnectionData.Address = "0.0.0.0";
        transport.ConnectionData.Port = 7777;

        bool success = networkManager.StartHost();
        Debug.Log($"StartHost success: {success}");
        if (success) {
            joinCodeDisplay.text = GetLocalIPAddress(); // Show host's IP for clients to copy
            SceneManager.LoadScene("SampleScene");
        }
        else {
            Debug.LogError("Failed to start host.");
        }
    }


    public void JoinGame() {
        var networkManager = NetworkManager.Singleton;
        var transport = networkManager.GetComponent<UnityTransport>();
        if (transport == null) {
            Debug.LogError("UnityTransport component not found on NetworkManager!");
            return;
        }

        // Get IP from input field
        string ipAddress = joinCodeInput.text.Trim();
        if (string.IsNullOrEmpty(ipAddress)) {
            Debug.LogError("IP address is empty! Please enter the host's IP address.");
            return;
        }

        transport.ConnectionData.Address = ipAddress;
        transport.ConnectionData.Port = 7777; // Must match host's port

        bool success = networkManager.StartClient();
        if (!success) {
            Debug.LogError($"Failed to start client. Check IP address ({ipAddress}) or network configuration.");
        }
    }

    private string GetLocalIPAddress() {
        try {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) // IPv4
                {
                    return ip.ToString();
                }
            }
        }
        catch (System.Exception ex) {
            Debug.LogError($"Error getting local IP: {ex.Message}");
        }
        return "127.0.0.1"; // Fallback to localhost
    }
}