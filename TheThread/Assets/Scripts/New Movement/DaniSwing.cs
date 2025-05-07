using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DaniSwing : MonoBehaviour{

    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask grappable;
    public Transform guntip, cam, player;
    private float maxDistance = 100f;
    private SpringJoint joint;

    private void Awake(){
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update(){
        if (Input.GetKeyDown(KeyCode.Q)){
            StartSwing();
        }
        if (Input.GetKeyUp(KeyCode.Q)){
            StopSwing();
        }
    }

    private void LateUpdate(){
        DrawRope();
    }

    private void StartSwing(){
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxDistance, grappable)){ 
            grapplePoint = hit.point;
            joint = player.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);
            joint.maxDistance = distanceFromPoint;
            joint.minDistance = 0f;

            joint.spring = 2f;
            joint.damper = 4f;
            joint.massScale = 1f;
            joint.enableCollision = true;

            lr.positionCount = 2;

            player.GetComponent<PlayerMovementScript>().isSwinging = true;
        }
    }

    void DrawRope(){
        if (!joint)
        {
            return;
        }
        lr.SetPosition(0, guntip.position);
        lr.SetPosition(1, grapplePoint);
    }

    private void StopSwing(){
        lr.positionCount = 0;
        Destroy(joint);

        player.GetComponent<PlayerMovementScript>().isSwinging = false;
    }
}
