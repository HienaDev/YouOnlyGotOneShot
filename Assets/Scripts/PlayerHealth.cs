using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    [SerializeField] private int lives = 1;
    [SerializeField] private GameObject deathScreen;

    private Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindAnyObjectByType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        lives -= damage;
        if (lives <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        deathScreen.SetActive(true);
        player.enabled = false;
    }
}
