using UnityEngine;

public class DamageEnemy : MonoBehaviour
{

    [SerializeField] private bool withKnockback = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with : " + other.gameObject.name);
        // Check if the bullet collided with an enemy
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Deal damage to the enemy
            enemy.DealDamage();
            if(withKnockback)
            {
                enemy.GetKnockback();
            }

        }
    }
}
