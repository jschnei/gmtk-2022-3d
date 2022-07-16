using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    float timeRemaining;
    bool is_infinite;
    public Timer(float duration, bool infinite=false) {
        timeRemaining = duration;
        is_infinite = infinite;
    }
    public void UpdateTimer(float deltaTime)
    {
        if (timeRemaining > 0 && !is_infinite)
        {
            timeRemaining -= deltaTime;
        }
    }

    public bool IsOver()
    {
        return timeRemaining <= 0;
    }

    public static Timer InfiniteTimer() 
    {
        return new Timer(1.0f, true);
    }
}