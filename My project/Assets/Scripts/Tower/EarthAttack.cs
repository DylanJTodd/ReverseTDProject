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
                if (monster != null)
                {
                    validMonsters.Add(monster);
                }
            }
        }

        if (validMonsters.Count == 0) return;

        // Find the monster that maximizes the number of monsters in the AOE radius
        Monster targetMonster = FindBestTargetMonster(validMonsters);
        ApplyDamageToMonstersInRadius(targetMonster);
        RotateVFXToTarget(targetMonster);
        attackVFX.Play(); // Play VFX after rotation
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
        attackVFX.transform.rotation = Quaternion.identity;  // Reset rotation
        Debug.Log($"VFX Reset: Rotation {attackVFX.transform.rotation.eulerAngles}");
    }

    Monster FindBestTargetMonster(List<Monster> validMonsters)
    {
        Monster bestTarget = null;
        int maxHits = 0;

        // Loop through each monster to see how many others are within the AOE radius around it
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
            // Check distance between the target monster and the others
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
                if (monster != null)
                {
                    monster.AdjustHealth(-attackDamage);
                    monsterDisplayHandler.UpdateHealth(monster);
                }
            }
        }
    }

    void RotateVFXToTarget(Monster targetMonster)
    {
        // Calculate the direction to the target monster
        Vector3 directionToMonster = (targetMonster.transform.position - transform.position).normalized;

        // Flatten the direction vector by setting Y to 0 to avoid vertical rotation
        directionToMonster.y = 0;

        // Create a rotation that faces the target monster on the XZ plane
        Quaternion targetRotation = Quaternion.LookRotation(directionToMonster);

        // Rotate the VFX to face the target monster
        attackVFX.transform.rotation = targetRotation;

        Debug.Log($"VFX Rotated (XZ only): Target {targetMonster.transform.position}, Rotation {attackVFX.transform.rotation.eulerAngles}");
    }
}

