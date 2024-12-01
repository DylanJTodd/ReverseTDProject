using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class IceAttack : TowerAttackBase
{
    public GameObject attackVFX;
    public MonsterDisplayHandler monsterDisplayHandler;

    public float attackCooldown1 = 0.5f;
    public float attackCooldown2 = 0.5f;
    public float attackSpeed = 10f;
    public float slowPercent = 20f;

    public int slowTime = 2;
    public float attackDamage = 20f;

    private bool canAttack1 = true;
    private bool canAttack2 = true;
    private bool isAttacking1 = false;
    private bool isAttacking2 = false;

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
                attackDamage = 20f;
                attackCooldown = 0.5f;
                towerRadius = 4;
                slowPercent *= 1.25f;
                SetLightEmissionIntensity(1);
                break;
            case 2:
                attackSpeed *= 1.5f;
                slowPercent *= 1.2f;
                attackDamage *= 2f;
                SetLightEmissionIntensity(2.5f);
                break;
            case 3:
                slowPercent *= 1.5f;
                attackDamage *= 3f;
                towerRadius *= 1.5f;
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
        DetectMonstersInRadius();
    }

    void DetectMonstersInRadius()
    {
        if (!canAttack1 && !canAttack2) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, towerRadius);
        List<Monster> monstersInRange = new List<Monster>();

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Monster"))
            {
                Monster monster = hitCollider.GetComponent<Monster>();
                if (monster != null && monster.gameObject.activeInHierarchy)
                {
                    monstersInRange.Add(monster);
                }
            }
        }

        if (monstersInRange.Count == 0) return;

        monstersInRange.Sort((a, b) => b.GetSpeed().CompareTo(a.GetSpeed()));

        if (canAttack1 && !isAttacking1 && monstersInRange.Count > 0)
        {
            AttackMonster(monstersInRange[0], 1);
        }

        if (canAttack2 && !isAttacking2 && monstersInRange.Count > 0)
        {
            AttackMonster(monstersInRange.Count > 1 ? monstersInRange[1] : monstersInRange[0], 2);
        }
    }

    void AttackMonster(Monster monster, int attackNumber)
    {
        if (monster == null || !monster.gameObject.activeInHierarchy) return;

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
        if (attackNumber == 1) isAttacking1 = true;
        else isAttacking2 = true;

        if (attackNumber == 2)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (targetMonster == null || !targetMonster.gameObject.activeInHierarchy)
        {
            Destroy(vfxInstance);
            if (attackNumber == 1) isAttacking1 = false;
            else isAttacking2 = false;
            yield break;
        }

        Vector3 targetPosition = targetMonster.GetComponent<Collider>().bounds.center;
        float startTime = Time.time;
        float journeyLength = Vector3.Distance(vfxInstance.transform.position, targetPosition);
        float timeToReachTarget = journeyLength / attackSpeed;

        while (Time.time - startTime < timeToReachTarget)
        {
            if (targetMonster == null || !targetMonster.gameObject.activeInHierarchy)
            {
                Destroy(vfxInstance);
                if (attackNumber == 1) isAttacking1 = false;
                else isAttacking2 = false;
                yield break;
            }

            float distanceCovered = (Time.time - startTime) * attackSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            vfxInstance.transform.position = Vector3.Lerp(vfxInstance.transform.position, targetPosition, fractionOfJourney);

            Vector3 direction = (targetPosition - vfxInstance.transform.position).normalized;
            if (direction != Vector3.zero)
            {
                vfxInstance.transform.rotation = Quaternion.LookRotation(direction);
            }

            if (Vector3.Distance(vfxInstance.transform.position, targetPosition) < 0.1f)
            {
                break;
            }

            yield return null;
        }

        if (targetMonster != null && targetMonster.gameObject.activeInHierarchy)
        {
            targetMonster.AdjustHealth(-attackDamage);
            monsterDisplayHandler.UpdateHealth(targetMonster);
            targetMonster.AdjustSpeed(slowPercent, slowTime);

            if (targetMonster.GetHealth() <= 0)
            {
                monsterDisplayHandler.HideMonsterDisplay();
            }
        }

        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
        }

        if (attackNumber == 1) isAttacking1 = false;
        else isAttacking2 = false;
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