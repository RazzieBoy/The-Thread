using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using System.Globalization;

public class MovementPlayer : NetworkBehaviour {
    [Header("References")]
    [Header("Speeds")]
    public float moveSpeed;
    public float walkSpeed;
    public float slideSpeed;
    public float slideDrag;
    public float groundDrag;

    [Header("JumpStats")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMulitplier;
    public bool canJump;

    [Header("CrouchStats")]
    public float crouchSpeed;
    public float crouchYScale;
    public float startYScale;

    [Header("SlidingStats")]
    public float slideYScale;
    public bool sliding;

    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode slideKey = KeyCode.LeftControl;

    [Header("PlayerStats")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public LayerMask whatIsSkybox;
    bool grounded;

    [Header("SlopeInfo")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

    [SerializeField] public Transform orientation;
    public GameObject cameraPrefab;
    private GameObject camInstance;
    [SerializeField] private Transform cameraPos;

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;

    public MovementState state;
    public enum MovementState {
        walking,
        crouching,
        sliding,
        air

    }

    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<Quaternion> goggleRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = !IsOwner;
        rb.freezeRotation = true;

        if (IsOwner)
        {
            // Disable Cube MeshRenderer locally
            Transform GoggleTransform = transform.Find("Goggle"); // Adjust path if needed
            if (GoggleTransform != null)
            {
                MeshRenderer GoggleRenderer = GoggleTransform.GetComponent<MeshRenderer>();
                if (GoggleRenderer != null)
                {
                    GoggleRenderer.enabled = false;
                    Debug.Log("Disabled Cube MeshRenderer for local player");
                }
                else
                {
                    Debug.LogWarning("MeshRenderer not found on Cube");
                }
            }
            else
            {
                Debug.LogWarning("Could not find Cube");
            }

            // Instantiate camera for owner
            camInstance = Instantiate(cameraPrefab);
            camInstance.transform.SetParent(transform);
            PlayerCam camScript = camInstance.GetComponent<PlayerCam>();
            MoveCameraScript moveScript = camInstance.GetComponent<MoveCameraScript>();
            if (camScript != null)
            {
                camScript.SetupOrientation(orientation);
            }
        }

        if (IsServer)
        {
            networkPosition.Value = transform.position;
        }
    }

    private void Awake()
    {
        if (orientation == null)
            orientation = transform.Find("Orientation");

        if (cameraPos == null)
            cameraPos = transform.Find("CameraPos");

        rb = GetComponent<Rigidbody>();
        if (rb == null)
           // Debug.LogError("Rigidbody component missing on player!");
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // Update is called once per frame
    void Update() {
        //Debug.Log($"Player {gameObject.name} IsOwner: {IsOwner}, ClientId: {OwnerClientId}");
        if (IsOwner) {
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 1f, whatIsGround);

            MyInput();
            SpeedControl();
            StateHandler();

            if (state == MovementState.walking)
            {
                rb.drag = groundDrag;
            }
            else if (state == MovementState.crouching)
            {
                rb.drag = groundDrag;
            }
            else if (state == MovementState.sliding)
            {
                rb.drag = slideDrag;
            }
            else
                rb.drag = 0;

            if (IsClient && transform.hasChanged) {
                UpdatePositionServerRpc(transform.position);
                transform.hasChanged = false;
            }

            Transform cameraTransform = camInstance != null ? camInstance.transform.Find("Main Camera") : null;
            if (cameraTransform != null)
            {
                goggleRotation.Value = cameraTransform.rotation;
                Transform goggleTransform = transform.Find("Goggle");
                if (goggleTransform != null) 
                {
                    goggleTransform.rotation = goggleRotation.Value;
                }
            }
        }
        else {
            transform.position = Vector3.Lerp(transform.position, networkPosition.Value, Time.deltaTime * 10f);
            Transform goggleTransform = transform.Find("Goggle");
            if(goggleTransform != null)
            {
                goggleTransform.rotation = goggleRotation.Value;
            }
        }

        if (!IsOwner)
        {
            if (rb != null && !rb.isKinematic)
            {
               // Debug.LogWarning($"[Non-owner has non-kinematic RB!] {gameObject.name}, ClientId: {NetworkManager.Singleton.LocalClientId}");
            }
            return;
        }
    }

    private void FixedUpdate() {
        if (IsOwner)
        {
            MovePlayer();
        }
    }

    private void MyInput() {
       // Debug.Log($"Processing input for {gameObject.name}, Horizontal: {Input.GetAxisRaw("Horizontal")}, Vertical: {Input.GetAxisRaw("Vertical")}");
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && canJump && grounded) {
            canJump = false;
            Jump();
            Invoke(nameof(JumpReset), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey)) {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(moveDirection.normalized * 5f, ForceMode.Impulse);
        }
        if (Input.GetKeyDown(slideKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        }
        if (Input.GetKeyUp(crouchKey) || Input.GetKeyUp(slideKey)) {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }


    }

    private void StateHandler() {

        if (Input.GetKey(slideKey))
        {
            state = MovementState.sliding;
        }
        else if (Input.GetKey(crouchKey)) {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        else if (grounded) {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else {
            state = MovementState.air;
        }
    }

    private void MovePlayer() {
        if (Input.GetKey(slideKey) && grounded) { return; }

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope()) {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0) {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMulitplier, ForceMode.Force);

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl() {
        if (OnSlope()) {
            if (rb.velocity.magnitude > moveSpeed) {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            //if (flatVel.magnitude > moveSpeed)
            //{
            //    Vector3 limitedVel = flatVel.normalized * moveSpeed;
            //    rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            //}
        }
    }

    private void Jump() {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void JumpReset() {
        canJump = true;
    }

    private bool OnSlope() {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 1f)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection() {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    [ServerRpc]
    private void UpdatePositionServerRpc(Vector3 newPosition) { 
        networkPosition.Value = newPosition;
    }
}
