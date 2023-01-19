using UnityEngine;

public class Timer
{
    private readonly float _timerLength;
    private float _timeRemaining;

    public Timer(float startValue=0f)
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

    public float TimeRemaining()
    {
        return _timeRemaining;
    }
}
