using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    private static ObjectPool<AudioSource> _pool;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _pool = new ObjectPool<AudioSource>(
            () => Instantiate(_audioSource),
            source => { source.gameObject.SetActive(true); },
            source => { source.gameObject.SetActive(false); },
            source => { Destroy(source.gameObject); },
            false, 5, 15
        );
    }

    public static IEnumerator PlaySound(AudioClip clip)
    {
        var source = _pool.Get();
        source.clip = clip;
        source.Play();
        yield return Utility.GetWait(clip.length);
        
        source.Stop();
        _pool.Release(source);
    }
}