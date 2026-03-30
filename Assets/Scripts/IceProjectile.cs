using UnityEngine;

public class IceProjectile : MonoBehaviour
{
    private GameObject target;
    private float damage;
    private float slowPercent;
    private float slowDuration;
    private float slowRadius;
    private float speed = 10f;

    public float maxDistance = 7f;
    private Vector3 spawnPosition;

    public void SetTarget(GameObject _target, float _damage, float _slowPercent, float _slowDuration, float _slowRadius)
    {
        target       = _target;
        damage       = _damage;
        slowPercent  = _slowPercent;
        slowDuration = _slowDuration;
        slowRadius   = _slowRadius;
    }

    void Start()
    {
        spawnPosition = transform.position;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (Vector3.Distance(spawnPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = target.transform.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            Explode();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void Explode()
    {
        EnemyHealth health = target.GetComponent<EnemyHealth>();
        if (health != null)
            health.TakeDamage(damage);

        EnemyMovement movement = target.GetComponent<EnemyMovement>();
        if (movement != null)
            movement.ApplySlow(slowPercent, slowDuration);

        Collider[] colliders = Physics.OverlapSphere(transform.position, slowRadius);
        foreach (Collider col in colliders)
        {
            if (col.gameObject == target) continue;
            if (col.CompareTag("Enemy"))
            {
                EnemyMovement em = col.GetComponent<EnemyMovement>();
                if (em != null)
                    em.ApplySlow(slowPercent, slowDuration);
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, slowRadius);
    }
}