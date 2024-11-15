using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public abstract class Monster : MonoBehaviour
{
    [Header("Base Stats")]
    public int health = 100;
    public int maxHealth = 100;
    public int damage = 10;
    public int level = 1;
    public int cost = 10;
    public float heightAdjust = 0;
    public float movementSpeed = 1;
    
    [Header("Combat Stats")]
    public float attackRange = 1;
    public float attackSpeed = 1;

    [Header("UI Elements")]
    public GameObject healthBarPrefab; // Assign this in the inspector
    private Transform healthBarTransform;
    private Camera mainCamera;
    private HealthBar healthBar;

    public float maxSpeed;
    private Coroutine latestSlowCoroutine;
    private Coroutine latestDOTCoroutine;
    private float slowEndTime = 0f;
    protected bool cantTarget = false;

    protected virtual void Awake()
    {
        Debug.Log($"Monster Awake: {gameObject.name}");
        maxSpeed = movementSpeed;
        mainCamera = Camera.main;
        
        // Try to find the health bar prefab if not assigned
        if (healthBarPrefab == null)
        {
            healthBarPrefab = Resources.Load<GameObject>("Monsters/HealthBar");
            if (healthBarPrefab == null)
            {
                Debug.LogWarning("Health bar prefab not found in Resources folder");
            }
        }

        InitializeHealthBar();
    }

    private void InitializeHealthBar()
    {
        if (healthBarPrefab != null)
        {
            GameObject healthBarObj = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            if (healthBarObj != null)
            {
                HealthBar healthBarComponent = healthBarObj.GetComponent<HealthBar>();
                if (healthBarComponent != null)
                {
                    healthBarComponent.SetType("monster");
                    healthBarObj.transform.SetParent(transform);
                    healthBarTransform = healthBarObj.transform;
                    healthBar = healthBarComponent;
                    
                    // Set initial health
                    healthBar.SetMaxHealth(maxHealth);
                    healthBar.SetHealth(health);
                    healthBar.SetType("Monster");
                }
                else
                {
                    Debug.LogError("HealthBar component not found on prefab");
                }
            }
            else
            {
                Debug.LogError("Failed to instantiate health bar");
            }
        }
    }

    public virtual void Start()
    {
        Debug.Log($"Monster Start: {gameObject.name} - Health: {health}, Speed: {movementSpeed}");
        if (MonsterManager.instance != null)
        {
            MonsterManager.instance.RegisterMonster(this);
        }
        else
        {
            Debug.LogError("MonsterManager instance is null!");
        }
    }

    private void Update()
    {
        if (healthBarTransform != null && mainCamera != null)
        {
            // Make health bar face camera and position it above monster
            healthBarTransform.position = transform.position + Vector3.up * 2f;
            healthBarTransform.rotation = mainCamera.transform.rotation;
        }
    }

    public abstract void Attack();
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (healthBar != null)
        {
            healthBar.SetHealth(health);
        }
        
        if (health <= 0)
        {
            Die();
        }
    }

    public void AdjustHealth(float amount)
    {
        health += (int)amount;
        if (healthBar != null)
        {
            healthBar.SetHealth(health);
        }
        
        if (health <= 0)
        {
            Die();
        }
    }

    public void AdjustSpeed(float percent, int slowTime)
    {
        float properPercent = 1 - (percent / 100f);
        float slowedSpeed = maxSpeed * properPercent;

        if (slowedSpeed <= movementSpeed)
        {
            movementSpeed = slowedSpeed;
            slowEndTime = Time.time + slowTime;

            if (latestSlowCoroutine != null)
            {
                StopCoroutine(latestSlowCoroutine);
            }
            latestSlowCoroutine = StartCoroutine(RestoreSpeed(slowTime));
        }
    }

    public void ApplyDOT(float amount, int ticks)
    {
        if (latestDOTCoroutine != null)
        {
            StopCoroutine(latestDOTCoroutine);
        }
        latestDOTCoroutine = StartCoroutine(ApplyDOTCoroutine(amount, ticks));
    }

    private IEnumerator RestoreSpeed(int slowTime)
    {
        while (Time.time < slowEndTime)
        {
            yield return null;
        }
        movementSpeed = maxSpeed;
        latestSlowCoroutine = null;
    }

    private IEnumerator ApplyDOTCoroutine(float amount, int ticks)
    {
        for (int i = 0; i < ticks; i++)
        {
            AdjustHealth(-amount);
            yield return new WaitForSeconds(1f);
        }
        latestDOTCoroutine = null;
    }

    protected virtual void Die()
    {
        MonsterManager.instance.UnregisterMonster(this);
        Destroy(gameObject);
    }

    // Getter methods
    public float GetSpeed() => movementSpeed;
    public int GetDamage() => damage;
    public int GetHealth() => health;
    public int GetMaxHealth() => maxHealth;
    public bool CantTarget() => cantTarget;

    // Add these upgrade methods after the existing properties


    public abstract void Upgrade(int tier);

    public void UpgradeStrength(int tier)
    {
        damage = (int)(damage * (1 + (0.5f * tier)));
        Debug.Log($"Upgraded {gameObject.name} strength to tier {tier}. New damage: {damage}");
    }

    public void UpgradeSpeed(int tier)
    {
        float multiplier = 1 + (0.25f * tier);
        movementSpeed *= multiplier;
        maxSpeed *= multiplier;
        Debug.Log($"Upgraded {gameObject.name} speed to tier {tier}. New speed: {movementSpeed}");
    }

    public void UpgradeHealth(int tier)
    {
        float multiplier = 1 + (0.5f * tier);
        maxHealth = (int)(maxHealth * multiplier);
        health = (int)(health * multiplier);
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(health);
        }
        Debug.Log($"Upgraded {gameObject.name} health to tier {tier}. New health: {health}");
    }
}
