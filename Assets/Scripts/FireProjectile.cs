using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    private GameObject target;
    private float damage;
    private FireZone.Settings zone;
    private float speed = 18f;

    public void SetTarget(GameObject _target, float _damage, FireZone.Settings _zone)
    {
        target = _target;
        damage = _damage;
        zone   = _zone;
    }

    void Update()
    {
        if (target == null)
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
        Vector3 impact = transform.position;

        // Golpe directo al objetivo, como siempre.
        EnemyHealth health = target.GetComponent<EnemyHealth>();
        if (health != null)
            health.TakeDamage(damage);

        // Y deja el suelo ardiendo donde cayo.
        FireZone.Spawn(impact, zone);

        Destroy(gameObject);
    }
}
