using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    enum FROG_STATE
    {
        RESTING, TRANSITIONING
    }
    
    public GameObject frog;
    public Transform[] pads;

    FROG_STATE current_state = FROG_STATE.RESTING;
    int current_pad = 0;
    int target_pad = 0;
    Vector3 current_position;
    Vector3 target_position;
    float position_progress = 0.0f;

    int MAX_PAD; //Treat as const I guess...
    const float JUMP_PEAK_HEIGHT = 1.0f;
    const float PROGRESS_STEP = 0.01f;
    
    // Start is called before the first frame update
    void Start()
    {
        MAX_PAD = pads.Length - 1;
        current_position = pads[0].position;
    }

    // Update is called once per frame
    void Update()
    {
        if (current_state == FROG_STATE.TRANSITIONING)
        {
            if (position_progress < 1.0f)
            {
                position_progress += PROGRESS_STEP;
                Vector3 updated_position = Vector3.Lerp(current_position, target_position, position_progress);
                Vector3 halfway_point = Vector3.Lerp(current_position, target_position, 0.5f);
                float halfway_distance = Vector3.Distance(current_position, halfway_point);
                float updated_height = JUMP_PEAK_HEIGHT * ((halfway_distance - Vector3.Distance(updated_position, halfway_point)) / halfway_distance);
                updated_position.y = updated_height;
                frog.transform.position = updated_position;
            }
            else
            {
                current_pad = target_pad;
                current_position = target_position;
                current_state = FROG_STATE.RESTING;
                position_progress = 0.0f;
            }
        }
    }

    void OnDown()
    {
        //Debug.Log("Down!");

        if(current_pad < MAX_PAD)
        {
            target_pad = current_pad + 1;
            target_position = pads[target_pad].position;
            current_state = FROG_STATE.TRANSITIONING;
            frog.transform.LookAt(target_position);
        }
    }

    void OnUp()
    {
        //Debug.Log("Up!");
        
        if(current_pad > 0)
        {
            target_pad = current_pad - 1;
            target_position = pads[target_pad].position;
            current_state = FROG_STATE.TRANSITIONING;
            frog.transform.LookAt(target_position);
        }
    }

    void OnEnter()
    {
        switch(current_pad)
        {
            case 3: Application.Quit(); Debug.Log("Quit ignored inside editor..."); break;
            default: Debug.Log("No action assigned to this pad..."); break;
        }
    }
}
