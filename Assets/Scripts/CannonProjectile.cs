using UnityEngine;

public class CannonProjectile : MonoBehaviour
{
    private GameObject target;
    private float damage;
    private float speed = 6f; // lento, se siente pesado

    public void SetTarget(GameObject _target, float _damage)
    {
        target = _target;
        damage = _damage;
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

        // Rotar hacia donde va
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