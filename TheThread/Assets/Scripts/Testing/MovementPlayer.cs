using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovementPlayer : MonoBehaviour{
    [Header("References")]
    [Header("Speeds")]
    public float moveSpeed;
    public float walkSpeed;
    public float slideSpeed;

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
    public float slideForce;
    public float maxSlideTime;
    private float slideTimer;
    public float slideYScale;
    public bool sliding;

    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode slideKey = KeyCode.LeftControl;

    [Header("PlayerStats")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("SlopInfo")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState{
        walking,
        crouching,
        sliding,
        air

    }

    // Start is called before the first frame update
    void Start(){
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        canJump = true;

        startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    void Update(){
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        if (Input.GetKey(slideKey) && (horizontalInput != 0 || verticalInput != 0)){
            StartSlide();
        }
        else{
            StopSlide();
        }

        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate(){
        if (sliding){
            SlidingMovement();
        }
        else{
            MovePlayer();
        }
    }

    private void MyInput(){
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && canJump && grounded){
            canJump = false;

            Jump();

            Invoke(nameof(JumpReset), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey)){
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if (Input.GetKeyUp(crouchKey)){
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler(){

        if (sliding){
            state = MovementState.sliding;
            moveSpeed = slideSpeed;
        }
        else if (Input.GetKey(crouchKey)){
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        else if (grounded){
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else{
            state = MovementState.air;
        }
    }

    private void MovePlayer(){
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope()){
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0){
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMulitplier, ForceMode.Force);

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl(){
        if (OnSlope()){
            if (rb.velocity.magnitude > moveSpeed){
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else{
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            //if(flatVel.magnitude > moveSpeed)
            //{
            //    Vector3 limitedVel = flatVel.normalized * moveSpeed;
            //    rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            //}
        }
    }

    private void Jump(){
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void JumpReset(){
        canJump = true;
    }

    private bool OnSlope(){
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)){
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private void StartSlide(){
        if (!grounded) return;

        sliding = true;
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);

        if (grounded) {
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        
        slideTimer = maxSlideTime;

        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Impulse);
    }

    private void StopSlide(){
        sliding  = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

    private Vector3 GetSlopeMoveDirection(){
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void SlidingMovement(){
        slideTimer = Time.deltaTime;

        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (inputDirection.magnitude > 0.1f){
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
        }
        else{
            Vector3 flatvel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            Vector3 decel = -flatvel.normalized * slideForce * 0.5f;
            rb.AddForce(decel, ForceMode.Force);
        }

        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVelocity.magnitude < 2f || slideTimer <= 0f){
            StopSlide();
        }

        if (flatVelocity.magnitude > slideSpeed){
            Vector3 limitedVel = flatVelocity.normalized * slideSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

//    private void SlidingMovement(){
//        slideTimer -= Time.deltaTime;
//        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
//        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

//        // sliding normal
//        if (!OnSlope() || rb.velocity.y > -0.1f){

//            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
//        }

//        // sliding down a slope
//        else{
//            rb.AddForce(GetSlopeMoveDirection() * slideForce, ForceMode.Force);
//        }

//        if (slideTimer <= 0 || flatVel.magnitude < 2f){
//            StopSlide();
//        }

        
//        if (flatVel.magnitude > slideSpeed){
//            Vector3 limitedVel = flatVel.normalized * slideSpeed;
//            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);   
//        }
//    }
}
