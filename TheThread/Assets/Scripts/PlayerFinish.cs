using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerFinish : NetworkBehaviour{
    private TImer timerScript;
    private bool localTimerStopped = false;
    public static PlayerFinish Local;
    private FinishLineWall finishLineWall;

    private void Awake() {
        if (IsOwner) {
            Local = this;
        }
    }

    private void Start() {
        if (IsOwner) {
            timerScript = Object.FindFirstObjectByType<TImer>();
            finishLineWall = Object.FindFirstObjectByType<FinishLineWall>();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!IsOwner || localTimerStopped) {
            return;
        }

        if (other.CompareTag("FinishLine")) {
            localTimerStopped = true;

            Debug.Log("You reached the finish line! Timer stopped locally.");

            if (timerScript != null && timerScript.timerText != null) {
                timerScript.timerText.text += "\nFinished!";
            }

            if (finishLineWall != null && timerScript != null) {
                // Get survival time from timer (you'll want to add a method to TImer to get this)
                float finishTime = timerScript.GetSurvivalTime();

                FixedString32Bytes playerName = new FixedString32Bytes($"Player {OwnerClientId}");
                finishLineWall.AddFinishEntryServerRpc(OwnerClientId, playerName, finishTime);
            }
        }
    }

    public bool HasFinished() => localTimerStopped;
}
