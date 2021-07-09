using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPanel : MonoBehaviour
{
    private bool camSwitch = false;
    private bool unpushed = true;
    /*
    void OnMouseOver()
    {
        Debug.Log("Over");
        if (Input.GetKeyDown("e") && !camSwitch)
        {
            GameObject camObj = GameObject.Find("PlayerCamera");
            (camObj.GetComponent("Camera") as Camera).enabled = false;
            camObj = GameObject.Find("SubCamera");
            (camObj.GetComponent("Camera") as Camera).enabled = true;
            camSwitch = true;
        }
    }
    void Update()
    {




        if(camSwitch && Input.GetKeyDown("e"))
        {
            GameObject camObj = GameObject.Find("SubCamera");
            (camObj.GetComponent("Camera") as Camera).enabled = false;
            camObj = GameObject.Find("PlayerCamera");
            (camObj.GetComponent("Camera") as Camera).enabled = true;
            camSwitch = false;
        }
    }
    */
    void Update()
    {
        if (HoveringOverControls() && Input.GetKeyDown("e") && unpushed)
        {
            GameObject camObj = GameObject.Find("PlayerCamera");
            (camObj.GetComponent("Camera") as Camera).enabled = false;
            camObj = GameObject.Find("SubCamera");
            (camObj.GetComponent("Camera") as Camera).enabled = true;
            camSwitch = true;
            unpushed = false;
        }
        if (!Input.GetKeyDown("e"))
        {
            unpushed = true;
        }
        if(Input.GetKeyDown("e") && unpushed)
        {
            GameObject camObj = GameObject.Find("SubCamera");
            (camObj.GetComponent("Camera") as Camera).enabled = false;
            camObj = GameObject.Find("PlayerCamera");
            (camObj.GetComponent("Camera") as Camera).enabled = true;
            unpushed = false;
        }
    }
    static bool HoveringOverControls()
    {
        RaycastHit hit;
        Camera c = GameObject.Find("PlayerCamera").GetComponent("Camera") as Camera;
        Vector3 cameraCenter = c.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, c.nearClipPlane));
        if (Physics.Raycast(cameraCenter, c.transform.forward, out hit, c.farClipPlane))
        {
            if (hit.transform.gameObject.name == "ControlPanel")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}
