using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;

public class HideGoggles : NetworkBehaviour
{
    private MeshRenderer cubeRenderer;

    private void Start()
    {
        cubeRenderer = GetComponent<MeshRenderer>();
        if (IsOwner)
        {
            
        }
        
    }
    //public override void OnNetworkSpawn()
    //{
    //    cubeRenderer = GetComponent<MeshRenderer>();
    //    if (cubeRenderer == null)
    //    {
    //        Debug.LogError("MeshRenderer not found on Cube!");
    //        return;
    //    }
    //    Trigger the visibility update
    //    RequestVisibilityUpdateServerRpc();
    //}

    //[ClientRpc]
    //private void UpdateVisibilityClientRpc(bool isVisible)
    //{
    //    if (cubeRenderer != null)
    //    {
    //        cubeRenderer.enabled = isVisible;
    //    }
    //}

    //public override void OnNetworkDespawn()
    //{
    //    cubeRenderer = null;
    //}

    //[ServerRpc(RequireOwnership = false)]
    //private void RequestVisibilityUpdateServerRpc()
    //{
    //    Server doesn't need to check IsLocalPlayer; it will broadcast to all clients
    //    UpdateVisibilityClientRpc(IsLocalPlayer ? false : true); // Hide for local player, show for others
    //}
}
