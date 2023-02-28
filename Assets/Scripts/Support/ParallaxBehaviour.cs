using System;
using UnityEngine;


public class ParallaxBehaviour : MonoBehaviour
{
    private Vector3   _targetPreviousPosition;
    private Transform Target { get; set; }


    [Header("SETTINGS")]
    [SerializeField] private bool _yParallax = false;
    [SerializeField, Range(0f, 1f)] private float _xParallaxStrength = 0.1f;
    [SerializeField, Range(0f, 1f)] private float _yParallaxStrength = 1f;

    private void Start()
    {
        if (Camera.main == null) throw new NullReferenceException();
        Target                  = Camera.main.transform;
        _targetPreviousPosition = Target.position;
    }

    private void FixedUpdate()
    {
        var delta = Target.position - _targetPreviousPosition;

        if (_yParallax == false) delta.y = 0;


        var shift = new Vector3(delta.x * _xParallaxStrength,
                                delta.y * _yParallaxStrength);
        transform.position      += shift;
        _targetPreviousPosition =  Target.position;
    }
}