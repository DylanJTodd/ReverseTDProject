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
    public float heightAdjust; //Height value from origin to feet
    public float movementSpeed;
    public float currentHealth;


    private float maxSpeed;
    private Coroutine latestSlowCoroutine;
    private Coroutine latestDOTCoroutine;
    private float slowEndTime = 0f;

    public void Awake()
    {
        currentHealth = health;
        maxSpeed = movementSpeed;
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
            latestSlowCoroutine = StartCoroutine(RestoreSpeed(slowTime, maxSpeed));
        }
    }

    private IEnumerator RestoreSpeed(int slowTime, float previousSpeed)
    {
        while (Time.time < slowEndTime)
        {
            yield return null;
        }

        movementSpeed = previousSpeed;
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
}