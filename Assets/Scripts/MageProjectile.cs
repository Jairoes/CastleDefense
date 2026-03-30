using UnityEngine;

public class MageProjectile : MonoBehaviour
{
    private GameObject target;
    private float damage;
    private float splashDamage;
    private float splashRadius;
    private float speed = 8f;

    public float maxDistance = 7f;
    private Vector3 spawnPosition;

    public void SetTarget(GameObject _target, float _damage, float _splashDamage, float _splashRadius)
    {
        target       = _target;
        damage       = _damage;
        splashDamage = _splashDamage;
        splashRadius = _splashRadius;
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
    }

    void Explode()
    {
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
            enemyHealth.TakeDamage(damage);

        Collider[] colliders = Physics.OverlapSphere(transform.position, splashRadius);
        foreach (Collider col in colliders)
        {
            if (col.gameObject == target) continue;
            if (col.CompareTag("Enemy"))
            {
                EnemyHealth eh = col.GetComponent<EnemyHealth>();
                if (eh != null)
                    eh.TakeDamage(splashDamage);
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}