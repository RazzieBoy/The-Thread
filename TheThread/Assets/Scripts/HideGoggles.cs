using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;

public class HideGoggles : NetworkBehaviour
{
    private MeshRenderer cubeRenderer;

    void Awake()
    {
        cubeRenderer = GetComponent<MeshRenderer>();
        if (cubeRenderer == null)
        {
            Debug.LogError("MeshRenderer not found on " + gameObject.name + ". This should not happen based on Inspector.");
        }
    }

    public override void OnNetworkSpawn()
    {
        if (cubeRenderer != null)
        {
            // Hide for the local player's own cube
            cubeRenderer.enabled = !IsOwner;
            Debug.Log($"Cube on {gameObject.name} - IsOwner: {IsOwner}, Enabled: {cubeRenderer.enabled}");
        }
    }
}
