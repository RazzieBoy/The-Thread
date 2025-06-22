using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Collections;
using System.Linq;

public class FinishLineWall : NetworkBehaviour
{
    public TMPro.TextMeshProUGUI textMeshPro;

    private NetworkList<PlayerFinishEntry> finishEntries;

    public override void OnNetworkSpawn() {
        if (IsServer) {
            finishEntries = new NetworkList<PlayerFinishEntry>();
            finishEntries.OnListChanged += OnFinishEntriesChanged;
        }
    }

    public override void OnNetworkDespawn() {
        if (IsServer) {
            finishEntries.OnListChanged -= OnFinishEntriesChanged;
            finishEntries.Dispose();
        }
    }

    private void OnFinishEntriesChanged(NetworkListEvent<PlayerFinishEntry> changeEvent) {
        UpdateDisplay();
    }

    private void UpdateDisplay() {
        List<PlayerFinishEntry> sortedList = new List<PlayerFinishEntry>();
        foreach (var entry in finishEntries) {
            sortedList.Add(entry);
        }
        sortedList.Sort((a, b) => a.FinishTime.CompareTo(b.FinishTime)); // fastest first

        List<string> lines = new List<string>();
        int rank = 1;
        foreach (var entry in sortedList) {
            lines.Add($"{rank}. {entry.PlayerName}: {entry.FinishTime:F2}s");
            rank++;
        }

        textMeshPro.text = string.Join("\n", lines);
    }


    [ServerRpc(RequireOwnership = false)]
    public void AddFinishEntryServerRpc(ulong clientId, FixedString32Bytes playerName, float finishTime) {
        finishEntries.Add(new PlayerFinishEntry {
            ClientId = clientId,
            PlayerName = playerName,
            FinishTime = finishTime
        });
    }
}
