using UnityEngine;

public class IceChunk : MonoBehaviour
{
    public float speed = 10f;
    public int splashRadius = 0;

    public int damage = 10;
    private Transform target;
    public GameObject splashEffectPrefab;
    public void ThrowTowards(Transform targetTransform)
    {
        target = targetTransform;
    }

    private void Update()
    {
        if (target != null)
        {
            // Move towards the target
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // Optional: Destroy ice chunk if it reaches the target
            if (Vector3.Distance(transform.position, target.position) < 0.5f)
            {
                // Implement effect on monster here
                foreach (var monster in MonsterManager.instance.GetMonstersInRadius(transform.position, splashRadius))
                {
                    monster.TakeDamage(damage);
                }

                // Show splash effect
                if (splashEffectPrefab != null)
                {
                    GameObject splashEffectInstance = Instantiate(splashEffectPrefab, transform.position, Quaternion.identity);

                    float scale = splashRadius / 5f;
                    splashEffectInstance.transform.localScale = new Vector3(scale, scale, scale);

                    ParticleSystem particleSystem = splashEffectInstance.GetComponent<ParticleSystem>();
                    if (particleSystem != null)
                    {
                        particleSystem.Play();
                    }
                    Destroy(splashEffectInstance, 1f);
                }

                Destroy(gameObject);
            }
        }
        else
        {
            // Destroy ice chunk if no target
            Destroy(gameObject);
        }
    }
}
