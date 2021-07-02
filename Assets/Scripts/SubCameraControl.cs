using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubCameraControl : MonoBehaviour
{
    [SerializeField]
    public float cameraDistance = 1f;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 subControl = GameObject.Find("Sub").transform.position;
        this.transform.position = subControl + new Vector3(2 * cameraDistance, cameraDistance, 0);
        //this.transform.eulerAngles = new Vector3(0, -90, 0);
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 subControl = GameObject.Find("Sub").transform.position;
        this.transform.position = subControl + new Vector3(2 * cameraDistance, cameraDistance, 0);
        //this.transform.LookAt(GameObject.Find("Sub").transform);
        //this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x + 45, this.transform.eulerAngles.y, this.transform.eulerAngles.z);
    }
}
