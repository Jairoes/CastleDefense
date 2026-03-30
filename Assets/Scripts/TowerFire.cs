using UnityEngine;

public class TowerFire : MonoBehaviour
{
    [Header("Configuración")]
    public float range      = 6f;
    public float damage     = 25f;
    public float burnDamage = 10f;  // tick extra de quemadura
    public float burnDelay  = 1f;   // segundos después del impacto
    public float fireRate   = 0.5f; // 1 disparo cada 2 segundos

    [Header("Proyectil")]
    public GameObject projectilePrefab;

    private float fireCountdown = 0f;
    private GameObject target;

    void Update()
    {
        FindTarget();

        if (target == null) return;

        // Rotar hacia el enemigo
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }

        // Countdown primero, luego verificar
        fireCountdown -= Time.deltaTime;

        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }
    }

    void FindTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                nearestEnemy = enemy;
            }
        }

        target = (nearestEnemy != null && shortestDistance <= range) ? nearestEnemy : null;
    }

    void Shoot()
    {
        if (target == null) return;

        if (projectilePrefab == null)
        {
            Debug.LogWarning("TowerFire: no tiene proyectil asignado!");
            return;
        }

        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        FireProjectile fp = proj.GetComponent<FireProjectile>();

        if (fp != null)
            fp.SetTarget(target, damage, burnDamage, burnDelay);
        else
            Debug.LogWarning("FireProjectile component no encontrado!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}