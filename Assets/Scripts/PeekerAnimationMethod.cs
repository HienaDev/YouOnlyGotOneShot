using UnityEngine;

public class PeekerAnimationMethod : MonoBehaviour
{
    private EnemyFollow enemyFollow;

    private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyFollow = GetComponent<EnemyFollow>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AttackMethod()
    {
        animator.SetTrigger("Attack");  
    }

    public void ToggleWalkAnimation(bool toggle) => animator.SetBool("Walk", toggle);
}
