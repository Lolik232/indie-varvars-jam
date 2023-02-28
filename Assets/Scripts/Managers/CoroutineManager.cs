using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : Singleton<CoroutineManager>
{
    public static Coroutine StartRoutine(IEnumerator routine) => Instance.StartCoroutine(routine);

    public static void StopRoutine(Coroutine routine) => Instance.StopCoroutine(routine);
}