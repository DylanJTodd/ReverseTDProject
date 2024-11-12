using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthAttack : MonoBehaviour
{
    public ParticleSystem attackVFX;
    public float attackDamage = 2f;
    public float attackCooldown = 1f;
    public float towerRadius = 5f;
    public float aoeRadius = 2f;  // Radius around the target monster for the AOE attack
    public MonsterDisplayHandler monsterDisplayHandler;

    private bool canAttack = true;

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
