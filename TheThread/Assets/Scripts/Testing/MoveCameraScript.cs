using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCameraScript : MonoBehaviour{
    public Transform cameraPosition;

    // Optional manual setup method
    public void SetupCameraPosition(Transform cameraPositionTransform)
    {
        cameraPosition = cameraPositionTransform;
    }

    private void Start()
    {
        if (cameraPosition == null)
        {
            Transform root = transform.root;
            Transform[] allChildren = root.GetComponentsInChildren<Transform>();

            foreach (Transform child in allChildren)
            {
                if (child.name == "CameraPos")
                {
                    cameraPosition = child;
                    break;
                }
            }

            if (cameraPosition == null)
            {
                Debug.LogWarning("CameraPos not found in children of " + root.name);
            }
        }
    }

    private void LateUpdate()
    {
        if (cameraPosition != null)
        {
            transform.position = cameraPosition.position;
        }
    }
}
