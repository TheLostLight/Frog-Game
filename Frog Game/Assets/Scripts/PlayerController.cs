using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    const float MAX_JUMP_POWER = 5.0f;

    Rigidbody physics_body;
    
    bool is_grounded = true; //If on ground or sticking to a wall
    float grace_period = 0.5f; //Amount of time after jumping before frog will stick to wall again.
    float jump_power = 0.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        physics_body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnJump()
    {
        Debug.Log("Jump go!");
    }

    void FixedUpdate()
    {
        if(!is_grounded && grace_period > 0)
        {
            grace_period -= Time.deltaTime;
            if (grace_period < 0) grace_period = 0.0f;
        }
    }
}
