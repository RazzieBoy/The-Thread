using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Net;
using TMPro;

public class MainMenuScript : MonoBehaviour {
    public GameObject mainMenu;
    public GameObject gameMode;
    public GameObject settingsMenu;
    public GameObject KeyMenu;
    public GameObject multiplayerMenu;

    public TMP_InputField joinCodeInput;
    public TMP_Text joinCodeDisplay;
    public TMP_Text errorText; //New: For displaying errors to the user
    private bool isInitializing;

    private Stack<GameObject> menuStorage;

    private void Start() {
        menuStorage = new Stack<GameObject>();
        mainMenu.SetActive(true);
        gameMode.SetActive(false);
        settingsMenu.SetActive(false);
        KeyMenu.SetActive(false);
        multiplayerMenu.SetActive(false);
        joinCodeDisplay.text = GetLocalIPAddress();
        errorText.text = "";
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
        multiplayerMenu.SetActive(true);
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
        multiplayerMenu.SetActive(false);

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
        if (isInitializing) {
            errorText.text = "Host already initializing!";
            Debug.LogWarning("Host already initializing, ignoring request.");
            return;
        }

        var networkManager = NetworkManager.Singleton;
        if (networkManager == null) {
            errorText.text = "NetworkManager is missing in the scene!";
            Debug.LogError("NetworkManager.Singleton is null!");
            return;
        }

        var transport = networkManager.GetComponent<UnityTransport>();
        if (transport == null) {
            errorText.text = "Network configuration error!";
            Debug.LogError("UnityTransport component not found on NetworkManager");
            return;
        }

        transport.ConnectionData.Address = "0.0.0.0";
        transport.ConnectionData.Port = 7777;
        //
        transport.ConnectionData.ServerListenAddress = "0.0.0.0";
        Debug.Log($"Starting host on {transport.ConnectionData.Address}:{transport.ConnectionData.Port}, Listen: {transport.ConnectionData.ServerListenAddress}");
        //

        isInitializing = true;
        errorText.text = "Starting host...";
        bool success = networkManager.StartHost();
        Debug.Log($"StartHost success: {success}");
        if (success) {
            joinCodeDisplay.text = GetLocalIPAddress();
            SceneManager.LoadScene("SampleScene");
        }
        else {
            isInitializing = false;
            errorText.text = "Failed to start host!";
            Debug.LogError("Failed to start host.");
        }
    }

    public void JoinGame() {
        if (joinCodeInput == null) {
            errorText.text = "Join code input field is not assigned!";
            Debug.LogError("joinCodeInput is not assigned in the Inspector!");
            return;
        }

        var networkManager = NetworkManager.Singleton;
        if (networkManager == null) {
            errorText.text = "NetworkManager is missing in the scene!";
            Debug.LogError("NetworkManager.Singleton is null!");
            return;
        }

        //
        if (networkManager.IsClient || networkManager.IsHost | networkManager.IsServer) {
            errorText.text = "A network session is already running! Please disconnect first.";
            Debug.LogError("Cannot start client: A network session is already active.");
            return;
        }
        //

        var transport = networkManager.GetComponent<UnityTransport>();
        if (transport == null) {
            errorText.text = "Network configuration error: UnityTransport not found!";
            Debug.LogError("UnityTransport component not found on NetworkManager!");
            return;
        }

        string ipAddress = joinCodeInput.text.Trim();
        if (string.IsNullOrEmpty(ipAddress)) {
            errorText.text = "Please enter a valid IP address!";
            Debug.LogError("IP address is empty! Please enter the host's IP address.");
            return;
        }

        transport.ConnectionData.Address = ipAddress;
        transport.ConnectionData.Port = 7777;
        //
        Debug.Log($"Attempting to connect to {ipAddress}:{transport.ConnectionData.Port}");
        //

        bool success = networkManager.StartClient();
        if (!success) {
            errorText.text = $"Failed to connect to {ipAddress}. Check IP or network.";
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