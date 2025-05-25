using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] private bool destroyOnHit = true;
    [SerializeField] private GameObject indicator;
    [SerializeField] private LayerMask stickLayers; // Selectable in inspector

    [SerializeField] private bool withKnockback = false;

    private bool isOnFloor = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isOnFloor)
        {
            PlayerCombat playerCombat = other.GetComponent<PlayerCombat>();
            if (playerCombat != null)
            {
                playerCombat.ApplyKickback();
                playerCombat.PickUpBullet();
                Destroy(gameObject, 0.1f);
            }
            return;
        }

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            PlayerCombat playerCombat = FindAnyObjectByType<PlayerCombat>();
            playerCombat.ApplyKickback();
            playerCombat.PickUpBullet();

            enemy.DealDamage();
            if (withKnockback)
            {
                enemy.GetKnockback();
            }

            if (destroyOnHit)
            {
                Destroy(gameObject, 0.1f);
            }
        }
        else if (IsInStickLayer(other.gameObject))
        {
            StickToSurface(other.transform);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isOnFloor)
        {
            PlayerCombat playerCombat = collision.gameObject.GetComponent<PlayerCombat>();
            if (playerCombat != null)
            {
                playerCombat.ApplyKickback();
                playerCombat.PickUpBullet();
                Destroy(gameObject, 0.1f);
            }
            return;
        }
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            PlayerCombat playerCombat = FindAnyObjectByType<PlayerCombat>();
            playerCombat.ApplyKickback();
            playerCombat.PickUpBullet();

            enemy.DealDamage();
            if (withKnockback)
            {
                enemy.GetKnockback();
            }

            if (destroyOnHit)
            {
                Destroy(gameObject, 0.1f);
            }
        }
        else if (IsInStickLayer(collision.gameObject))
        {
            StickToSurface(collision.transform);
        }
    }

    private bool IsInStickLayer(GameObject obj)
    {
        return (stickLayers.value & (1 << obj.layer)) != 0;
    }

    private void StickToSurface(Transform other)
    {
        //GetComponent<DamageEnemy>().enabled = false; // Disable damage script to prevent further damage 
        isOnFloor = true;
        indicator.SetActive(true);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        //transform.SetParent(other);
        Debug.Log("Bullet stuck to: " + other.name);
    }
}
