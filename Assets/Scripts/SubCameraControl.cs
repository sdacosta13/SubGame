using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubCameraControl : MonoBehaviour
{
    public Camera PlayerCamera;
    public Camera SubCamera;
    // Start is called before the first frame update
    void Start()
    {
        PlayerCamera.enabled = true;
        SubCamera.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
