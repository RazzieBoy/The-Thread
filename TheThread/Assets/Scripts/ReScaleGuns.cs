using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReScaleGuns : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl)) {
            transform.localScale = new Vector3(transform.localScale.x, 0.5f, transform.localScale.z);
        }
        else {
            transform.localScale = new Vector3(transform.localScale.x, 0.2f, transform.localScale.z);
        }
    }
}
