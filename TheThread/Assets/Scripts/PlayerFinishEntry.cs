using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public struct PlayerFinishEntry : INetworkSerializable, System.IEquatable<PlayerFinishEntry> {
    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public float FinishTime;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref FinishTime);
    }

    public bool Equals(PlayerFinishEntry other) {
        return ClientId == other.ClientId && PlayerName.Equals(other.PlayerName) && FinishTime == other.FinishTime;
    }
}
