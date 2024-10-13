using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAttack : MonoBehaviour
{
    public GameObject fireBeamVFXPrefab;
    public float towerRadius = 8f;
    public float baseDamage = 20f;
    public float fireDamageOverTime = 100f;
    public int fireDOTTicks = 10;

    public MonsterDisplayHandler monsterDisplayHandler;

    public float switchCooldown = 1f;
    public LineRenderer beamLineRenderer;

    private Transform currentTarget = null;
    private GameObject fireBeamVFXInstance = null;
    private bool canSwitchTarget = true;

    void Update()
    {
        if (currentTarget == null || !IsTargetInRadius(currentTarget))
        {
            if (fireBeamVFXInstance != null)
            {
                Destroy(fireBeamVFXInstance);
                fireBeamVFXInstance = null;
                beamLineRenderer.enabled = false; // Disable beam when there's no target
            }

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
            SpawnVFX();
        }
    }

    bool IsTargetInRadius(Transform target)
    {
        return Vector3.Distance(transform.position, target.position) <= towerRadius;
    }

    void SpawnVFX()
    {
        if (fireBeamVFXInstance != null)
        {
            Destroy(fireBeamVFXInstance);
        }

        fireBeamVFXInstance = Instantiate(fireBeamVFXPrefab, transform.position, Quaternion.identity);
        fireBeamVFXInstance.transform.SetParent(transform);

        beamLineRenderer.enabled = true; // Enable the LineRenderer when firing
    }

    void FireAtTarget()
    {
        if (currentTarget == null || fireBeamVFXInstance == null) return;

        Collider targetCollider = currentTarget.GetComponent<Collider>();
        Vector3 targetPosition = currentTarget.position;

        if (targetCollider != null)
        {
            targetPosition = targetCollider.bounds.center;
        }

        fireBeamVFXInstance.transform.LookAt(targetPosition);

        beamLineRenderer.SetPosition(0, transform.position);
        beamLineRenderer.SetPosition(1, targetPosition); // Update the second point to the monster’s current position

        Monster monster = currentTarget.GetComponent<Monster>();
        monster.AdjustHealth(-baseDamage * Time.deltaTime);
        monsterDisplayHandler.UpdateHealth(monster);

        if (monster.GetHealth() <= 0)
        {
            monster.ApplyDOT(fireDamageOverTime, fireDOTTicks);
            StartCoroutine(SwitchTargetAfterCooldown());
        }
    }

    IEnumerator SwitchTargetAfterCooldown()
    {
        canSwitchTarget = false;
        currentTarget = null;

        if (fireBeamVFXInstance != null)
        {
            Destroy(fireBeamVFXInstance);
            fireBeamVFXInstance = null;
            beamLineRenderer.enabled = false; // Disable the LineRenderer when no target
        }

        yield return new WaitForSeconds(switchCooldown);

        canSwitchTarget = true;
        DetectAndLockOntoMonster();
    }
}
