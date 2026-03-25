using UnityEngine;

public class TowerMage : MonoBehaviour
{
    [Header("Configuración")]
    public float range = 7f;
    public float damage = 40f;
    public float splashDamage = 10f;
    public float splashRadius = 3f;
    public float fireRate = 0.67f; // 1 disparo cada 1.5 segundos

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
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
            target = nearestEnemy;
        else
            target = null;
    }

    void Shoot()
    {
        if (target == null) return;
        if (projectilePrefab == null)
        {
            Debug.LogWarning("TowerMage no tiene proyectil asignado!");
            return;
        }

        // Crear la bola mágica en la posición de la torre
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        // Decirle al proyectil a quién perseguir y cuánto daño hacer
        MageProjectile mp = proj.GetComponent<MageProjectile>();
        if (mp != null)
        {
            mp.SetTarget(target, damage, splashDamage, splashRadius);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}