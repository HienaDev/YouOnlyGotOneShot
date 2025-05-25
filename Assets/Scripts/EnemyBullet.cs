using UnityEngine;

public class EnemyBullet : MonoBehaviour
{

    [SerializeField] private bool destroy = true;

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("hit" + other.name);

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1); // Assuming the bullet does 1 damage
        }

        if (destroy)
        {
            Destroy(gameObject);
        }


    }
}
