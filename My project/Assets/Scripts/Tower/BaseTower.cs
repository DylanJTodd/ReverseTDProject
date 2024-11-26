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
    public float attackDamage = 10f;
    
    [Header("References")]
    public MonsterDisplayHandler monsterDisplayHandler;
    protected float lastAttackTime;
    protected bool canAttack = true;

    [Header("UI Elements")]
    public GameObject healthBarPrefab; // Assign this in the inspector
    private Transform healthBarTransform;
    private Camera mainCamera;
    private HealthBar healthBar;   

    protected virtual void Start()
    {
        mainCamera = Camera.main;
        InitializeHealthBar();
    }

    private void InitializeHealthBar()
    {
        // Try to find the health bar prefab if not assigned
        if (healthBarPrefab == null)
        {
            healthBarPrefab = Resources.Load<GameObject>("UI/HealthBar");
            if (healthBarPrefab == null)
            {
                Debug.LogWarning("Health bar prefab not found in Resources folder");
            }
        }

        if (healthBarPrefab != null)
        {
            GameObject healthBarInstance = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBarInstance.transform.SetParent(transform);
            healthBarTransform = healthBarInstance.transform;
            healthBar = healthBarInstance.GetComponent<HealthBar>();
            
            // Set initial health
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(health);
            healthBar.SetType("Tower");
        } else {
            Debug.LogError("Health bar prefab is not assigned");
        }
    }

    private void Update()
    {
        // Update health bar position and rotation
        if (healthBarTransform != null)
        {
            healthBarTransform.position = transform.position + Vector3.up * 2f;
            healthBarTransform.rotation = mainCamera.transform.rotation;
        }

        if (canAttack && Time.time >= lastAttackTime + attackRate)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    protected virtual void Attack()
    {
        // Base implementation for finding targets
        Transform target = FindTarget();
        if (target != null)
        {
            PerformAttack(target);
        }
    }

    protected abstract void PerformAttack(Transform target);

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
