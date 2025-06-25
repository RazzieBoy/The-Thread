using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBorder : MonoBehaviour {
    [SerializeField] private Transform startPoint; // Reference to the start position
    [SerializeField] private string playerTag = "Player"; // Tag to identify the player
    [SerializeField] private float yOffset = 0.5f; // Small offset to avoid spawning inside the ground

    private void Start() {
        if (startPoint == null) {
            Debug.LogError("StartPoint is not assigned in the Inspector for " + gameObject.name);
        }
        else {
            Debug.Log("DeathBorder script initialized on " + gameObject.name + ". StartPoint: " + startPoint.position);
        }
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Trigger entered by: " + other.gameObject.name + " with tag: " + other.tag);

        if (other.CompareTag(playerTag)) {
            Debug.Log("Player detected! Teleporting to: " + startPoint.position);

            // Get the root GameObject of the player hierarchy
            Transform playerRoot = other.transform.root;

            // Calculate teleport position with offset
            Vector3 teleportPosition = new Vector3(
                startPoint.position.x,
                startPoint.position.y + yOffset,
                startPoint.position.z
            );

            // Teleport the root (parent) GameObject
            playerRoot.position = teleportPosition;
            playerRoot.rotation = startPoint.rotation;

            // Reset Rigidbody physics if present (check both root and child)
            Rigidbody rb = playerRoot.GetComponent<Rigidbody>() ?? other.GetComponent<Rigidbody>();
            if (rb != null) {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Debug.Log("Player Rigidbody velocity reset.");
            }

            Debug.Log("Player root (" + playerRoot.name + ") teleported to: " + teleportPosition);
        }
        else {
            Debug.Log("Object with tag '" + other.tag + "' entered trigger, but playerTag is '" + playerTag + "'");
        }
    }
}
