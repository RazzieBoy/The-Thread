using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCameraScript : MonoBehaviour{
    public Transform cameraPosition;

    // Update is called once per frame

    private void Start(){
        //cameraPosition = 
    }

    void Update(){
        transform.position = cameraPosition.position;
    }
}
