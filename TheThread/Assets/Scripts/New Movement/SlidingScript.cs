using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingScript : MonoBehaviour{

    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovementScript pm;

    public float slideTime;
    public float slideStr;
    private float slideTimer;

    public float ySlideScale;
    private float yStartScale;

    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    // Start is called before the first frame update
    void Start(){
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementScript>();
        yStartScale = playerObj.localScale.y;
    }

    // Update is called once per frame
    void Update(){
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0)){
            StartSliding();
        }
        
        if (Input.GetKeyUp(slideKey) && pm.sliding){
            StopSliding();
        }
    }

    private void FixedUpdate(){
        if (pm.sliding){
            SlidingMovement();
        }
    }

    private void StartSliding(){
        pm.sliding = true;
        playerObj.localScale = new Vector3(playerObj.localScale.x, ySlideScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        slideTimer = slideTime;
    }

    private void SlidingMovement(){
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (!pm.OnSlope() || rb.velocity.y > -0.1f){
            rb.AddForce(inputDirection.normalized * slideStr, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }
        else{
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideStr, ForceMode.Force);
        }

        if (slideTimer <= 0){
            StopSliding();
        }
    }

    private void StopSliding(){
        pm.sliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, yStartScale, playerObj.localScale.z);
    }
}
