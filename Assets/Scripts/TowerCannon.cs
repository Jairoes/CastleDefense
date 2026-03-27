using UnityEngine;

public class TowerCannon : MonoBehaviour
{
    [Header("Configuración")]
    public float range    = 8f;
    public float damage   = 55f;
    public float fireRate = 0.33f; // 1 disparo cada 3 segundos

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
            Debug.LogWarning("TowerCannon: no tiene proyectil asignado!");
            return;
        }

        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        CannonProjectile cp = proj.GetComponent<CannonProjectile>();

        if (cp != null)
            cp.SetTarget(target, damage);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}