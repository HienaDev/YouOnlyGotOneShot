using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TriggerSoundPlayer : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private AudioClip[] soundClips;

    [Header("Pitch Randomization")]
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownTime = 0f; // seconds
    private float lastPlayTime = -Mathf.Infinity;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Vector3 contactPoint = other.ClosestPoint(transform.position);
        PlaySound(contactPoint);
    }

    // Public method to play sound at the object's position 
    public void PlaySound()
    {
        PlaySound(transform.position);
    }

    // Core method to handle playback with position
    private void PlaySound(Vector3 position)
    {
        if (Time.time - lastPlayTime < cooldownTime)
            return; // Skip if still on cooldown

        if (soundClips == null || soundClips.Length == 0)
        {
            Debug.LogWarning("TriggerSoundPlayer: No sound clips assigned.");
            return;
        }

        AudioClip randomClip = soundClips[Random.Range(0, soundClips.Length)];
        float randomPitch = Random.Range(minPitch, maxPitch);

        audioSource.pitch = randomPitch;
        AudioSource.PlayClipAtPoint(randomClip, position, audioSource.volume);

        lastPlayTime = Time.time;
    }
}
