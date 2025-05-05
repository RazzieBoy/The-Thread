using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour{

    private float movementSpeed;
    public float walkingSpeed;
    public float runningSpeed;
    public float slidingSpeed;
    private float wantedMoveSpeed;
    private float lastWantedMoveSpeed;
    public float speedMulitplier;
    public float slideMulitplier;
    public float groundDrag;

    public float jumpStr;
    public float jumpCooldown;
    public float airTime;
    public bool canJump;

    public float duckSpeed;
    public float yCrouchSize;
    private float yStartSize;

    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode duckKey = KeyCode.LeftControl;

    public float playerSize;
    public LayerMask IsGround;
    bool isGrounded;

    public float maximumSlopeAngle;
    private RaycastHit slopeHit;
    private bool leavingSlope;
    public bool sliding;

    public Transform orientation;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDir;
    Rigidbody rb;

    public MoveState state;
    public enum MoveState{
        walking,
        running,
        ducking,
        sliding,
        air
    }

    void Start(){
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        canJump = true;
        yStartSize = transform.localScale.y;
    }

    void Update(){
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerSize * 0.5f + 0.2f, IsGround);

        PlayerInput();
        SpeedManager();
        StateManager();

        if (isGrounded){
            rb.drag = groundDrag;
        }
        else{
            rb.drag = 0;
        }
    }

    void FixedUpdate(){
        PlayerMoving();
    }

    private void PlayerInput(){
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && canJump && isGrounded){
            canJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(duckKey)){
            transform.localScale = new Vector3(transform.localScale.x, yCrouchSize, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(duckKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, yStartSize, transform.localScale.z);
        }
    }

    private void StateManager(){
        if (isGrounded){
            state = MoveState.walking;
            wantedMoveSpeed = walkingSpeed;
        }
       
        else if (isGrounded && Input.GetKey(sprintKey)){
            state = MoveState.running;
            wantedMoveSpeed = runningSpeed;
        }

        else if (sliding){
            state = MoveState.sliding;
            if (OnSlope() && rb.velocity.y < 0.1f){
                wantedMoveSpeed = slidingSpeed;
            }
            else{
                wantedMoveSpeed = runningSpeed;
            }
        }

        else if (Input.GetKey(duckKey)){ 
            state = MoveState.ducking;
            wantedMoveSpeed = duckSpeed;
        }

        else{
            state = MoveState.air;
        }

        if (Mathf.Abs(wantedMoveSpeed - lastWantedMoveSpeed) > 4f && movementSpeed != 0){
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else{
            movementSpeed = wantedMoveSpeed;
        }

        lastWantedMoveSpeed = wantedMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed(){
        float time = 0;
        float difference = Mathf.Abs(wantedMoveSpeed - movementSpeed);
        float startValue = movementSpeed;

        while (time < difference) {
            movementSpeed = Mathf.Lerp(startValue, wantedMoveSpeed, time / difference);
            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);
                time += Time.deltaTime * speedMulitplier * slideMulitplier * slopeAngleIncrease;
            }
            else{
                time += Time.deltaTime * speedMulitplier;
                
            }
            yield return null;
        }
        movementSpeed = wantedMoveSpeed;
    }

    private void PlayerMoving(){
        moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !leavingSlope){
            rb.AddForce(GetSlopeMoveDirection(moveDir) * movementSpeed * 20f, ForceMode.Force);
            
            if (rb.velocity.y > 0){
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else if (isGrounded){
            rb.AddForce(moveDir.normalized * movementSpeed * 10f, ForceMode.Force);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDir.normalized * movementSpeed * 10f * airTime, ForceMode.Force);
        }

        rb.useGravity = !OnSlope();
    }

    private void SpeedManager(){
        if (OnSlope() && !leavingSlope){
            if (rb.velocity.magnitude > movementSpeed){
                rb.velocity = rb.velocity.normalized * movementSpeed;
            }
        }

        else{
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude > movementSpeed){
                Vector3 limitedVel = flatVel.normalized * movementSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump(){
        leavingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpStr, ForceMode.Impulse);
    }

    private void ResetJump(){
        canJump = true;
        leavingSlope= false;
    }

    public bool OnSlope() {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerSize * 0.5f + 0.3f)){
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maximumSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction){
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
