using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    private LineRenderer lr1;
    private LineRenderer lr2;
    private Vector3 grapplePoint1;
    private Vector3 grapplePoint2;
    public LayerMask whatIsGrappleable;
    public Transform gunTip1, gunTip2, cam;
    public Transform player;
    private float maxDistance = 100f;
    private SpringJoint joint;
    public float reeling = 2f;

    private Vector3 currentGrapplePosition1;
    private Vector3 currentGrapplePosition2;
    private bool isGrappling1 = false;
    private bool isGrappling2 = false;

    void Awake()
    {
        lr1 = gunTip1.GetComponent<LineRenderer>();
        lr2 = gunTip2.GetComponent<LineRenderer>();

        //Transform topParent = transform;
        //while (topParent.parent != null)
        //{
        //    Debug.Log("Climbing from: " + topParent.name + " to " + topParent.parent.name);
        //    topParent = topParent.parent;
        //}

        //player = topParent;

        //Debug.Log("Top parent assigned: " + player.name);
    }

    public void Start()
    {
        player = transform.root;
        Debug.Log("Top-level parent (player) is: " + player.name);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartGrapple(1);
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            StopGrapple(1);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartGrapple(2);
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            StopGrapple(2);
        }

        if (joint != null) {
            float distanceToTarget = Vector3.Distance(player.position, joint.connectedAnchor);
            joint.maxDistance = Mathf.MoveTowards(joint.maxDistance, distanceToTarget * 0.25f, reeling * Time.deltaTime);
            joint.minDistance = Mathf.MoveTowards(joint.minDistance, 0f, reeling * Time.deltaTime);
        }

    }

    void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple(int hook)
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxDistance, whatIsGrappleable))
        {
            // If no joint exists, create one
            if (joint == null)
            {
                joint = player.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.spring = 4.5f;
                joint.damper = 7f;
                joint.massScale = 4.5f;

            }

            if (hook == 1)
            {
                grapplePoint1 = hit.point;
                isGrappling1 = true;
                currentGrapplePosition1 = gunTip1.position;
                lr1.positionCount = 2;
            }
            else if (hook == 2)
            {
                grapplePoint2 = hit.point;
                isGrappling2 = true;
                currentGrapplePosition2 = gunTip2.position;
                lr2.positionCount = 2;
            }

            UpdateJoint();
        }
    }

    void StopGrapple(int hook)
    {
        if (hook == 1)
        {
            isGrappling1 = false;
            lr1.positionCount = 0;
        }
        else if (hook == 2)
        {
            isGrappling2 = false;
            lr2.positionCount = 0;
        }

        // If neither grapple is active, remove the joint
        if (!isGrappling1 && !isGrappling2)
        {
            Destroy(joint);
        }
        else
        {
            UpdateJoint();
        }
    }

    void UpdateJoint(){
        if (joint == null) return;

        if (isGrappling1 && isGrappling2){
            // Pull towards the midpoint of both grapple points
            joint.connectedAnchor = (grapplePoint1 + grapplePoint2) / 2f;
        }
        else if (isGrappling1){
            joint.connectedAnchor = grapplePoint1;
        }
        else if (isGrappling2){
            joint.connectedAnchor = grapplePoint2;
        }

        float distanceFromPoint = Vector3.Distance(player.position, joint.connectedAnchor);
        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;
    }

    void DrawRope(){
        if (isGrappling1){
            currentGrapplePosition1 = Vector3.Lerp(currentGrapplePosition1, grapplePoint1, Time.deltaTime * 8f);
            lr1.SetPosition(0, gunTip1.position);
            lr1.SetPosition(1, currentGrapplePosition1);
        }

        if (isGrappling2){
            currentGrapplePosition2 = Vector3.Lerp(currentGrapplePosition2, grapplePoint2, Time.deltaTime * 8f);
            lr2.SetPosition(0, gunTip2.position);
            lr2.SetPosition(1, currentGrapplePosition2);
        }
    }

    public bool IsGrappling(){
        return joint != null;
    }

}
