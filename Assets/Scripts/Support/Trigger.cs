using System;
using System.Collections;
using UnityEngine;

public class Trigger : ITrigger
{
    private Coroutine _routine;
    public bool Value { get; private set; }

    public Trigger()
    {
        Value = false;
    }

    public void Set()
    {
        Value = true;
        if (_routine != null)
        {
            CoroutineManager.StopRoutine(_routine);
        }

        _routine = CoroutineManager.StartRoutine(ResetValueNextFrame());

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

    private IEnumerator ResetValueNextFrame()
    {
        yield return null;

        Reset();
    }

    public static implicit operator bool(Trigger trigger)
    {
        return trigger.Value;
    }
}