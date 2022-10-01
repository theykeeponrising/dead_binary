using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    float timer;

    public Timer(float startValue)
    {
        // Creates a new timer and set it to the provided duration

        timer = startValue;
    }

    public bool CheckTimer()
    {
        // Checks if timer has exceeded the provided value
        // When false, reduces the timer
        // When true, resets the timer

        if (timer > 0)
        {
            timer -= Time.deltaTime;
            return false;
        }
        timer = 0f;
        return true;
    }
}
