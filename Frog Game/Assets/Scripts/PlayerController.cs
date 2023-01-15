using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    enum TONGUE_STATE
    {
        READY, EXTENDING, RETRACTING, DISABLED
    }
    
    const float MIN_JUMP_POWER  = 250.0f;
    const float JUMP_POWER_STEP = 600.0f;
    const float MAX_JUMP_POWER  = 1000.0f;
    const float GRACE_DEFAULT   = 0.5f;
    const float MIN_TONGUE_SIZE = 0.01f;
    const float MAX_TONGUE_SIZE = 6.0f;

    Rigidbody physics_body;
    Transform frog_transform;
    Transform pivot_transform;
    Transform camera_transform;
    Transform tongue_transform;

    bool is_charging = false;
    bool is_grounded = true; //If on ground or sticking to a wall
    float grace_period = 0.0f; //Amount of time after jumping before frog will stick to wall again.
    float jump_power = 0.0f;
    TONGUE_STATE tongue_state = TONGUE_STATE.READY;

    bool ignore_trigger = false; //Unity is trash?
    
    // Start is called before the first frame update
    void Start()
    {
        physics_body = GetComponent<Rigidbody>();
        frog_transform = this.gameObject.transform;
        pivot_transform = frog_transform.GetChild(1).transform;
        camera_transform = pivot_transform.GetChild(0).transform;
        tongue_transform = frog_transform.GetChild(3).transform;
        tongue_transform.localScale = new Vector3(0.1f, MIN_TONGUE_SIZE, 0.1f);
        Cursor.lockState = CursorLockMode.Locked;
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

    void OnTongue(InputValue value)
    {
        if (value.isPressed && tongue_state == TONGUE_STATE.READY) tongue_state = TONGUE_STATE.EXTENDING;
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
            ignore_trigger = true; //Unity is trash?
            physics_body.isKinematic = false;
            physics_body.AddForce(jump_direction * jump_power);
            Debug.Log("Added Force: " + jump_direction + "\nWith Magnitude: " + jump_power);
            jump_power = 0.0f;
            physics_body.useGravity = true;
            is_grounded = false;
            grace_period = GRACE_DEFAULT;
        }

        if (tongue_state == TONGUE_STATE.EXTENDING)
        {
            Vector3 delta_tongue = new Vector3(0.0f, 5.0f, 0.0f)*Time.deltaTime;

            tongue_transform.localScale += delta_tongue;
            tongue_transform.Translate(delta_tongue, Space.Self);

            if(tongue_transform.localScale.y > MAX_TONGUE_SIZE)
            {
                tongue_transform.Translate(new Vector3(0.0f, MAX_TONGUE_SIZE - tongue_transform.localScale.y, 0.0f), Space.Self);
                tongue_transform.localScale = new Vector3(0.1f, MAX_TONGUE_SIZE, 0.1f);
                tongue_state = TONGUE_STATE.RETRACTING;
            }
        }
        else if (tongue_state == TONGUE_STATE.RETRACTING)
        {
            Vector3 delta_tongue = new Vector3(0.0f, -5.0f, 0.0f) * Time.deltaTime;

            tongue_transform.localScale += delta_tongue;
            tongue_transform.Translate(delta_tongue, Space.Self);

            if(tongue_transform.localScale.y < MIN_TONGUE_SIZE)
            {
                tongue_transform.Translate(new Vector3(0.0f, MIN_TONGUE_SIZE - tongue_transform.localScale.y), Space.Self);
                tongue_transform.localScale = new Vector3(0.1f, MIN_TONGUE_SIZE, 0.1f);
                tongue_state = TONGUE_STATE.READY;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (grace_period == 0.0f && (collision.gameObject.CompareTag("sticky") || collision.gameObject.CompareTag("goal")))
        {
            Debug.Log("Gravity off!");
            physics_body.useGravity = false;
            ignore_trigger = true; //Unity is trash?
            physics_body.isKinematic = true;
            is_grounded = true;
            //frog_transform.Rotate(new Vector3(1, 0, 0), collision.transform.eulerAngles.x - frog_transform.eulerAngles.x + 90, Space.Self);
            //pivot_transform.Rotate(new Vector3(1, 0, 0), collision.transform.eulerAngles.x - frog_transform.eulerAngles.x + 90, Space.Self);
            //frog_transform.Rotate(collision.transform.eulerAngles - frog_transform.eulerAngles, Space.World);
            float old_y = frog_transform.eulerAngles.y;
            frog_transform.eulerAngles = collision.transform.eulerAngles;
            frog_transform.Rotate(Vector3.up, old_y - frog_transform.eulerAngles.y, Space.Self);
            frog_transform.Translate(Vector3.up * (frog_transform.InverseTransformPoint(collision.transform.position).y), Space.Self);
        }
        else if (collision.gameObject.CompareTag("grappable"))
        {
            Vector3 direction = collision.contacts[0].point - frog_transform.position;
            direction.Normalize();
            physics_body.AddForce(direction * 1000);
        }

        if (collision.gameObject.CompareTag("goal"))
        {
            Debug.Log("Goal!!!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (ignore_trigger) ignore_trigger = false; //Unity is trash?   
        else if (collider.gameObject.CompareTag("sinkable") && physics_body.isKinematic == false)
            {
                Debug.Log("Exited sinkable");
                OnReload();
            }
    }
}
