using System.Collections;
using UnityEngine;

public class BoomExplosion : MonoBehaviour
{

    [SerializeField] private ParticleSystem[] particleSystems;
    [SerializeField] private Collider explosionCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        PlayParticles();

        yield return new WaitForSeconds(0.1f); // Wait for 1 second before disabling the collider
        explosionCollider.enabled = false;
    }



    public void PlayParticles()
    {
        if (particleSystems == null || particleSystems.Length == 0)
        {
            Debug.LogWarning("No particle systems assigned to BoomExplosion.");
            return;
        }
        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                ps.Play();
            }
            else
            {
                Debug.LogWarning("A particle system in the array is null.");
            }
        }
    }
}
