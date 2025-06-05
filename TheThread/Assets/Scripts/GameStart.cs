using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameStart : MonoBehaviour{
    void Start(){
        NetworkManager.Singleton.StartHost();
    }
}
