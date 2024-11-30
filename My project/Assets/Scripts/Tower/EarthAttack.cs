using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthAttack : TowerAttackBase
{
    public ParticleSystem attackVFX;
    public float attackDamage = 250f;
    public float aoeRadius = 2f;

    public GameObject lightHolder;
    public MonsterDisplayHandler monsterDisplayHandler;

    private bool canAttack = true;

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
                attackDamage = 250f;
                attackCooldown = 5f;
                towerRadius = 4f;

                SetLightEmissionIntensity(1);
                break;
            case 2:
                attackDamage *= 2.5f;
                towerRadius *= 1.2f;
                aoeRadius *= 1.2f;
                attackCooldown = 7f;

                SetLightEmissionIntensity(2.5f);
                break;
            case 3:
                attackDamage *= 8f;
                towerRadius *= 1.25f;
                aoeRadius *= 1.2f;
                attackCooldown = 10f;

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
            DetectAndAttackMonsters();
        }
    }

    void DetectAndAttackMonsters()
    {
        Collider[] monstersInRange = Physics.OverlapSphere(transform.position, towerRadius);
        List<Monster> validMonsters = new List<Monster>();

        foreach (var collider in monstersInRange)
        {
            if (collider.CompareTag("Monster"))
            {
                Monster monster = collider.GetComponent<Monster>();
                if (monster != null && !monster.CantTarget())
                {
                    validMonsters.Add(monster);
                }
            }
        }

        if (validMonsters.Count == 0) return;

        Monster targetMonster = FindBestTargetMonster(validMonsters);
        ApplyDamageToMonstersInRadius(targetMonster);
        RotateVFXToTarget(targetMonster);
        attackVFX.Play();
        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        ResetVFX();
        canAttack = true;
    }

    void ResetVFX()
    {
        attackVFX.transform.rotation = Quaternion.identity;
    }

    Monster FindBestTargetMonster(List<Monster> validMonsters)
    {
        Monster bestTarget = null;
        int maxHits = 0;

        foreach (var monster in validMonsters)
        {
            int hits = CountMonstersInRadius(monster, validMonsters);

            if (hits > maxHits)
            {
                maxHits = hits;
                bestTarget = monster;
            }
        }

        return bestTarget;
    }

    int CountMonstersInRadius(Monster target, List<Monster> validMonsters)
    {
        int hits = 0;

        foreach (var monster in validMonsters)
        {
            if (Vector3.Distance(target.transform.position, monster.transform.position) <= aoeRadius)
            {
                hits++;
            }
        }

        return hits;
    }

    void ApplyDamageToMonstersInRadius(Monster targetMonster)
    {
        Collider[] monstersInAOE = Physics.OverlapSphere(targetMonster.transform.position, aoeRadius);

        foreach (var collider in monstersInAOE)
        {
            if (collider.CompareTag("Monster"))
            {
                Monster monster = collider.GetComponent<Monster>();
                if (monster != null && !monster.CantTarget())
                {
                    monster.AdjustHealth(-attackDamage);
                    monsterDisplayHandler.UpdateHealth(monster);
                }
            }
        }
    }

    void RotateVFXToTarget(Monster targetMonster)
    {
        Vector3 directionToMonster = (targetMonster.transform.position - transform.position).normalized;
        directionToMonster.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(directionToMonster);

        attackVFX.transform.rotation = targetRotation;
    }
}
