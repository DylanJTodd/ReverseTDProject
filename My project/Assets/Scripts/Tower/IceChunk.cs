using UnityEngine;

public class IceChunk : MonoBehaviour
{
    public float speed = 10f;
    public float splashRadius = 3f;

    public int damage = 5;
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
                // Debug position
                Debug.Log($"Spawning effect at position: {transform.position}");
                
                // Spawn effect slightly above ground to be visible
                Vector3 effectPosition = transform.position + Vector3.up * 0.5f;
                
                // Show splash effect
                if (splashEffectPrefab != null)
                {
                    GameObject splashEffectInstance = Instantiate(splashEffectPrefab, effectPosition, Quaternion.identity);
                    Debug.Log($"Splash effect instantiated: {splashEffectInstance.name}");
                    
                    // Make sure scale is reasonable
                    float scale = 1f; // Start with base scale of 1
                    splashEffectInstance.transform.localScale = new Vector3(scale, scale, scale);

                    // Get and check particle system
                    ParticleSystem particleSystem = splashEffectInstance.GetComponent<ParticleSystem>();
                    if (particleSystem != null)
                    {
                        Debug.Log("Found particle system, playing...");
                        particleSystem.Clear(); // Clear any existing particles
                        particleSystem.Play();
                    }
                    else
                    {
                        Debug.LogWarning("No ParticleSystem found on splash effect prefab!");
                    }
                    
                    // Don't destroy too quickly
                    Destroy(splashEffectInstance, 2f);
                }
                else
                {
                    Debug.LogError("Splash effect prefab is not assigned!");
                }

                // Apply damage after effect
                MonsterManager.instance.ApplyDamageToMonstersInRadius(target.position, splashRadius, damage);

                Destroy(gameObject);
            }
        }
        else
        {
            // Destroy ice chunk if no target
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }

    private void OnDrawGizmos()
    {
        // Draw a yellow sphere at spawn position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.5f);
    }
}
