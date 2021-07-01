using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private float rotX;
    public float sensitivity = 5f;
    public float movementSpeed = 50f;
    // Start is called before the first frame update
    void Start()
    {
        rotX = 0;
    }

    // Update is called once per frame

    void Update()
    {

        float y = Input.GetAxis("Mouse X") * sensitivity;
        rotX += Input.GetAxis("Mouse Y") * sensitivity;
        rotX = Mathf.Clamp(rotX, -90.0f, 90.0f);
        transform.eulerAngles = new Vector3(-rotX, transform.eulerAngles.y + y, 0);

        Vector3 dir = new Vector3(0, 0, 0);
        dir.x = Input.GetAxis("Horizontal");
        dir.z = Input.GetAxis("Vertical");
        transform.Translate(dir * movementSpeed * Time.deltaTime);

    }
}