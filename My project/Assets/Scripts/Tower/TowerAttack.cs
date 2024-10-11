using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class TowerAttack : MonoBehaviour
{
    public GameObject attackVFX;
    public float towerRadius = 4f;
    public float attackCooldown = 1f;
    public float attackSpeed = 1f;

    private bool canAttack = true;

    void Start()
    {

    }

    void Update()
    {
        DetectMonstersInRadius();
    }

    void DetectMonstersInRadius()
    {
        if (!canAttack) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, towerRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Monster"))
            {
                Transform monsterTransform = hitCollider.transform;
                AttackMonster(monsterTransform);
                break;
            }
        }
    }

    void AttackMonster(Transform monster)
    {
        GameObject vfxInstance = Instantiate(attackVFX, transform.position, Quaternion.identity);

        StartCoroutine(MoveVFX(vfxInstance, monster));
        StartCoroutine(StartCooldown());
    }

    IEnumerator MoveVFX(GameObject vfxInstance, Transform targetMonster)
    {
        while (vfxInstance != null)
        {
            Vector3 direction = (targetMonster.position - vfxInstance.transform.position).normalized;
            vfxInstance.transform.position += direction * attackSpeed * Time.deltaTime;

            if (Vector3.Distance(vfxInstance.transform.position, targetMonster.position) < 0.1f)
            {
                Debug.Log("Hit");

                Destroy(vfxInstance);

                //deal damage to the monster here
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator StartCooldown()
    {
        canAttack = false;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }
}
