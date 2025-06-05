using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ConstantNetworkManager : MonoBehaviour{
    private void Awake(){
        DontDestroyOnLoad(gameObject);
    }
}
