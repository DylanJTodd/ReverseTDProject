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

    private float maxSpeed;
    private Coroutine latestSlowCoroutine;
    private Coroutine latestDOTCoroutine;
    private float slowEndTime = 0f;
    protected bool cantTarget = false;

    protected virtual void Awake()
    {
        maxSpeed = movementSpeed;
        mainCamera = Camera.main;
        
        // Create health bar
        if (healthBarPrefab != null)
        {
            GameObject healthBarObj = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBarObj.GetComponent<HealthBar>().SetType("monster");
            healthBarObj.transform.SetParent(transform);
            healthBarTransform = healthBarObj.transform;
            healthBar = healthBarObj.GetComponent<HealthBar>();
            
            // Set initial health
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(health);
            healthBar.SetType("Monster");
        }
    }

    private void Start()
    {
        MonsterManager.instance.RegisterMonster(this);
    }

    private void Update()
    {
        if (healthBarTransform != null)
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
    public void UpgradeStrength(int tier)
    {
        switch (tier)
        {
            case 1:
                damage = (int)(damage * 1.2f);
                break;
            case 2:
                damage = (int)(damage * 1.5f);
                break;
            case 3:
                damage = (int)(damage * 2f);
                break;
        }
    }

    public void UpgradeSpeed(int tier)
    {
        switch (tier)
        {
            case 1:
                movementSpeed *= 1.2f;
                maxSpeed *= 1.2f;
                break;
            case 2:
                movementSpeed *= 1.5f;
                maxSpeed *= 1.5f;
                break;
            case 3:
                movementSpeed *= 2f;
                maxSpeed *= 2f;
                break;
        }
    }

    public void UpgradeHealth(int tier)
    {
        float multiplier = 1f;
        switch (tier)
        {
            case 1:
                multiplier = 1.2f;
                break;
            case 2:
                multiplier = 1.5f;
                break;
            case 3:
                multiplier = 2f;
                break;
        }
        
        maxHealth = (int)(maxHealth * multiplier);
        health = (int)(health * multiplier);
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(health);
        }
    }

    // Modify the existing abstract Upgrade method to handle different upgrade types
    public void Upgrade(string type, int tier)
    {
        switch (type.ToLower())
        {
            case "strength":
                UpgradeStrength(tier);
                break;
            case "speed":
                UpgradeSpeed(tier);
                break;
            case "health":
                UpgradeHealth(tier);
                break;
        }
    }
}
