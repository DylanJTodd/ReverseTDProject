using UnityEngine;

public class EarthTower : BaseTower
{
    [Header("Earth Tower Specific")]
    public ParticleSystem attackVFX;
    public float aoeRadius = 2f;  // Radius around the target monster for the AOE attack

    protected override void PerformAttack(Transform target)
    {
        if (target == null) return;

        // Apply AOE damage around the target
        Collider[] monstersInAOE = Physics.OverlapSphere(target.position, aoeRadius);
        foreach (var collider in monstersInAOE)
        {
            if (collider.CompareTag("Monster"))
            {
                Monster monster = collider.GetComponent<Monster>();
                if (monster != null && !monster.CantTarget())
                {
                    monster.AdjustHealth(-attackDamage);
                }
            }
        }

        // Play VFX
        if (attackVFX != null)
        {
            RotateVFXToTarget(target);
            attackVFX.Play();
        }
    }

    private void RotateVFXToTarget(Transform target)
    {
        Vector3 directionToMonster = (target.position - transform.position).normalized;
        directionToMonster.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(directionToMonster);
        attackVFX.transform.rotation = targetRotation;
    }

    // Reset VFX rotation after attack
    private void ResetVFX()
    {
        if (attackVFX != null)
        {
            attackVFX.transform.rotation = Quaternion.identity;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxRange);

        // Draw AOE radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}
