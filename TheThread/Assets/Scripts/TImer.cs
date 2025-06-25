using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TImer : NetworkBehaviour {
    public TextMeshProUGUI timerText;
    public NetworkObject cageNetworkObject;

    [Tooltip("Assign all 5 cage parts here")]
    public GameObject[] cageParts;

    [Tooltip("Tag of the trigger to stop the survival timer")]
    [SerializeField] private string finishTriggerTag = "FinishLine"; // Tag for the trigger object

    private NetworkVariable<float> syncedStartTime = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private const float countdownDuration = 10f;
    private bool cageRemovedByServer = false;
    private bool countdownStarted => syncedStartTime.Value > 0f;

    private NetworkVariable<float> survivalStartTime = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> survivalTimerRunning = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> finalSurvivalTime = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn() {
        Debug.Log("OnNetworkSpawn: Camera.main is assigned: " + (Camera.main != null));

        if (cageParts == null || cageParts.Length == 0) {
            GameObject[] foundParts = new GameObject[5];
            foundParts[0] = GameObject.Find("CagePart");
            foundParts[1] = GameObject.Find("CagePart1");
            foundParts[2] = GameObject.Find("CagePart2");
            foundParts[3] = GameObject.Find("CagePart3");
            foundParts[4] = GameObject.Find("CagePart4");

            cageParts = foundParts;
        }

        Debug.Log("OnNetworkSpawn: cageParts assigned? " + (cageParts != null && cageParts.Length == 5));

        if (timerText == null) {
            var go = GameObject.Find("TimerText");
            if (go != null)
                timerText = go.GetComponent<TextMeshProUGUI>();
        }

        if (cageNetworkObject == null) {
            var cageGO = GameObject.Find("Cage");
            if (cageGO != null)
                cageNetworkObject = cageGO.GetComponent<NetworkObject>();
        }
    }

    void Update() {
        // On clients, if PlayerFinish.Local is null or finished, don't update timer display
        if (!IsServer) {
            if (PlayerFinish.Local == null || PlayerFinish.Local.HasFinished()) {
                return;
            }
        }

        // Server-only logic
        if (IsServer) {
            if (!countdownStarted && Input.GetKeyDown(KeyCode.F)) {
                if (IsLookingAtCage()) {
                    StartCountdown();
                }
            }

            if (countdownStarted && timerText != null) {
                float serverTime = (float)NetworkManager.Singleton.ServerTime.Time;
                float timeLeft = Mathf.Max(0f, countdownDuration - (serverTime - syncedStartTime.Value));

                if (!cageRemovedByServer && timeLeft <= 0f) {
                    RemoveCageForEveryone();
                    survivalStartTime.Value = serverTime;
                    survivalTimerRunning.Value = true;
                    Debug.Log("Server: Survival timer started at " + survivalStartTime.Value);
                }
            }
        }

        // Show countdown timer if active and cage not removed
        if (countdownStarted && timerText != null && !cageRemovedByServer) {
            float serverTime = (float)NetworkManager.Singleton.ServerTime.Time;
            float timeLeft = Mathf.Max(0f, countdownDuration - (serverTime - syncedStartTime.Value));
            timerText.text = "Countdown: " + timeLeft.ToString("F2");
        }
        // Show survival timer if running
        else if (survivalTimerRunning.Value && timerText != null) {
            float serverTime = (float)NetworkManager.Singleton.ServerTime.Time;
            float survivalTime = serverTime - survivalStartTime.Value;
            timerText.text = "Survival: " + survivalTime.ToString("F2");
        }
        // Show final survival time if timer stopped
        else if (!survivalTimerRunning.Value && finalSurvivalTime.Value > 0f && timerText != null) {
            timerText.text = "Final Survival Time: " + finalSurvivalTime.Value.ToString("F2");
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (IsServer && other.CompareTag(finishTriggerTag)) {
            Debug.Log("Trigger entered by: " + other.gameObject.name + " with tag: " + other.tag);

            // Check if the player's root or child has the Player tag
            Transform playerRoot = other.transform.root;
            if (playerRoot.CompareTag("Player") || other.CompareTag("Player")) {
                Debug.Log("Player hit finish trigger. Stopping survival timer.");
                if (survivalTimerRunning.Value) {
                    float serverTime = (float)NetworkManager.Singleton.ServerTime.Time;
                    finalSurvivalTime.Value = serverTime - survivalStartTime.Value;
                    survivalTimerRunning.Value = false;
                    Debug.Log("Server: Survival timer stopped. Final time: " + finalSurvivalTime.Value);
                }
            }
        }
    }

    private void StartCountdown() {
        syncedStartTime.Value = (float)NetworkManager.Singleton.ServerTime.Time;
        Debug.Log("Server: Countdown started at time: " + syncedStartTime.Value);
    }

    private void RemoveCageForEveryone() {
        cageRemovedByServer = true;
        Debug.Log("Server: Destroying cage parts.");
        foreach (GameObject part in cageParts) {
            if (part != null)
                Destroy(part);
        }
        DestroyCageClientRpc();
    }

    [ClientRpc]
    private void DestroyCageClientRpc() {
        foreach (GameObject part in cageParts) {
            if (part != null)
                Destroy(part);
        }
    }

    private bool IsLookingAtCage() {
        Camera cam = Camera.main;
        if (cam == null) {
            Debug.LogWarning("Camera.main not found.");
            return false;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f)) {
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name);
            if (hit.collider.CompareTag("CagePart")) {
                Debug.Log("Looking at cage part (via tag): " + hit.collider.name);
                return true;
            }
            Debug.Log("Hit something, but not a cage part.");
        }
        else {
            Debug.Log("Raycast hit nothing.");
        }
        return false;
    }

    public float GetSurvivalTime() {
        if (!survivalTimerRunning.Value && finalSurvivalTime.Value > 0f) {
            return finalSurvivalTime.Value;
        }
        if (!survivalTimerRunning.Value) {
            return 0f;
        }
        return (float)NetworkManager.Singleton.ServerTime.Time - survivalStartTime.Value;
    }
}