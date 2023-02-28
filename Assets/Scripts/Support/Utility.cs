using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    private static Camera _camera;
    public static Camera Camera
    {
        get
        {
            if (_camera == null) _camera = Camera.main;
            return _camera;
        }
    }

    private static readonly Dictionary<float, WaitForSeconds> WaitForSecondsMap = new();

    public static WaitForSeconds GetWait(float time)
    {
        if (WaitForSecondsMap.TryGetValue(time, out var wait)) return wait;

        wait = new WaitForSeconds(time);
        WaitForSecondsMap[time] = wait;
        return wait;
    }

    public static Vector2 GetWorldPositionOfCanvasElement(RectTransform element)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, Camera, out var result);
        return result;
    }

    public static void DeleteChildren(this Transform t)
    {
        foreach (Transform child in t) Object.Destroy(child.gameObject);
    }
    
    public static Coroutine StartRoutine(IEnumerator routine) => CoroutineManager.StartRoutine(routine);
    
    public static void StopRoutine(Coroutine routine) => CoroutineManager.StopRoutine(routine);
}
