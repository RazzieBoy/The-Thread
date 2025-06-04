using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    void Start()
    {
        // Check if the NetworkManager is present
        if (NetworkManager.Singleton != null)
        {
            // Start host automatically
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host started automatically.");
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton is null! Make sure you have a NetworkManager in the scene.");
        }
    }
}
