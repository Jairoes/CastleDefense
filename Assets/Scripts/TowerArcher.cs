using UnityEngine;

public class TowerArcher : MonoBehaviour
{
    [Header("Configuración")]
    public float range    = 7f;
    public float damage   = 20f;
    public float fireRate = 1f;

    [Header("Proyectil")]
    public GameObject projectilePrefab; // ← AGREGAR

    private float fireCountdown = 0f;
    private GameObject target;

    void Update()
    {
        FindTarget();

        if (target == null) return;

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }

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
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
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
            Debug.LogWarning("TowerArcher: no tiene proyectil asignado!");
            return;
        }

        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        ArrowProjectile ap = proj.GetComponent<ArrowProjectile>();

        if (ap != null)
            ap.SetTarget(target, damage);
        else
            Debug.LogWarning("ArrowProjectile component no encontrado!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}