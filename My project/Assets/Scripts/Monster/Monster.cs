using UnityEngine;
using System.Collections;

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

    private float maxSpeed;
    private Coroutine latestSlowCoroutine;
    private Coroutine latestDOTCoroutine;
    private float slowEndTime = 0f;
    protected bool cantTarget = false;

    protected virtual void Awake()
    {
        maxSpeed = movementSpeed;
    }

    private void Start()
    {
        MonsterManager.instance.RegisterMonster(this);
    }

    public abstract void Attack();
    public abstract void Upgrade(int tier);

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    public void AdjustHealth(float amount)
    {
        health += (int)amount;
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
}
