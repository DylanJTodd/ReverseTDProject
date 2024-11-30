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
    private Dictionary<Monster, Vector3> lastKnownPositions = new Dictionary<Monster, Vector3>();
    private Dictionary<Monster, Vector3> frozenPositions = new Dictionary<Monster, Vector3>();
    private HashSet<Monster> frozenMonsters = new HashSet<Monster>();
    private Dictionary<Monster, Vector3> lastValidPositions = new Dictionary<Monster, Vector3>();
    private HashSet<Monster> deadMonsters = new HashSet<Monster>();


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
        if (canAttack)
        {
            DetectAndChainMonsters();
        }
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
        if (chainedMonsters == null || chainedMonsters.Count == 0)
        {
            yield break;
        }

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = true;

        // Initialize with tower position
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);

        Vector3 initialPosition = GetMonsterCenter(chainedMonsters[0]);
        if (!IsValidPosition(initialPosition))
        {
            lineRenderer.enabled = false;
            yield break;
        }

        lineRenderer.SetPosition(1, initialPosition);
        yield return new WaitForSeconds((chainDuration / 2) - 0.02f);

        if (chainedMonsters[0] != null && chainedMonsters[0].gameObject != null)
        {
            DealDamage(chainedMonsters[0], 1);
        }

        for (int i = 0; i < chainedMonsters.Count - 1; i++)
        {
            if (!lineRenderer.enabled) yield break;

            Vector3 currentPos = GetMonsterCenter(chainedMonsters[i]);
            Vector3 nextPos = GetMonsterCenter(chainedMonsters[i + 1]);

            if (!IsValidPosition(currentPos) || !IsValidPosition(nextPos))
            {
                continue;
            }

            int newPositionCount = i + 3;
            if (newPositionCount > lineRenderer.positionCount)
            {
                lineRenderer.positionCount = newPositionCount;
                lineRenderer.SetPosition(newPositionCount - 2, currentPos);
                lineRenderer.SetPosition(newPositionCount - 1, nextPos);
            }

            yield return new WaitForSeconds((chainDuration / 2) - 0.02f);

            if (chainedMonsters[i + 1] != null && chainedMonsters[i + 1].gameObject != null)
            {
                DealDamage(chainedMonsters[i + 1], i + 2);
            }
        }

        StartCoroutine(RemoveChainGradually());
    }

    private IEnumerator RemoveChainGradually()
    {
        while (lineRenderer.positionCount > 0 && lineRenderer.enabled)
        {
            yield return new WaitForSeconds(chainDuration / 4);
            if (lineRenderer.positionCount > 0)
            {
                lineRenderer.positionCount--;
            }
        }
        lineRenderer.enabled = false;
    }

    bool IsValidPosition(Vector3 position)
    {
        return position != Vector3.zero;
    }


    IEnumerator RemoveChainSegment(int segmentIndex)
    {
        yield return new WaitForSeconds(chainDuration);

        if (lineRenderer.positionCount > segmentIndex)
        {
            lineRenderer.positionCount = segmentIndex;
        }
    }

    Vector3 GetMonsterCenter(Monster monster)
    {
        if (monster == null || monster.gameObject == null || monster.GetHealth() <= 0)
        {
            return lastValidPositions.ContainsKey(monster) ? lastValidPositions[monster] : Vector3.zero;
        }

        Collider collider = monster.GetComponent<Collider>();
        Vector3 position = collider != null ? collider.bounds.center : monster.transform.position;

        if (position != Vector3.zero)
        {
            lastValidPositions[monster] = position;
        }

        return position;
    }

    void DealDamage(Monster monster, int chainLevel)
    {
        if (monster == null || monster.gameObject == null || monster.GetHealth() <= 0) return;

        float damage = baseDamage * Mathf.Pow(0.9f, chainLevel - 1);
        monster.AdjustHealth(-damage);
        monsterDisplayHandler.UpdateHealth(monster);

        if (monster.GetHealth() <= 0)
        {
            deadMonsters.Add(monster);
            RebuildChainExcludingMonster(monster);
        }
    }

    private void RebuildChainExcludingMonster(Monster deadMonster)
    {
        if (activeChainedMonsters == null || !activeChainedMonsters.Contains(deadMonster)) return;
        if (lineRenderer.positionCount <= 2)
        {
            lineRenderer.enabled = false;
            return;
        }

        int deadIndex = activeChainedMonsters.IndexOf(deadMonster);
        if (deadIndex < 0) return;

        int currentPositionCount = lineRenderer.positionCount;
        int newPositionCount = currentPositionCount - 1;

        if (newPositionCount < 1)
        {
            lineRenderer.enabled = false;
            return;
        }

        Vector3[] positions = new Vector3[newPositionCount];
        int sourceIndex = 0;
        int targetIndex = 0;

        while (targetIndex < newPositionCount && sourceIndex < currentPositionCount)
        {
            if (sourceIndex != deadIndex + 1)
            {
                positions[targetIndex] = lineRenderer.GetPosition(sourceIndex);
                targetIndex++;
            }
            sourceIndex++;
        }

        if (targetIndex == newPositionCount)
        {
            lineRenderer.positionCount = newPositionCount;
            lineRenderer.SetPositions(positions);
            activeChainedMonsters.RemoveAt(deadIndex);
        }
    }
}
