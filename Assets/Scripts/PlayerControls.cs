using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public float walkSpeed = 100;
    Rigidbody rb;
    Vector3 movementDirection;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        Vector3 spawnpoint = GameObject.Find("SpawnPoint").transform.position;
        this.transform.position = spawnpoint;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 cameraDirection = GameObject.Find("PlayerCamera").transform.forward.normalized;
        Vector3 cameraHorizontalDir = GameObject.Find("PlayerCamera").transform.right.normalized;
        cameraDirection.y = 0;
        movementDirection = (cameraDirection * vertical + cameraHorizontalDir * horizontal);
    }
    void FixedUpdate() // Physics updates here
    {
        PerformMove();
    }
    void PerformMove()
    {
        Vector3 preVelocity = rb.velocity;
        Vector3 postVelocity = movementDirection * walkSpeed * Time.deltaTime;
        postVelocity.y = preVelocity.y;
        rb.velocity = postVelocity;
        
    }
}
