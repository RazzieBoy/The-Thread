using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallVerticalMovevement : MonoBehaviour
{
    private float wallSpeed = 20f;
    public float topHeight = 40f;
    public float bottomHeight = -40f;
    public float maxDelay = 2f;

    private Vector3 moveDirection = Vector3.up;
    private Vector3 startPosition;
    public float movementPhase;

    private void Start() {
        startPosition = transform.position;
        moveDirection = Random.value > 0.5f ? Vector3.up : Vector3.down;
        movementPhase = Random.Range(0.5f, maxDelay);
    }
    private void Update() {
        float movement = Mathf.PingPong(Time.time * wallSpeed * movementPhase, topHeight - bottomHeight) + bottomHeight;
        transform.position = new Vector3(startPosition.x, startPosition.y + movement, startPosition.z);
    }
}
