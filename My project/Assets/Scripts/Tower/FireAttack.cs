using System.Collections;
using UnityEngine;

public class FireAttack : TowerAttackBase
{
    public float fireDamageOverTime = 5f;
    public int fireDOTTicks = 10;

    public MonsterDisplayHandler monsterDisplayHandler;
    public LineRenderer beamLineRenderer;
    public GameObject lightHolder;

    private Transform currentTarget = null;
    private bool canSwitchTarget = true;

    private void Start()
    {
        monsterDisplayHandler = FindObjectOfType<MonsterDisplayHandler>();
        SetTier(tier);
    }

    protected override void UpdateStats()
    {
        switch (tier)
        {
            case 1:
                baseDamage = 10f;
                attackCooldown = 2f;
                fireDamageOverTime = 5f;
                fireDOTTicks = 10;
                towerRadius = 8f;
                SetLightEmissionIntensity(1);
                break;
            case 2:
                baseDamage *= 6f;
                fireDamageOverTime *= 2f;
                attackCooldown = 4f;
                fireDOTTicks = (int)(fireDOTTicks * 1.5);
                towerRadius = 10f;
                SetLightEmissionIntensity(2.5f);
                break;
            case 3:
                baseDamage *= 15f;
                attackCooldown = 7f;
                fireDamageOverTime = 50f;
                fireDOTTicks = 10;
                towerRadius = 15f;
                break;
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
            beamLineRenderer.enabled = false;

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
            StartCoroutine(SwitchTargetAfterCooldown());
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
                StartCoroutine(SwitchTargetAfterCooldown());
                return;
            }
        }
    }

    IEnumerator SwitchTargetAfterCooldown()
    {
        canSwitchTarget = false;
        currentTarget = null;
        beamLineRenderer.enabled = false;

        yield return new WaitForSeconds(attackCooldown);

        canSwitchTarget = true;
        DetectAndLockOntoMonster();
    }
}