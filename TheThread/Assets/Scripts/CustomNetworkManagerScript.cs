using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CustomNetworkManagerScript : MonoBehaviour
{
    public GameObject playerPrefab;

    private void Awake()
    {
        var networkManager = NetworkManager.Singleton;
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager.Singleton is null!");
            return;
        }
        networkManager.NetworkConfig.ConnectionApproval = true;
        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        Debug.Log("CustomNetworkManagerScript initialized with ConnectionApproval enabled.");
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log($"ApprovalCheck for ClientID: {request.ClientNetworkId}, IsHost: {NetworkManager.Singleton.IsHost && request.ClientNetworkId == NetworkManager.Singleton.LocalClientId}");
        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = true;
        StartCoroutine(FinalizeApproval(request.ClientNetworkId, response));
        
    }

    private IEnumerator FinalizeApproval(ulong clientId, NetworkManager.ConnectionApprovalResponse response) {
        Debug.Log($"[Server] Spawning player for ClientID: {clientId}");
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();

        if (netObj == null) {
            Debug.LogError($"Player prefab is missing a NetworkObject component for ClientID: {clientId}");
            response.Approved = false;
            response.Pending = false;
            Object.Destroy(playerInstance);
            yield break;
        }

        try {
            netObj.SpawnWithOwnership(clientId);
            Debug.Log($"[Server] Spawned player for ClientID: {clientId}, OwnerClientId: {netObj.OwnerClientId}, IsSpawned: {netObj.IsSpawned}");
        }
        catch (System.Exception ex) {
            Debug.LogError($"Failed to spawn player for ClientID: {clientId}. Error: {ex.Message}");
            response.Approved = false;
            response.Pending = false;
            Object.Destroy(playerInstance);
            yield break;
        }

        response.Pending = false;
        response.Approved = true;
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
    }

    private void OnServerStarted() {
        Debug.Log("Server started.");
        if (NetworkManager.Singleton.IsHost) {
            Debug.Log($"Host detected. Checking player spawn for ClientID: {NetworkManager.Singleton.LocalClientId}");
            StartCoroutine(EnsureHostPlayerSpawned());
        }
    }

    private IEnumerator EnsureHostPlayerSpawned() {
        // Wait briefly to ensure network state is stable
        yield return new WaitForSeconds(0.5f);

        if (!NetworkManager.Singleton.IsHost) {
            Debug.LogWarning("No longer host, skipping host player spawn check.");
            yield break;
        }

        ulong hostClientId = NetworkManager.Singleton.LocalClientId;
        bool hasPlayer = NetworkManager.Singleton.ConnectedClients.ContainsKey(hostClientId) && NetworkManager.Singleton.ConnectedClients[hostClientId].PlayerObject != null;

        if (!hasPlayer) {
            Debug.Log($"Host player not found for ClientID: {hostClientId}, manually triggering spawn.");
            Vector3 spawnPosition = GetSpawnPosition();
            GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();

            if (netObj != null) {
                try {
                    netObj.SpawnWithOwnership(hostClientId);
                    Debug.Log($"Host player spawned manually. ClientID: {hostClientId}, OwnerClientId: {netObj.OwnerClientId}, IsSpawned: {netObj.IsSpawned}");
                }
                catch (System.Exception ex) {
                    Debug.LogError($"Failed to spawn host player for ClientID: {hostClientId}. Error: {ex.Message}");
                    Object.Destroy(playerInstance);
                }
            }
            else {
                Debug.LogError($"Host player prefab is missing NetworkObject component for ClientID: {hostClientId}!");
                Object.Destroy(playerInstance);
            }
        }
        else {
            Debug.Log($"Host player already spawned for ClientID: {hostClientId}.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}, IsHost: {NetworkManager.Singleton.IsHost && clientId == NetworkManager.Singleton.LocalClientId}");
    }

    private Vector3 GetSpawnPosition()
    {
        return new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
    }
}
