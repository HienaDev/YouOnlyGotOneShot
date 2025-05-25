using JigglePhysics;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{

    [SerializeField] private int health = 1;
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Collider[] colliders;

    [SerializeField] private ParticleSystem deathEffect;

    [SerializeField] private JiggleRigBuilder jiggleRigBuilder;
    [SerializeField] private JiggleSettings deadJiggle;
    private LeftRightMover leftRightMover;

    [SerializeField] private float timeToDie = 2f;

    [SerializeField] private UnityEvent onDeath;

    private bool dead = false;

    [SerializeField] private AudioClip[] deathSounds;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(jiggleRigBuilder == null)
            jiggleRigBuilder = GetComponent<JiggleRigBuilder>();
        leftRightMover = GetComponent<LeftRightMover>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DealDamage()
    {
        health--;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log(gameObject.name + " has died!");
        if (dead)
            return;

        if (deathSounds.Length > 0)
        {
            AudioManager.Instance.PlayOneShot(deathSounds[Random.Range(0, deathSounds.Length)], transform.position, 1f, true);
        }
            dead = true;
        if(jiggleRigBuilder != null)
        {
            jiggleRigBuilder.jiggleRigs[0].jiggleSettings = (deadJiggle);
        }
        else
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }

        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        ParticleSystem temp = Instantiate(deathEffect, transform.position, Quaternion.identity);
        temp.Play();

        if(leftRightMover != null)
        {
            leftRightMover.StopMoving();
        }
        onDeath.Invoke();

        Destroy(gameObject, timeToDie);


    }

    public void GetKnockback()
    {
        Knockback knockback = GetComponent<Knockback>();
        if (knockback != null)
        {
            knockback.GetKnockedBack();
        }
        
    }
}
