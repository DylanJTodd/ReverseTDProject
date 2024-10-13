using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

//still need to implement vfx
public class IceAttack : MonoBehaviour
{
    public GameObject attackVFX;
    public MonsterDisplayHandler monsterDisplayHandler;

    public float towerRadius = 4f;
    public float attackCooldown1 = 0.5f;
    public float attackCooldown2 = 0.5f;
    public float attackSpeed = 50f;
    public float slowPercent = 50f;

    public int slowTime = 2;
    public float attackDamage = 1f;

    private bool canAttack1 = true;
    private bool canAttack2 = true;

    void Update()
    {
        DetectMonstersInRadius();
    }

    void DetectMonstersInRadius()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, towerRadius);
        List<Transform> monstersInRange = new List<Transform>();

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Monster"))
            {
                monstersInRange.Add(hitCollider.transform);
            }
        }

        if (monstersInRange.Count == 0) return;

        // Handle first projectile attack
        if (canAttack1)
        {
            Transform targetMonster = monstersInRange[0];
            AttackMonster(targetMonster, 1);
        }

        // Handle second projectile attack
        if (canAttack2)
        {
            Transform targetMonster = monstersInRange.Count > 1 ? monstersInRange[1] : monstersInRange[0];
            AttackMonster(targetMonster, 2);
        }
    }

    void AttackMonster(Transform monster, int attackNumber)
    {
        GameObject vfxInstance = Instantiate(attackVFX, transform.position, Quaternion.identity);
        StartCoroutine(MoveVFX(vfxInstance, monster, attackNumber));

        if (attackNumber == 1)
        {
            StartCoroutine(StartCooldown1());
        }
        else
        {
            StartCoroutine(StartCooldown2());
        }
    }

    IEnumerator MoveVFX(GameObject vfxInstance, Transform targetMonster, int attackNumber)
    {
        if (attackNumber == 2)
        {
            yield return new WaitForSeconds(0.1f);
        }

        vfxInstance.transform.LookAt(targetMonster);

        while (vfxInstance != null)
        {
            Vector3 direction = (targetMonster.position - vfxInstance.transform.position).normalized;
            vfxInstance.transform.position += direction * attackSpeed * Time.deltaTime;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            vfxInstance.transform.rotation = Quaternion.Slerp(vfxInstance.transform.rotation, lookRotation, Time.deltaTime * attackSpeed);

            if (Vector3.Distance(vfxInstance.transform.position, targetMonster.position) < 0.1f)
            {
                Destroy(vfxInstance);

                Monster monster = targetMonster.GetComponent<Monster>();

                monster.AdjustHealth(-attackDamage);
                monsterDisplayHandler.UpdateHealth(monster);

                monster.AdjustSpeed(slowPercent, slowTime);

                if (monster.GetHealth() <= 0)
                {
                    monsterDisplayHandler.HideMonsterDisplay();
                }
            }
            yield return null;
        }
    }

    IEnumerator StartCooldown1()
    {
        canAttack1 = false;
        yield return new WaitForSeconds(attackCooldown1);
        canAttack1 = true;
    }

    IEnumerator StartCooldown2()
    {
        canAttack2 = false;
        yield return new WaitForSeconds(attackCooldown2);
        canAttack2 = true;
    }
}
