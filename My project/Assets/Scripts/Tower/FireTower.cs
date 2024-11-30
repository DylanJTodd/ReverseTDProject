using UnityEngine;

public class FireTower : BaseTower
{
    [Header("Fire Tower Settings")]
    public float fireDamageOverTime = 10f;
    public int fireDOTTicks = 10;
    public LineRenderer beamLineRenderer;
    private Transform currentTarget;

    protected override void PerformAttack(Transform target)
    {
        currentTarget = target;
        if (currentTarget == null) return;

        Collider targetCollider = currentTarget.GetComponent<Collider>();
        Vector3 targetPosition = targetCollider != null ? targetCollider.bounds.center : currentTarget.position;

        Monster monster = currentTarget.GetComponent<Monster>();
        if (monster != null)
        {
            monster.AdjustHealth(-attackDamage * Time.deltaTime);
            monster.ApplyDOT(fireDamageOverTime, fireDOTTicks);
        }
    }

    private void LateUpdate()
    {
        if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.position) > attackRange)
        {
            beamLineRenderer.enabled = false;
            currentTarget = null;
        }

        if (currentTarget != null)
        {
            beamLineRenderer.enabled = true;
            beamLineRenderer.SetPosition(0, transform.position);
            beamLineRenderer.SetPosition(1, currentTarget.position);
        }
    }
}
