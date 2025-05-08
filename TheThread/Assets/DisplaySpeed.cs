using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplaySpeed : MonoBehaviour{

    //public Text speedText;
    public TMP_Text speedText;

    // Update is called once per frame
    void Update(){
        Rigidbody rb = GetComponent<Rigidbody>();
        float speed = rb.velocity.magnitude;
        string speedString = speed.ToString("F2");
        speedText.text = "Speed: " + speedString + " m/s";
    }
}
