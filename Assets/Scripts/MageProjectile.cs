using UnityEngine;

public class MageProjectile : MonoBehaviour
{
    private GameObject target;
    private float damage;
    private float splashDamage;
    private float splashRadius;
    private float speed = 8f;

    public void SetTarget(GameObject _target, float _damage, float _splashDamage, float _splashRadius)
    {
        target = _target;
        damage = _damage;
        splashDamage = _splashDamage;
        splashRadius = _splashRadius;
    }

    void Update()
    {
        // Si el enemigo murió antes de llegar, destruir el proyectil
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Mover la bola hacia el enemigo
        Vector3 direction = target.transform.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // Si llegó al enemigo — explotar
        if (direction.magnitude <= distanceThisFrame)
        {
            Explode();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
    }

    void Explode()
    {
        // Daño directo al enemigo principal
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
            enemyHealth.TakeDamage(damage);

        // Daño de salpicadura a enemigos cercanos
        Collider[] colliders = Physics.OverlapSphere(transform.position, splashRadius);
        foreach (Collider col in colliders)
        {
            if (col.gameObject == target) continue; // ya recibió daño directo

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