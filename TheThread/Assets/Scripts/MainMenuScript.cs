using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;

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

    // Start host with Relay
    public void StartHostWithRelay() {
        if (!CheckNetworkManager()) return;

        StartHostWithRelayAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.LogError($"StartHostWithRelay failed: {task.Exception}");
                joinCodeDisplay.text = "Error starting host.";
            }
        });
    }

    private async Task StartHostWithRelayAsync() {
        if (!await InitializeUnityServicesAsync()) return;

        try {
            // Create Relay allocation (max 4 players, adjust as needed)
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Display join code for clients to use
            joinCodeDisplay.text = $"Join Code: {joinCode}";
            Debug.Log($"Join Code: {joinCode}");

            // Configure NetworkManager with Relay data
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null) {
                Debug.LogError("UnityTransport component not found on NetworkManager.");
                joinCodeDisplay.text = "Network configuration error.";
                return;
            }

            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // Start host
            bool success = NetworkManager.Singleton.StartHost();
            if (success) {
                Debug.Log("Host started successfully with Relay.");
                SceneManager.LoadScene("SampleScene");
            }
            else {
                Debug.LogError("Failed to start host.");
                joinCodeDisplay.text = "Failed to start host.";
            }
        }
        catch (System.Exception e) {
            Debug.LogError($"Relay Host Error: {e.Message}");
            joinCodeDisplay.text = "Error setting up host.";
        }
    }

    // Join as client with Relay
    public void JoinGameWithRelay() {
        if (!CheckNetworkManager()) return;

        if (string.IsNullOrEmpty(joinCodeInput.text)) {
            Debug.LogError("Join code is empty.");
            joinCodeDisplay.text = "Please enter a join code.";
            return;
        }

        JoinGameWithRelayAsync(joinCodeInput.text).ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.LogError($"JoinGameWithRelay failed: {task.Exception}");
                joinCodeDisplay.text = "Error joining game.";
            }
        });
    }

    private async Task JoinGameWithRelayAsync(string joinCode) {
        if (!await InitializeUnityServicesAsync()) return;

        try {
            // Join Relay allocation with join code
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Configure NetworkManager with Relay data
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null) {
                Debug.LogError("UnityTransport component not found on NetworkManager.");
                joinCodeDisplay.text = "Network configuration error.";
                return;
            }

            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            // Start client
            bool success = NetworkManager.Singleton.StartClient();
            if (!success) {
                Debug.LogError("Failed to start client. Check join code or network status.");
                joinCodeDisplay.text = "Failed to join game.";
            }
            else {
                Debug.Log("Client started successfully with Relay.");
            }
        }
        catch (System.Exception e) {
            Debug.LogError($"Relay Join Error: {e.Message}");
            joinCodeDisplay.text = "Error joining game.";
        }
    }

    // Helper to initialize Unity Services
    private async Task<bool> InitializeUnityServicesAsync() {
        if (isInitializing) {
            Debug.Log("Unity Services initialization already in progress.");
            return false;
        }

        if (AuthenticationService.Instance.IsSignedIn) {
            return true;
        }

        isInitializing = true;
        try {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Unity Services initialized and signed in anonymously.");
            return true;
        }
        catch (System.Exception e) {
            Debug.LogError($"Unity Services initialization failed: {e.Message}");
            joinCodeDisplay.text = "Service initialization failed.";
            return false;
        }
        finally {
            isInitializing = false;
        }
    }

    // Check if NetworkManager is valid
    private bool CheckNetworkManager() {
        if (NetworkManager.Singleton == null) {
            Debug.LogError("NetworkManager Singleton is null. Ensure a NetworkManager GameObject exists in the scene.");
            joinCodeDisplay.text = "NetworkManager not found.";
            return false;
        }
        return true;
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