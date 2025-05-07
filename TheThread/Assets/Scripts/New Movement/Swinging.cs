using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swinging : MonoBehaviour{

    public KeyCode swingKey = KeyCode.Q;

    public LineRenderer lr;
    public Transform gunTip, playerCamera, player;
    public LayerMask Grappleable;

    private float maximumSwingDistance = 35f;
    private Vector3 swingPoint;
    private SpringJoint joint;

    private Vector3 currentGrapplePosition;

    public PlayerMovementScript pm;

    // Start is called before the first frame update
    void Start(){
        
    }

    // Update is called once per frame
    void Update(){
        if (Input.GetKeyDown(swingKey)){
            StartSwinging();
        }
        if (Input.GetKeyUp(swingKey))
        {
            StopSwinging();
        }
    }

    private void LateUpdate(){
        DrawRope();
    }

    private void StartSwinging(){
        pm.isSwinging = true;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maximumSwingDistance, Grappleable)){
            swingPoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        }
    }

    private void StopSwinging(){
        pm.isSwinging = false;

        lr.positionCount = 0;
        Destroy(joint);
    }

    void DrawRope() { 
        if (!joint){
            return;
        }

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
    }
}
