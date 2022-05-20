using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    const float MAX_JUMP_POWER = 5.0f;

    Rigidbody physics_body;
    Transform frog_transform;
    Transform camera_transform;

    bool is_charging = false;
    bool is_grounded = true; //If on ground or sticking to a wall
    float grace_period = 0.5f; //Amount of time after jumping before frog will stick to wall again.
    float jump_power = 0.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        physics_body = GetComponent<Rigidbody>();
        frog_transform = this.gameObject.transform;
        camera_transform = frog_transform.GetChild(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed) is_charging = true;
        else is_charging = false;
    }

    void FixedUpdate()
    {
        if(!is_grounded && grace_period > 0)
        {
            grace_period -= Time.deltaTime;
            if (grace_period < 0) grace_period = 0.0f;
        }
        else if(is_charging)
        {
            jump_power += 2500 * Time.deltaTime;
        }
        else if(jump_power > 0)
        {
            Vector3 jump_direction = frog_transform.position - camera_transform.position;
            jump_direction.Normalize();
            jump_direction += Vector3.up;
            jump_direction.Normalize();
            physics_body.AddForce(jump_direction * jump_power);
            Debug.Log("Added Force: " + jump_direction + "\nWith Magnitude: " + jump_power);
            jump_power = 0;
        }
    }
}
