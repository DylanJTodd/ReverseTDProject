using UnityEngine;

public class Monster : MonoBehaviour
{
    public int health = 100;

    private void Start()
    {
        MonsterManager.instance.RegisterMonster(this);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle monster death (e.g., play animation, notify manager)
        MonsterManager.instance.UnregisterMonster(this);
        Destroy(gameObject);
    }

    // Additional monster behaviors can be implemented here
}
