using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float pitchVariation = 0.1f;
    [SerializeField] private AudioMixerGroup mixerGroup; // Optional for routing

    private Dictionary<string, AudioSource> loopedSources = new();

    private void Awake()
    {


        Instance = this;

    }

    /// <summary>
    /// Plays a one-shot audio clip.
    /// </summary>
    public void PlayOneShot(AudioClip clip, Vector3? position = null, float volume = 1f, bool is3D = false)
    {
        GameObject tempGO = new GameObject("TempOneShot");
        tempGO.transform.position = position ?? Vector3.zero;

        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = is3D ? 1f : 0f;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.outputAudioMixerGroup = mixerGroup;
        source.Play();

        Destroy(tempGO, clip.length / source.pitch);
    }

    /// <summary>
    /// Plays or starts looping an audio clip with a key identifier.
    /// </summary>
    public void PlayLoop(string key, AudioClip clip, Transform parent = null, float volume = 1f, bool is3D = false)
    {
        if (loopedSources.ContainsKey(key)) return;

        GameObject go = new GameObject($"Loop_{key}");
        if (parent != null)
        {
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
        }

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.volume = volume;
        source.spatialBlend = is3D ? 1f : 0f;
        source.playOnAwake = false;
        source.outputAudioMixerGroup = mixerGroup;
        source.Play();

        loopedSources.Add(key, source);
    }

    /// <summary>
    /// Stops and removes a looping audio by key.
    /// </summary>
    public void StopLoop(string key)
    {
        if (loopedSources.TryGetValue(key, out var source))
        {
            Destroy(source.gameObject);
            loopedSources.Remove(key);
        }
    }

    /// <summary>
    /// Adjusts volume of a loop by key.
    /// </summary>
    public void SetLoopVolume(string key, float volume)
    {
        if (loopedSources.TryGetValue(key, out var source))
        {
            source.volume = volume;
        }
    }

    /// <summary>
    /// Returns true if a loop with the given key is playing.
    /// </summary>
    public bool IsLoopPlaying(string key) => loopedSources.ContainsKey(key);
}
