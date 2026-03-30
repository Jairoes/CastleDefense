using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    private GameObject target;
    private float damage;
    private float burnDamage;
    private float burnDelay;
    private float speed = 9f;

    public float maxDistance = 7f;
    private Vector3 spawnPosition;

    public void SetTarget(GameObject _target, float _damage, float _burnDamage, float _burnDelay)
    {
        target     = _target;
        damage     = _damage;
        burnDamage = _burnDamage;
        burnDelay  = _burnDelay;
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
        {
            health.TakeDamage(damage);
            health.ApplyBurn(burnDamage, burnDelay);
        }

        Destroy(gameObject);
    }
}