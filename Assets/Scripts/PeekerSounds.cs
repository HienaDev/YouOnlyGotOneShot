using UnityEngine;

public class PeekerSounds : MonoBehaviour
{

    [SerializeField] private AudioClip[] peekSounds;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayPeekSound()
    {
        if (peekSounds.Length > 0)
        {
            AudioClip clip = peekSounds[Random.Range(0, peekSounds.Length)];
            AudioManager.Instance.PlayOneShot(clip, transform.position, 1f, true);
        }
    }
}
