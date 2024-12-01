using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningChainTower : TowerAttackBase
{
    public float attackRadius = 5f;
    public int maxChain = 3;
    public int maxBranch = 3;
    public float chainRadius = 2f;
    public float chainDuration = 1.5f;
    public LineRenderer lineRenderer;
    public MonsterDisplayHandler monsterDisplayHandler;

    private bool canAttack = true;
    private List<Monster> activeChainedMonsters;
    private Dictionary<Monster, Vector3> monsterPositions;
    private bool isAnimatingChain = false;

    public GameObject lightHolder;

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
                baseDamage = 30f;
                attackCooldown = 1f;
                towerRadius = 5f;
                SetLightEmissionIntensity(1);
                break;
            case 2:
                baseDamage *= 2.5f;
                maxChain *= 2;
                SetLightEmissionIntensity(2.5f);
                break;
            case 3:
                baseDamage *= 2f;
                maxChain *= 3;
                attackCooldown *= 0.75f;
                attackRadius *= 1.5f;
                SetLightEmissionIntensity(5);
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
        if (canAttack && !isAnimatingChain)
        {
            DetectAndChainMonsters();
        }
    }

    Vector3 GetMonsterPosition(Monster monster)
    {
        if (!IsMonsterValid(monster)) return Vector3.zero;

        Collider collider = monster.GetComponent<Collider>();
        return collider != null ? collider.bounds.center : monster.transform.position;
    }

    void DetectAndChainMonsters()
    {
        List<Monster> validMonsters = GetValidMonstersInRange();
        if (validMonsters.Count == 0) return;

        monsterPositions = new Dictionary<Monster, Vector3>();
        foreach (var monster in validMonsters)
        {
            monsterPositions[monster] = GetMonsterPosition(monster);
        }

        Monster initialTarget = FindBestInitialMonster(validMonsters);
        if (initialTarget != null)
        {
            activeChainedMonsters = new List<Monster>();
            ChainToMonsters(initialTarget, validMonsters, activeChainedMonsters, 1);

            if (activeChainedMonsters.Count > 0)
            {
                StartCoroutine(AnimateLightningChain());
            }
        }
    }

    List<Monster> GetValidMonstersInRange()
    {
        List<Monster> validMonsters = new List<Monster>();
        Collider[] monstersInRange = Physics.OverlapSphere(transform.position, attackRadius);

        foreach (var collider in monstersInRange)
        {
            if (collider.CompareTag("Monster"))
            {
                Monster monster = collider.GetComponent<Monster>();
                if (IsMonsterValid(monster))
                {
                    validMonsters.Add(monster);
                }
            }
        }

        return validMonsters;
    }

    bool IsMonsterValid(Monster monster)
    {
        return monster != null &&
               monster.gameObject != null &&
               monster.GetHealth() > 0;
    }

    Monster FindBestInitialMonster(List<Monster> validMonsters)
    {
        Monster bestMonster = null;
        int maxHitCount = 0;

        foreach (Monster monster in validMonsters)
        {
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
        if (!IsMonsterValid(currentMonster) ||
            chainedMonsters.Contains(currentMonster) ||
            chainLevel > maxChain) return;

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
        Vector3 currentPos = monsterPositions[currentMonster];

        foreach (var monster in allMonsters)
        {
            if (!IsMonsterValid(monster) || alreadyChained.Contains(monster)) continue;

            Vector3 targetPos = monsterPositions[monster];
            float distance = Vector3.Distance(currentPos, targetPos);
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
        Vector3 currentPos = monsterPositions[currentMonster];

        foreach (var monster in allMonsters)
        {
            if (!IsMonsterValid(monster) || alreadyChained.Contains(monster)) continue;

            Vector3 targetPos = monsterPositions[monster];
            float distance = Vector3.Distance(currentPos, targetPos);
            if (distance <= chainRadius)
            {
                closeMonsters.Add(monster);
            }
        }

        closeMonsters.Sort((a, b) =>
            Vector3.Distance(currentPos, monsterPositions[a])
            .CompareTo(Vector3.Distance(currentPos, monsterPositions[b])));

        int count = Mathf.Min(closeMonsters.Count, maxCount);
        return count > 0 ? closeMonsters.GetRange(0, count) : new List<Monster>();
    }

    IEnumerator AnimateLightningChain()
    {
        isAnimatingChain = true;
        canAttack = false;

        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.enabled = true;

        for (int i = 0; i < activeChainedMonsters.Count; i++)
        {
            Monster monster = activeChainedMonsters[i];
            if (!IsMonsterValid(monster)) continue;

            Vector3 position = GetMonsterPosition(monster);
            if (position == Vector3.zero) continue;

            lineRenderer.positionCount = i + 2;
            lineRenderer.SetPosition(i + 1, position);

            DealDamage(monster, i + 1);

            yield return new WaitForSeconds(chainDuration / 2);
        }

        yield return new WaitForSeconds(chainDuration);

        lineRenderer.enabled = false;
        isAnimatingChain = false;

        yield return new WaitForSeconds(attackCooldown - chainDuration);
        canAttack = true;
    }

    void DealDamage(Monster monster, int chainLevel)
    {
        if (!IsMonsterValid(monster)) return;

        float damage = baseDamage * Mathf.Pow(0.9f, chainLevel - 1);
        monster.AdjustHealth(-damage);
        monsterDisplayHandler.UpdateHealth(monster);
    }
}