using Unity.VisualScripting;
using UnityEngine;

public class IceTower : BaseTower
{
    [Header("Ice Tower Settings")]
    public GameObject iceChunkPrefab;
    public new float attackRange = 15f;
    public new float attackRate = 1f;
    private float lastAttackTime;

    protected override void Start()
    {
        base.Start();
        // Register this tower with the TowerManager
        TowerManager.instance.RegisterTower(this);
    }

    protected override void Die()
    {
        base.Die();
        // Additional death logic if necessary (e.g., notify other systems)
    }

    protected override void Attack()
    {
        if (Time.time >= lastAttackTime + attackRate)
        {
            // Find the nearest monster within range
            Monster target = FindNearestMonster();
            if (target != null)
            {
                // Spawn ice chunk and throw towards the target
                SpawnIceChunk(target);
                lastAttackTime = Time.time;
            }
        }
    }

    private Monster FindNearestMonster()
    {
        Monster nearest = null;
        float minDistance = Mathf.Infinity;
        foreach (Monster monster in MonsterManager.instance.monsters)
        {
            float distance = Vector3.Distance(transform.position, monster.transform.position);
            if (distance < minDistance && distance <= attackRange)
            {
                minDistance = distance;
                nearest = monster;
            }
        }
        return nearest;
    }

    private void SpawnIceChunk(Monster target)
    {
        // Instantiate ice chunk at tower's position
        GameObject iceChunk = Instantiate(iceChunkPrefab, transform.position, Quaternion.identity);
        IceChunk iceChunkScript = iceChunk.GetComponent<IceChunk>();
        if (iceChunkScript != null)
        {
            iceChunkScript.ThrowTowards(target.transform);
        }
    }

    private void OnDestroy()
    {
        // Unregister this tower from the TowerManager
        TowerManager.instance.UnregisterTower(this);
    }
}
