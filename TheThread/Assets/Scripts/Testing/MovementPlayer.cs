using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using System.Globalization;

public class MovementPlayer : NetworkBehaviour {
    [Header("References")]
    //Variables storing different speeds
    [Header("Speeds")]
    public float moveSpeed;
    public float walkSpeed;
    public float slideSpeed;
    public float slideDrag;
    public float groundDrag;

    //Variables Storing values for the jump ability
    [Header("JumpStats")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMulitplier;
    public bool canJump;

    //Variables storing values for when the player crouches
    [Header("CrouchStats")]
    public float crouchSpeed;
    public float crouchYScale;
    public float startYScale;

    //Variables storing values for when the player slides
    [Header("SlidingStats")]
    public float slideYScale;
    public bool sliding;

    //Variables for keybinds that is assigned to different abilities
    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode slideKey = KeyCode.LeftControl;

    //Variables storing values for the player body
    [Header("PlayerStats")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public LayerMask whatIsSkybox;
    public bool grounded;

    //Variables storing info for when a player is on a slope
    [Header("SlopeInfo")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

    //Variables that makes sure each player has a camera object
    [SerializeField] public Transform orientation;
    [SerializeField] private Transform cameraPos;
    public GameObject cameraPrefab;
    private GameObject camInstance;
    //Variable making sure that the players goggles is displayed properly
    [SerializeField]
    private Vector3 goggleOffsetValue = new Vector3(0.25f, 0.8f, 0);

    //Variables for player movement
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;

    //Enum containing the different states the player can be in.
    //Each state is assigned dynamiclly to the player depending on what they are doing
    public MovementState state;
    public enum MovementState {
        walking,
        crouching,
        sliding,
        air
    }

    //Network Variables that ensure that important informatin such as the players location is properly displayed on each users machine.
    //Also ensure that each player knows which dirrection the other ones are facing
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<Quaternion> goggleRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> goggleOffset = new NetworkVariable<Vector3>(new Vector3(0.25f, 0.8f, 0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    //Function that runs when an object with a "Network Object" component on it spawns
    public override void OnNetworkSpawn(){
        base.OnNetworkSpawn();
        //Get's the players rigidbody, makes the rigidbody not kinematic and freezes the root of the players rotation
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = !IsOwner;
        rb.freezeRotation = true;

        //Check if the object with the script on it is the owner of the assigned scripts instance
        if (IsOwner){
            // Disable Cube MeshRenderer locally, stops it from blocking client sides camera
            Transform goggleTransform = transform.Find("GoggleOrbit/Goggle");
            if (goggleTransform != null){
                MeshRenderer goggleRenderer = goggleTransform.GetComponent<MeshRenderer>();
                if (goggleRenderer != null){
                    goggleRenderer.enabled = false;
                    Debug.Log("Disabled Cube MeshRenderer for local player");
                }
                else{
                    Debug.LogWarning("MeshRenderer not found on Cube");
                }
                goggleTransform.localRotation = Quaternion.identity;    
            }
            else{
                Debug.LogWarning("Could not find Cube");
            }

            // Instantiate camera for owner
            camInstance = Instantiate(cameraPrefab);
            camInstance.transform.SetParent(transform);
            PlayerCam camScript = camInstance.GetComponent<PlayerCam>();
            MoveCameraScript moveScript = camInstance.GetComponent<MoveCameraScript>();
            if (camScript != null){
                camScript.SetupOrientation(orientation);
            }
            UpdateGoggleOffsetServerRpc(goggleOffsetValue);
        }

        if (IsServer){
            networkPosition.Value = transform.position;
        }
    }

    private void Awake(){
        if (orientation == null) {
            orientation = transform.Find("Orientation");
        }
            
        if (cameraPos == null) {
            cameraPos = transform.Find("CameraPos");
        }

        rb = GetComponent<Rigidbody>();
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

            if (state == MovementState.walking){
                rb.drag = groundDrag;
            }
            else if (state == MovementState.crouching){
                rb.drag = groundDrag;
            }
            else if (state == MovementState.sliding){
                rb.drag = slideDrag;
            }
            else
                rb.drag = 0;

            if (IsClient && transform.hasChanged) {
                UpdatePositionServerRpc(transform.position);
                transform.hasChanged = false;
            }

            Transform cameraTransform = camInstance != null ? camInstance.transform.Find("Main Camera") : null;
            if (cameraTransform != null){
                float yaw = cameraTransform.eulerAngles.y;
                float correctedYaw = (yaw - 90f) % 360f;
                if (correctedYaw < 0) {
                    correctedYaw += 360f;
                }
                goggleRotation.Value = Quaternion.Euler(0, correctedYaw, 0);
                if (IsOwner) {
                    UpdateGoggleOffsetServerRpc(goggleOffsetValue);
                }

                Transform goggleOrbitTransform = transform.Find("GoggleOrbit");
                if (goggleOrbitTransform != null) {
                    Vector3 offset = goggleOffset.Value;
                    float radius = new Vector2(offset.x, offset.z).magnitude;
                    float angle = -correctedYaw * Mathf.Deg2Rad;
                    Vector3 orbitPosition = new Vector3(
                        transform.position.x + radius * Mathf.Cos(angle),
                        transform.position.y + offset.y,
                        transform.position.z + radius * Mathf.Sin(angle)
                        );
                    goggleOrbitTransform.position = orbitPosition;
                    goggleOrbitTransform.rotation = goggleRotation.Value;

                    Transform goggleTransform = goggleOrbitTransform.Find("Goggle");
                    if (goggleTransform != null) {
                        goggleTransform.position = orbitPosition;
                    }
                }
            }
        }
        else{
            transform.position = Vector3.Lerp(transform.position, networkPosition.Value, Time.deltaTime * 10f);
            Transform goggleOrbitTransform = transform.Find("GoggleOrbit");
            if(goggleOrbitTransform != null){
                Vector3 offset = goggleOffset.Value;
                float radius = new Vector2(offset.x, offset.z).magnitude;
                float angle = -goggleRotation.Value.eulerAngles.y * Mathf.Deg2Rad;
                Vector3 orbitPosition = new Vector3(
                    transform.position.x + radius * Mathf.Cos(angle),
                    transform.position.y + offset.y,
                    transform.position.z + radius * Mathf.Sin(angle)
                );
                goggleOrbitTransform.position = orbitPosition;
                goggleOrbitTransform.rotation = goggleRotation.Value;

                Transform goggleTransform = goggleOrbitTransform.Find("Goggle");
                if (goggleTransform != null) {
                    goggleTransform.position = orbitPosition;
                }
            }
        }

        if (!IsOwner){
            if (rb != null && !rb.isKinematic){
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
        if (Input.GetKeyDown(slideKey)){
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        }
        if (Input.GetKeyUp(crouchKey) || Input.GetKeyUp(slideKey)) {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler() {

        if (Input.GetKey(slideKey)){
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

        if (grounded) {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded) {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMulitplier, ForceMode.Force);
        }

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl() {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (OnSlope()) {
            if (rb.velocity.magnitude > moveSpeed) {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else if (!grounded && flatVel.magnitude > 100) {
            Vector3 limitedVel = flatVel.normalized * 100f;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
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

    [ServerRpc]
    private void UpdateGoggleOffsetServerRpc(Vector3 offset) {
        goggleOffset.Value = offset;
    }
}
