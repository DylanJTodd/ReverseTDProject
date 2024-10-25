using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningChainTower : MonoBehaviour
{
    public float attackRadius = 5f;
    public int maxChain = 3;
    public int maxBranch = 3;
    public float chainRadius = 2f;
    public float baseDamage = 30f;
    public float chainDuration = 1.5f;
    public LineRenderer lineRenderer;
    public MonsterDisplayHandler monsterDisplayHandler;
    public float attackCooldown = 1f;

    private bool canAttack = true;
    private List<Monster> activeChainedMonsters;

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
            maxChain *= 2;
            SetLightEmissionIntensity(2.5f);
        }

        if (tier == 3)
        {
            baseDamage *= 5f;
            maxChain *= 3;
            attackCooldown *= 0.75f;
            attackRadius *= 1.5f;
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
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = true;

        // Add the initial segment between the tower and the first monster
        Vector3 initialPosition = GetMonsterCenter(chainedMonsters[0]);
        if (IsValidPosition(initialPosition))
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, initialPosition);
        }

        yield return new WaitForSeconds((chainDuration / 2) - 0.02f);

        if (chainedMonsters[0] != null && chainedMonsters[0].gameObject != null)
        {
            DealDamage(chainedMonsters[0], 1);
        }

        for (int i = 0; i < chainedMonsters.Count - 1; i++)
        {
            // Check if monsters are still valid (not destroyed)
            if (chainedMonsters[i] == null || chainedMonsters[i].gameObject == null) continue;
            if (chainedMonsters[i + 1] == null || chainedMonsters[i + 1].gameObject == null) continue;

            Vector3 currentPos = GetMonsterCenter(chainedMonsters[i]);
            Vector3 nextPos = GetMonsterCenter(chainedMonsters[i + 1]);

            // Ensure valid positions before setting
            if (IsValidPosition(currentPos) && IsValidPosition(nextPos))
            {
                lineRenderer.positionCount = i + 3;
                lineRenderer.SetPosition(i + 1, currentPos);
                lineRenderer.SetPosition(i + 2, nextPos);
            }

            yield return new WaitForSeconds((chainDuration / 2) - 0.02f);

            if (chainedMonsters[i + 1] != null && chainedMonsters[i + 1].gameObject != null)
            {
                DealDamage(chainedMonsters[i + 1], i + 2);
            }

            // Start a coroutine to remove the chain segment after chainDuration
            StartCoroutine(RemoveChainSegment(i + 2));
        }

        // Start a coroutine to remove the initial segment after chainDuration
        StartCoroutine(RemoveChainSegment(1));
    }

    bool IsValidPosition(Vector3 position)
    {
        // Check if the position is (0, 0, 0), which is usually an invalid or unintended value
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
        Collider collider = monster.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds.center;
        }
        return monster.transform.position;
    }

    void DealDamage(Monster monster, int chainLevel)
    {
        float damage = baseDamage * Mathf.Pow(0.9f, chainLevel - 1);

        if (monster == null || monster.gameObject == null || monster.GetHealth() <= 0) return;

        monster.AdjustHealth(-damage);
        monsterDisplayHandler.UpdateHealth(monster);

        if (monster.GetHealth() <= 0)
        {
            // Immediately remove the chain segment if the monster dies
            int segmentIndex = activeChainedMonsters.IndexOf(monster);
            if (segmentIndex >= 0)
            {
                StartCoroutine(RemoveChainSegment(segmentIndex + 1));
            }
        }
    }
}
