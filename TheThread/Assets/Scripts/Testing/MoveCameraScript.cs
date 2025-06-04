using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCameraScript : MonoBehaviour{
    public Transform cameraPosition;

    public void SetupCameraPosition(Transform cameraPositionTransform)
    {
        cameraPosition = cameraPositionTransform;
    }

    void LateUpdate(){
        if (cameraPosition != null) {
            transform.position = cameraPosition.position;
        }
        
    }
}
