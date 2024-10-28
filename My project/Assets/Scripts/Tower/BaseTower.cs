using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
public abstract class BaseTower : MonoBehaviour
{
    [Header("Tower Stats")]
    public int health = 100;
    public int maxHealth = 100;
    public int maxRange = 10;
    public float attackRange = 15f;
    public float attackRate = 1f;
    public int splashRadius = 0;

    [Header("Health Bar")]
    public GameObject healthBarPrefab;
    private HealthBar healthBar;    

    protected virtual void Start()
    {
        InitializeHealthBar();
    }

    private void InitializeHealthBar()
    {
        if (healthBarPrefab != null)
        {
            GameObject healthBarInstance = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2, Quaternion.identity, transform);
            healthBar = healthBarInstance.GetComponent<HealthBar>();
            healthBar.SetMaxHealth(maxHealth);
        } else {
            Debug.LogError("Health bar prefab is not assigned");
        }
    }

    protected abstract void Attack();

    // Method to take damage
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
        healthBar.SetHealth(health);
    }

    // Method to handle tower destruction
    protected virtual void Die()
    {
        // Handle tower destruction effects here (e.g., play animation, notify manager)
        Destroy(gameObject);
    }

    private void Update()
    {
        Attack();
    }

    public Transform FindTarget()
    {
        Collider[] monstersInRange = Physics.OverlapSphere(transform.position, maxRange);
        List<Monster> validMonsters = new List<Monster>();

        foreach (var collider in monstersInRange)
        {
            if (collider.CompareTag("Monster"))
            {
                Monster monster = collider.GetComponent<Monster>();
                validMonsters.Add(monster);
            }
        }

        Monster bestTarget = null;
        int maxGroupSize = 0;
        float groupRadius = splashRadius > 0 ? splashRadius : 5f;

        foreach (var monster in validMonsters)
        {
            int count = 0;
            Vector3 position = monster.transform.position;

            foreach (var other in validMonsters)
            {
                if (Vector3.Distance(position, other.transform.position) <= groupRadius)
                {
                    count++;
                }
            }

            if (count > maxGroupSize)
            {
                maxGroupSize = count;
                bestTarget = monster;
            }
        }

        return bestTarget?.transform;
    }

}
