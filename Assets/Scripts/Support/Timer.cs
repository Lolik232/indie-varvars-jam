using System;
using System.Collections;
using UnityEngine;

public class Timer : ITrigger
{
    private Coroutine _routine;
    public bool Value { get; private set; }
    public float Time { get; }

    public Timer(float time)
    {
        Value = false;
        Time = time;
    }

    public void Set()
    {
        Value = true;
        if (_routine != null)
        {
            CoroutineManager.StopRoutine(_routine);
        }
        
        _routine = CoroutineManager.StartRoutine(ResetValueOnTimeOut());
        
        SetEvent?.Invoke();
    }

    public void Reset()
    {
        Value = false;
        if (_routine == null) return;
        
        CoroutineManager.StopRoutine(_routine);
        _routine = null;
        
        ResetEvent?.Invoke();
    }

    public event Action SetEvent;
    public event Action ResetEvent;

    private IEnumerator ResetValueOnTimeOut()
    {
        yield return Utility.GetWait(Time);
        
        Reset();
    }

    public static implicit operator bool(Timer timer)
    {
        return timer.Value;
    }
}