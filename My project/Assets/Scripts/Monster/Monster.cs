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
    public float maxSpeed;
    
    [Header("Combat Stats")]
    public float attackRange = 1;
    public float attackSpeed = 1;

    [Header("UI Elements")]
    public GameObject healthBarPrefab; // Assign this in the inspector
    private Transform healthBarTransform;
    private Camera mainCamera;
    private HealthBar healthBar;


    private Coroutine latestSlowCoroutine;
    private Coroutine latestDOTCoroutine;
    private float slowEndTime = 0f;
    protected bool cantTarget = false;

    protected virtual void Awake()
    {
        Debug.Log($"Monster Awake: {gameObject.name}");
        maxSpeed = movementSpeed;
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }

        // Only try to load from Resources if not already assigned in inspector
        if (healthBarPrefab == null)
        {
            healthBarPrefab = Resources.Load<GameObject>("UI/HealthBar");
            if (healthBarPrefab == null)
            {
                Debug.LogWarning("Health bar prefab not found in Resources folder");
            }
        }
        else
        {
            Debug.Log($"Using pre-assigned healthbar prefab on {gameObject.name}");
        }

        InitializeHealthBar();
    }

    private void InitializeHealthBar()
    {
        Debug.Log("Initializing health bar");
        if (healthBarPrefab != null && mainCamera != null)
        {
            // Create the health bar at the monster's position
            GameObject healthBarObj = Instantiate(healthBarPrefab);
            if (healthBarObj != null)
            {
                // Parent first, then position
                healthBarObj.transform.SetParent(transform);
                healthBarObj.transform.localPosition = Vector3.up * 2f;
                
                // Get and setup Canvas
                Canvas canvas = healthBarObj.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.WorldSpace;
                    canvas.worldCamera = mainCamera;
                    canvas.sortingOrder = 100; // Make sure it renders on top
                    
                    // Reset the Canvas's transform
                    canvas.transform.localPosition = Vector3.zero;
                    canvas.transform.localRotation = Quaternion.identity;
                    canvas.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    
                    Debug.Log($"Canvas setup complete - Camera: {canvas.worldCamera}, RenderMode: {canvas.renderMode}");
                }
                
                // Get and setup HealthBar component
                HealthBar healthBarComponent = healthBarObj.GetComponentInChildren<HealthBar>();
                if (healthBarComponent != null)
                {
                    healthBarComponent.SetType("monster");
                    healthBar = healthBarComponent;
                    healthBarTransform = healthBarObj.transform;
                    
                    healthBar.SetMaxHealth(maxHealth);
                    healthBar.SetHealth(health);
                }
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
            Vector3 position = transform.position + Vector3.up * 2f;
            healthBarTransform.position = position;
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
    public abstract void Upgrade(int tier);
    public abstract MonsterType GetMonsterType();
}
