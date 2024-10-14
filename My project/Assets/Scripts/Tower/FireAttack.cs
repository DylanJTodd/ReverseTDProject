using System.Collections;
using UnityEngine;

public class FireAttack : MonoBehaviour
{
    public float towerRadius = 8f;
    public float baseDamage = 50f;  //Per second
    public float fireDamageOverTime = 10f;
    public int fireDOTTicks = 10;

    public MonsterDisplayHandler monsterDisplayHandler;

    public float switchCooldown = 1f;
    public LineRenderer beamLineRenderer;

    private Transform currentTarget = null;
    private bool canSwitchTarget = true;

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
        if (currentTarget == null) return;


        Collider targetCollider = currentTarget.GetComponent<Collider>();
        Vector3 targetPosition = currentTarget.position;

        if (targetCollider != null)
        {
            targetPosition = targetCollider.bounds.center;
        }

        // Update LineRenderer to point to the target
        beamLineRenderer.SetPosition(0, transform.position);
        beamLineRenderer.SetPosition(1, targetPosition);

        beamLineRenderer.enabled = true;

        Monster monster = currentTarget.GetComponent<Monster>();
        monster.AdjustHealth(-baseDamage * Time.deltaTime);
        monster.ApplyDOT(fireDamageOverTime, fireDOTTicks);
        monsterDisplayHandler.UpdateHealth(monster);

        if (monster.GetHealth() <= 0)
        {
            StartCoroutine(SwitchTargetAfterCooldown());
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
