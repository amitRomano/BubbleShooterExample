using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileInput : MonoSingleton<MobileInput>
{

    public bool tap, release, hold;
    public Vector3 swipeDelta;
    private Vector3 initialPos;

    private void Update()
    {
        release = tap = false;
        swipeDelta = Vector3.zero;
        if (Input.GetMouseButtonDown(0))
        {
            initialPos = Input.mousePosition;
            hold = tap = true;
        }

        else if (Input.GetMouseButtonUp(0))
        {
            release = true;
            hold = false;
            swipeDelta = Input.mousePosition - initialPos;
        }

        if (hold)
        {
            swipeDelta = Input.mousePosition - initialPos;
        }
    }
}