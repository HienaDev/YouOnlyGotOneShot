using JigglePhysics;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] private int health = 1;
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Collider[] colliders;

    [SerializeField] private ParticleSystem deathEffect;

    private JiggleRigBuilder jiggleRigBuilder;
    [SerializeField] private JiggleSettings deadJiggle;
    private LeftRightMover leftRightMover;

    [SerializeField] private float timeToDie = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

        Destroy(gameObject, timeToDie);


    }
}
