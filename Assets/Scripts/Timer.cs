using UnityEngine;

public class Timer
{
    float _timerLength;
    float _timeRemaining;

    public Timer(float startValue)
    {
        // Creates a new timer and set it to the provided duration

        _timeRemaining = startValue;
        _timerLength = startValue;
    }

    public bool CheckTimer()
    {
        // Checks if timer has exceeded the provided value
        // When false, reduces the timer
        // When true, resets the timer

        if (_timeRemaining > 0)
        {
            _timeRemaining -= Time.deltaTime;
            return false;
        }
        _timeRemaining = 0f;
        return true;
    }

    public bool CheckContinuousTimer()
    {
        if (_timeRemaining > 0)
        {
            _timeRemaining -= Time.deltaTime;
            return false;
        }
        else
        {
            _timeRemaining = _timerLength;
            return true;
        }
    }

    public void SetTimer(float newValue)
    {
        // Sets the timer without the need for a new timer object

        _timeRemaining = newValue;
    }
}
