using UnityEngine;

public class GnomeSounds : MonoBehaviour
{

    [SerializeField] private AudioClip gnomeDie;
    [SerializeField] private float gnomeDieSoundVolume = 1f; // Volume for the shoot sound
    [SerializeField] private AudioClip gnomeJump;
    [SerializeField] private float gnomeJumpSoundVolume = 1f; // Volume for the reload sound
    [SerializeField] private AudioClip gnomeExplosion;
    [SerializeField] private float gnomeExplosionSoundVolume = 1f; // Volume for the reload sound

    public void PlayDieSound()
    {
        AudioManager.Instance.PlayOneShot(gnomeDie, transform.position, gnomeDieSoundVolume, true);
    }

    public void PlayJumpSound()
    {
        AudioManager.Instance.PlayOneShot(gnomeJump, transform.position, gnomeJumpSoundVolume, true);
    }

    public void PlayExplosionSound()
    {
        AudioManager.Instance.PlayOneShot(gnomeExplosion, transform.position, gnomeExplosionSoundVolume, true);
    }
}
