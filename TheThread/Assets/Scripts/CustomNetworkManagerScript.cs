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
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log($"ApprovalCheck for ClientID: {request.ClientNetworkId}");
        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = true;
        StartCoroutine(FinalizeApproval(request.ClientNetworkId, response));
        
    }

    private IEnumerator FinalizeApproval(ulong clientId, NetworkManager.ConnectionApprovalResponse response)
    {
        // Removed delay to avoid timing issues
        Debug.Log($"[Server] Spawning player for ClientId: {clientId}");
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogError("Player prefab is missing a NetworkObject component!");
            response.Pending = false;
            yield break;
        }

        netObj.SpawnWithOwnership(clientId);
        Debug.Log($"[Server] Spawned player for ClientId: {clientId} with OwnerClientId: {netObj.OwnerClientId}");

        // Finalize approval immediately
        response.Pending = false;
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
        //    if (!NetworkManager.Singleton.IsServer) return;


        //    Debug.Log($"Spawning player for ClientId: {clientId}");
        //    Vector3 spawnPosition = GetSpawnPosition();
        //    GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        //    NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        //    if (networkObject == null)
        //    {
        //        Debug.LogError("Player prefab does not have a NetworkObject component!");
        //        return;
        //    }

        //    networkObject.SpawnWithOwnership(clientId);
        //    if (networkObject.OwnerClientId != clientId)
        //    {
        //        Debug.LogWarning($"Ownership not set correctly! Forcing ownership to ClientId: {clientId}");
        //        networkObject.ChangeOwnership(clientId);
        //    }
        //    Debug.Log($"Spawned with OwnerClientId: {networkObject.OwnerClientId}, Expected: {clientId}");
    }

    private Vector3 GetSpawnPosition()
    {
        return new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
    }
}
