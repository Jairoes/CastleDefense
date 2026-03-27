using UnityEngine;

public class IceProjectile : MonoBehaviour
{
    private GameObject target;
    private float damage;
    private float slowPercent;
    private float slowDuration;
    private float slowRadius;
    private float speed = 10f;

    // Llamado por TowerIce al disparar
    public void SetTarget(GameObject _target, float _damage, float _slowPercent, float _slowDuration, float _slowRadius)
    {
        target       = _target;
        damage       = _damage;
        slowPercent  = _slowPercent;
        slowDuration = _slowDuration;
        slowRadius   = _slowRadius;
    }

    void Update()
    {
        // Si el enemigo murió antes de llegar, destruir el proyectil
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Mover el proyectil hacia el enemigo
        Vector3 direction = target.transform.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // Si llegó al enemigo — explotar con slow
        if (direction.magnitude <= distanceThisFrame)
        {
            Explode();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
    }

    void Explode()
    {
        // 1. Daño + slow al enemigo impactado directamente
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
            enemyHealth.TakeDamage(damage);

        EnemyMovement enemyMovement = target.GetComponent<EnemyMovement>();
        if (enemyMovement != null)
            enemyMovement.ApplySlow(slowPercent, slowDuration);

        // 2. Slow a enemigos cercanos en el radio (sin daño extra)
        Collider[] colliders = Physics.OverlapSphere(transform.position, slowRadius);
        foreach (Collider col in colliders)
        {
            if (col.gameObject == target) continue; // ya fue procesado arriba

            if (col.CompareTag("Enemy"))
            {
                EnemyMovement em = col.GetComponent<EnemyMovement>();
                if (em != null)
                    em.ApplySlow(slowPercent, slowDuration);
            }
        }

        Destroy(gameObject);
    }

    // Muestra el radio de slow en el editor (círculo celeste)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, slowRadius);
    }
}