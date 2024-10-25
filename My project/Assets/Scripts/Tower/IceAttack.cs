using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

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
    public float attackDamage = 20f;

    private bool canAttack1 = true;
    private bool canAttack2 = true;

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
            attackSpeed *= 1.5f;
            slowPercent *= 1.2f;
            attackDamage *= 3.5f;
            SetLightEmissionIntensity(2.5f);
        }

        if (tier == 3)
        {
            attackSpeed *= 2f;
            slowPercent *= 1.5f;
            attackDamage *= 8f;
            towerRadius *= 1.5f;
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
        DetectMonstersInRadius();
    }

    void DetectMonstersInRadius()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, towerRadius);
        List<Monster> monstersInRange = new List<Monster>();

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Monster"))
            {
                Monster monster = hitCollider.GetComponent<Monster>();
                if (monster != null)
                {
                    monstersInRange.Add(monster);
                }
            }
        }

        if (monstersInRange.Count == 0) return;

        monstersInRange.Sort((a, b) => b.GetSpeed().CompareTo(a.GetSpeed()));

        if (canAttack1)
        {
            AttackMonster(monstersInRange[0], 1);
        }

        if (canAttack2)
        {
            AttackMonster(monstersInRange.Count > 1 ? monstersInRange[1] : monstersInRange[0], 2);
        }
    }

    void AttackMonster(Monster monster, int attackNumber)
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

    IEnumerator MoveVFX(GameObject vfxInstance, Monster targetMonster, int attackNumber)
    {
        if (attackNumber == 2)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (targetMonster != null)
        {
            vfxInstance.transform.LookAt(targetMonster.GetComponent<Collider>().bounds.center);
        }

        while (vfxInstance != null && targetMonster != null)
        {
            if (targetMonster == null || targetMonster.gameObject == null)
            {
                Destroy(vfxInstance);
                yield break;
            }

            Vector3 targetPosition = targetMonster.GetComponent<Collider>().bounds.center;
            Vector3 direction = (targetPosition - vfxInstance.transform.position).normalized;
            vfxInstance.transform.position += direction * attackSpeed * Time.deltaTime;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            vfxInstance.transform.rotation = Quaternion.Slerp(vfxInstance.transform.rotation, lookRotation, Time.deltaTime * attackSpeed);

            if (Vector3.Distance(vfxInstance.transform.position, targetPosition) < 0.1f)
            {
                Destroy(vfxInstance);

                if (targetMonster != null)
                {
                    targetMonster.AdjustHealth(-attackDamage);
                    monsterDisplayHandler.UpdateHealth(targetMonster);
                    targetMonster.AdjustSpeed(slowPercent, slowTime);

                    if (targetMonster.GetHealth() <= 0)
                    {
                        monsterDisplayHandler.HideMonsterDisplay();
                    }
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
