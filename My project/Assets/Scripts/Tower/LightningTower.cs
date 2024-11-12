using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LightningTower : BaseTower
{
    [Header("Lightning Tower Settings")]
    public int maxChain = 3;
    public int maxBranch = 3;
    public float chainRadius = 2f;
    public float chainDuration = 1.5f;
    public float attackCooldown = 1f;
    public int maxChainHits = 3;
    public int attackRadius = 10;
    public LineRenderer lineRenderer;
    private List<Monster> activeChainedMonsters;

    protected override void PerformAttack(Transform target)
    {
        Monster initialTarget = target.GetComponent<Monster>();
        if (initialTarget != null)
        {
            List<Monster> validMonsters = GetMonstersInRange();
            activeChainedMonsters = new List<Monster>();
            ChainToMonsters(initialTarget, validMonsters, activeChainedMonsters, 1);
            StartCoroutine(AnimateLightningChain(activeChainedMonsters));
        }
    }

    private List<Monster> GetMonstersInRange()
    {
        List<Monster> monsters = new List<Monster>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, attackRadius);
        foreach (Collider col in colliders)
        {
            Monster monster = col.GetComponent<Monster>();
            if (monster != null)
            {
                monsters.Add(monster);
            }
        }
        return monsters;
    }

    void DetectAndChainMonsters()
    {
        Collider[] monstersInRange = Physics.OverlapSphere(transform.position, attackRadius);
        List<Monster> validMonsters = new List<Monster>();

        foreach (var collider in monstersInRange)
        {
            if (collider.CompareTag("Monster"))
            {
                Monster monster = collider.GetComponent<Monster>();
                if (monster != null && monster.gameObject != null)
                {
                    validMonsters.Add(monster);
                }
            }
        }

        if (validMonsters.Count == 0) return;

        Monster initialTarget = FindBestInitialMonster(validMonsters);
        if (initialTarget != null)
        {
            activeChainedMonsters = new List<Monster>();
            ChainToMonsters(initialTarget, validMonsters, activeChainedMonsters, 1);
            StartCoroutine(AnimateLightningChain(activeChainedMonsters));
        }

        StartCoroutine(AttackCooldown());
    }

    IEnumerator ChainDurationCoroutine()
    {
        yield return new WaitForSeconds(chainDuration);
        lineRenderer.enabled = false;
        activeChainedMonsters = null;
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    Monster FindBestInitialMonster(List<Monster> validMonsters)
    {
        Monster bestMonster = null;
        int maxHitCount = 0;

        foreach (Monster monster in validMonsters)
        {
            if (monster == null || monster.gameObject == null) continue;

            List<Monster> tempChain = new List<Monster>();
            ChainToMonsters(monster, validMonsters, tempChain, 1);
            if (tempChain.Count > maxHitCount)
            {
                maxHitCount = tempChain.Count;
                bestMonster = monster;
            }
        }

        return bestMonster;
    }

    void ChainToMonsters(Monster currentMonster, List<Monster> allMonsters, List<Monster> chainedMonsters, int chainLevel)
    {
        if (currentMonster == null || currentMonster.gameObject == null) return;
        if (chainedMonsters.Contains(currentMonster) || chainLevel > maxChain) return;

        chainedMonsters.Add(currentMonster);

        if (chainLevel == 1)
        {
            List<Monster> branchTargets = FindClosestMonsters(currentMonster, allMonsters, chainedMonsters, maxBranch);
            foreach (var branchMonster in branchTargets)
            {
                ChainToMonsters(branchMonster, allMonsters, chainedMonsters, chainLevel + 1);
            }
        }
        else
        {
            Monster nextMonster = FindNextMonsterInRange(currentMonster, allMonsters, chainedMonsters);
            if (nextMonster != null)
            {
                ChainToMonsters(nextMonster, allMonsters, chainedMonsters, chainLevel + 1);
            }
        }
    }

    Monster FindNextMonsterInRange(Monster currentMonster, List<Monster> allMonsters, List<Monster> alreadyChained)
    {
        Monster closestMonster = null;
        float minDistance = chainRadius;

        foreach (var monster in allMonsters)
        {
            if (monster == null || monster.gameObject == null) continue;
            if (alreadyChained.Contains(monster)) continue;

            float distance = Vector3.Distance(currentMonster.transform.position, monster.transform.position);
            if (distance <= chainRadius && distance < minDistance)
            {
                closestMonster = monster;
                minDistance = distance;
            }
        }

        return closestMonster;
    }

    List<Monster> FindClosestMonsters(Monster currentMonster, List<Monster> allMonsters, List<Monster> alreadyChained, int maxCount)
    {
        List<Monster> closeMonsters = new List<Monster>();

        foreach (var monster in allMonsters)
        {
            if (monster == null || monster.gameObject == null) continue;
            if (alreadyChained.Contains(monster)) continue;

            float distance = Vector3.Distance(currentMonster.transform.position, monster.transform.position);
            if (distance <= chainRadius)
            {
                closeMonsters.Add(monster);
            }
        }

        closeMonsters.Sort((a, b) => Vector3.Distance(currentMonster.transform.position, a.transform.position)
                                     .CompareTo(Vector3.Distance(currentMonster.transform.position, b.transform.position)));

        return closeMonsters.GetRange(0, Mathf.Min(closeMonsters.Count, maxCount));
    }

    IEnumerator AnimateLightningChain(List<Monster> chainedMonsters)
    {
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = true;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, GetMonsterCenter(chainedMonsters[0]));
        yield return new WaitForSeconds((chainDuration / 2) - 0.02f);

        DealDamage(chainedMonsters[0], 1);

        for (int i = 0; i < chainedMonsters.Count - 1; i++)
        {
            if (chainedMonsters[i] == null || chainedMonsters[i].gameObject == null) continue;
            if (chainedMonsters[i + 1] == null || chainedMonsters[i + 1].gameObject == null) continue;

            lineRenderer.positionCount = i + 3;
            lineRenderer.SetPosition(i + 1, GetMonsterCenter(chainedMonsters[i]));
            lineRenderer.SetPosition(i + 2, GetMonsterCenter(chainedMonsters[i + 1]));

            yield return new WaitForSeconds((chainDuration / 2) - 0.02f);

            DealDamage(chainedMonsters[i + 1], i + 2);
        }

        yield return ChainDurationCoroutine();
    }

    Vector3 GetMonsterCenter(Monster monster)
    {
        BoxCollider collider = monster.GetComponent<BoxCollider>();
        if (collider != null)
        {
            return collider.bounds.center;
        }
        return monster.transform.position;
    }

    void DealDamage(Monster monster, int chainLevel)
    {
        if (monster == null || monster.gameObject == null) return;

        float damage = attackDamage * Mathf.Pow(0.9f, chainLevel - 1);
        monster.AdjustHealth(-damage);
    }
}
