using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    private GameObject target;
    private float damage;
    private float burnDamage;
    private float burnDelay;
    private float speed = 9f;

    public void SetTarget(GameObject _target, float _damage, float _burnDamage, float _burnDelay)
    {
        target     = _target;
        damage     = _damage;
        burnDamage = _burnDamage;
        burnDelay  = _burnDelay;
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
        EnemyHealth health = target.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);       // daño de impacto inmediato
            health.ApplyBurn(burnDamage, burnDelay); // tick de quemadura después
        }

        Destroy(gameObject);
    }
}