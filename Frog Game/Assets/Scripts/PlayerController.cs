using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    const float MIN_JUMP_POWER  = 250.0f;
    const float JUMP_POWER_STEP = 600.0f;
    const float MAX_JUMP_POWER  = 1000.0f;
    const float grace_default   = 0.5f;

    Rigidbody physics_body;
    Transform frog_transform;
    Transform pivot_transform;
    Transform camera_transform;

    bool is_charging = false;
    bool is_grounded = true; //If on ground or sticking to a wall
    float grace_period = grace_default; //Amount of time after jumping before frog will stick to wall again.
    float jump_power = 0.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        physics_body = GetComponent<Rigidbody>();
        frog_transform = this.gameObject.transform;
        pivot_transform = frog_transform.GetChild(1).transform;
        camera_transform = pivot_transform.GetChild(0).transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed && is_grounded) is_charging = true;
        else is_charging = false;
    }

    void OnLook(InputValue look_value)
    {
        //Debug.Log("Look vector: " + look_value.Get());
        if(look_value != null)
        {
            Vector2 look_direction = look_value.Get<Vector2>();
            frog_transform.Rotate(new Vector3(0, 1, 0), look_direction.x, Space.Self);
            if(is_grounded) pivot_transform.Rotate(new Vector3(1, 0, 0), -look_direction.y, Space.Self);
            else frog_transform.Rotate(new Vector3(1, 0, 0), -look_direction.y, Space.Self);
            
        }    
    }

    void OnReload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            if (jump_power == 0) jump_power = MIN_JUMP_POWER;
            else jump_power += JUMP_POWER_STEP * Time.deltaTime;
            if (jump_power > MAX_JUMP_POWER) jump_power = MAX_JUMP_POWER;
        }
        else if(jump_power > 0)
        {
            Vector3 jump_direction = frog_transform.position - camera_transform.position;
            jump_direction.Normalize();
            jump_direction += frog_transform.up;
            jump_direction.Normalize();
            physics_body.isKinematic = false;
            physics_body.AddForce(jump_direction * jump_power);
            Debug.Log("Added Force: " + jump_direction + "\nWith Magnitude: " + jump_power);
            jump_power = 0.0f;
            physics_body.useGravity = true;
            is_grounded = false;
            grace_period = grace_default;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(grace_period == 0.0)
        {
            Debug.Log("Gravity off!");
            physics_body.useGravity = false;
            physics_body.isKinematic = true;
            is_grounded = true;
            //frog_transform.Rotate(new Vector3(1, 0, 0), collision.transform.eulerAngles.x - frog_transform.eulerAngles.x + 90, Space.Self);
            //pivot_transform.Rotate(new Vector3(1, 0, 0), collision.transform.eulerAngles.x - frog_transform.eulerAngles.x + 90, Space.Self);
            //frog_transform.Rotate(collision.transform.eulerAngles - frog_transform.eulerAngles, Space.World);
            float old_y = frog_transform.eulerAngles.y;
            frog_transform.eulerAngles = collision.transform.eulerAngles;
            frog_transform.Rotate(new Vector3(0, 1, 0), old_y - frog_transform.eulerAngles.y, Space.Self);
        }
    }
}
