using UnityEngine;

public class TowerIce : MonoBehaviour
{
    [Header("Configuración")]
    public float range      = 5f;
    public float damage     = 15f;
    public float slowPercent = 0.5f;  // 50% más lento
    public float slowDuration = 2f;   // durante 2 segundos
    public float slowRadius = 2.5f;
    public float fireRate   = 0.6f;   // disparos por segundo

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

        // Disparar
        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }
        fireCountdown -= Time.deltaTime;
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
            Debug.LogWarning("TowerIce: no tiene proyectil asignado!");
            return;
        }

        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        IceProjectile ip = proj.GetComponent<IceProjectile>();

        if (ip != null)
            ip.SetTarget(target, damage, slowPercent, slowDuration, slowRadius);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}