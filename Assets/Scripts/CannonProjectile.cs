using UnityEngine;

public class CannonProjectile : MonoBehaviour
{
    private GameObject target;
    private float damage;
    private float speed = 7f;

    public float maxDistance = 8f; // ← distancia máxima de viaje
    private Vector3 spawnPosition;

    public void SetTarget(GameObject _target, float _damage)
    {
        target = _target;
        damage = _damage;
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

        // Destruir si viajó demasiado lejos
        if (Vector3.Distance(spawnPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = target.transform.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void HitTarget()
    {
        EnemyHealth health = target.GetComponent<EnemyHealth>();
        if (health != null)
            health.TakeDamage(damage);

        Destroy(gameObject);
    }
}