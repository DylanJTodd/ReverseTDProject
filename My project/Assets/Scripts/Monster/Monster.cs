using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    public int health = 100;
    public int maxHealth = 100;
    public int damage = 10;
    public int level = 1;
    public int cost = 10;
    public float heightAdjust = 0;
    public float movementSpeed = 1;

    public float attackRange = 1;
    public float attackSpeed = 1;

    public abstract void Attack();

    public abstract void Upgrade(int tier);

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

    public float GetSpeed()
    {
        return movementSpeed;
    }

    public int GetDamage()
    {
        return damage;
    }

    public int GetHealth()
    {
        return health;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

}
