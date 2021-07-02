using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubControls : MonoBehaviour
{
    public float speed = 10f;
    public float rotationSpeed = 30f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float translation = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float horizontalRot = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
        this.transform.Rotate(new Vector3(horizontalRot, 0, 0));
        this.transform.position += this.transform.forward * translation;
    }
}
