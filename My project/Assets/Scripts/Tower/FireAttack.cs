using System.Collections;
using UnityEngine;

public class FireAttack : MonoBehaviour
{
    public float towerRadius = 8f;
    public float baseDamage = 10f;
    public float fireDamageOverTime = 5f;
    public int fireDOTTicks = 10;

    public MonsterDisplayHandler monsterDisplayHandler;

    public float switchCooldown = 2f;
    public LineRenderer beamLineRenderer;

    private Transform currentTarget = null;
    private bool canSwitchTarget = true;

    public int tier;
    public GameObject lightHolder;

    private void Start()
    {
        monsterDisplayHandler = FindObjectOfType<MonsterDisplayHandler>();

        if (tier == 1)
        {
            SetLightEmissionIntensity(1);
        }

        if (tier == 2)
        {
            baseDamage *= 2.5f;
            fireDamageOverTime *= 2f;
            fireDOTTicks = (int)(fireDOTTicks * 1.5);
            SetLightEmissionIntensity(2.5f);
        }

        if (tier == 3)
        {
            baseDamage *= 10f;
            fireDamageOverTime *= 10f;
            fireDOTTicks = (int)(fireDOTTicks * 3);
            switchCooldown *= 0.5f;
            SetLightEmissionIntensity(5);
        }
    }

    private void SetLightEmissionIntensity(float intensity)
    {
        foreach (Transform lightTransform in lightHolder.transform)
        {
            Light light = lightTransform.GetComponent<Light>();
            light.intensity = intensity;
        }
    }

    void Update()
    {
        if (currentTarget == null || !IsTargetInRadius(currentTarget))
        {
            beamLineRenderer.enabled = false; // Disable the beam when there's no target

            if (canSwitchTarget)
            {
                DetectAndLockOntoMonster();
            }
        }
        else
        {
            FireAtTarget();
        }
    }

    void DetectAndLockOntoMonster()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, towerRadius);
        Transform highestHealthMonster = null;
        float maxHealth = 0;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Monster"))
            {
                Monster monster = hitCollider.GetComponent<Monster>();
                if (monster.GetHealth() > maxHealth)
                {
                    maxHealth = monster.GetHealth();
                    highestHealthMonster = hitCollider.transform;
                }
            }
        }

        if (highestHealthMonster != null)
        {
            currentTarget = highestHealthMonster;
        }
    }

    bool IsTargetInRadius(Transform target)
    {
        return Vector3.Distance(transform.position, target.position) <= towerRadius;
    }

    void FireAtTarget()
    {
        if (currentTarget == null || currentTarget.gameObject == null)
        {
            DetectAndLockOntoMonster();
            return;
        }

        Collider targetCollider = currentTarget.GetComponent<Collider>();
        Vector3 targetPosition = currentTarget.position;

        if (targetCollider != null)
        {
            targetPosition = targetCollider.bounds.center;
        }

        beamLineRenderer.SetPosition(0, transform.position);
        beamLineRenderer.SetPosition(1, targetPosition);

        beamLineRenderer.enabled = true;

        Monster monster = currentTarget.GetComponent<Monster>();
        if (monster != null)
        {
            monster.AdjustHealth(-baseDamage * Time.deltaTime);
            monster.ApplyDOT(fireDamageOverTime, fireDOTTicks);
            monsterDisplayHandler.UpdateHealth(monster);

            if (monster.GetHealth() <= 0)
            {
                DetectAndLockOntoMonster();
                return;
            }
        }
    }

    IEnumerator SwitchTargetAfterCooldown()
    {
        canSwitchTarget = false;
        currentTarget = null;

        beamLineRenderer.enabled = false;

        yield return new WaitForSeconds(switchCooldown);

        canSwitchTarget = true;
        DetectAndLockOntoMonster();
    }
}
