using System.Collections;
using UnityEngine;

[System.Serializable]
public class Monster : MonoBehaviour
{
    public new string name;
    public GameObject obj;
    public int cost;
    public int health;
    public int damage;
    public float heightAdjust;
    public float movementSpeed;
    public float currentHealth;
    public bool cantTarget;

    public AudioClip movementSound;
    public UpgradeButtonHandler upgradeButtonHandler;

    private float maxSpeed;
    private Coroutine latestSlowCoroutine;
    private Coroutine latestDOTCoroutine;
    private float slowEndTime = 0f;
    private float currentSlowPercent = 0f;

    public void Awake()
    {
        currentHealth = health;
        maxSpeed = movementSpeed;

        if (upgradeButtonHandler == null)
        {
            upgradeButtonHandler = FindObjectOfType<UpgradeButtonHandler>();
        }

        UpgradeTierStrength(upgradeButtonHandler.strengthUpgrades);
        UpgradeTierSpeed(upgradeButtonHandler.speedUpgrades);
        UpgradeTierHealth(upgradeButtonHandler.healthUpgrades);
    }

    public void UpgradeTierStrength(int tier)
    {
        if (tier == 1)
        {
            damage = (int)(damage * 1.2f);
        }
        if (tier == 2)
        {
            damage = (int)(damage * 1.5f);
        }
        if (tier == 3)
        {
            damage = (int)(damage * 2f);
        }
        return;
    }

    public void UpgradeTierSpeed(int tier)
    {
        if (tier == 1)
        {
            movementSpeed = movementSpeed * 1.1f;
            maxSpeed = maxSpeed * 1.1f;
        }
        if (tier == 2)
        {
            movementSpeed = movementSpeed * 1.25f;
            maxSpeed = maxSpeed * 1.25f;
        }
        if (tier == 3)
        {
            movementSpeed = movementSpeed * 1.5f;
            maxSpeed = maxSpeed * 1.5f;
        }
        return;
    }

    public void UpgradeTierHealth(int tier)
    {
        if (tier == 1)
        {
            health = (int)(health * 1.2f);
            currentHealth = currentHealth * 1.2f;
        }
        if (tier == 2)
        {
            health = (int)(health * 1.5f);
            currentHealth = currentHealth * 1.5f;
        }
        if (tier == 3)
        {
            health = (int)(health * 2f);
            currentHealth = currentHealth * 2f;
        }
        return;
    }

    public void AdjustHealth(float amount)
    {
        currentHealth += amount;

        if (currentHealth <= 0)
        {
            UnityEngine.Object.Destroy(obj);
            obj = null;
        }
    }

    public void AdjustSpeed(float percent, int slowTime)
    {
        float properPercent = percent / 100f;

        if (properPercent > currentSlowPercent)
        {
            currentSlowPercent = Mathf.Min(properPercent, 0.9f);
            movementSpeed = maxSpeed * (1 - currentSlowPercent);
            slowEndTime = Time.time + slowTime;

            if (latestSlowCoroutine != null)
            {
                StopCoroutine(latestSlowCoroutine);
            }
            latestSlowCoroutine = StartCoroutine(RestoreSpeed(slowTime));
        }
    }

    private IEnumerator RestoreSpeed(int slowTime)
    {
        while (Time.time < slowEndTime)
        {
            yield return null;
        }

        movementSpeed = maxSpeed;
        currentSlowPercent = 0f;
        latestSlowCoroutine = null;
    }

    public void ApplyDOT(float amount, int ticks)
    {
        if (latestDOTCoroutine != null)
        {
            StopCoroutine(latestDOTCoroutine);
        }
        latestDOTCoroutine = StartCoroutine(ApplyDOTCoroutine(amount, ticks));
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

    public float GetHealth()
    {
        return currentHealth;
    }

    public float GetSpeed()
    {
        return movementSpeed;
    }

    public bool CantTarget()
    {
        return cantTarget;
    }
}