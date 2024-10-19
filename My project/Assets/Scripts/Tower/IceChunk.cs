using UnityEngine;

public class IceChunk : MonoBehaviour
{
    public float speed = 10f;
    private Transform target;

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
