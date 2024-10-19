using UnityEngine;

public abstract class BaseTower : MonoBehaviour
{
    [Header("Tower Stats")]
    public int health = 100;

    [Header("Health Bar")]
    public GameObject healthBarPrefab;
    private HealthBar healthBar;
    
    protected abstract void UseAbility();

    // Method to take damage
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    // Method to handle tower destruction
    protected virtual void Die()
    {
        // Handle tower destruction effects here (e.g., play animation, notify manager)
        Destroy(gameObject);
    }

    private void Update()
    {
        UseAbility();
    }
}
