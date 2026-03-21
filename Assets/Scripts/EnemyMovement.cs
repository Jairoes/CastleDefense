using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Configuración")]
    public WaypointPath waypointPath;
    public float moveSpeed = 5f;
    public float damage = 10f;

    private int currentWaypointIndex = 0;

    void Update()
    {
        if (waypointPath == null) return;

        Transform targetWaypoint = waypointPath.GetWaypoint(currentWaypointIndex);
        Vector3 direction = targetWaypoint.position - transform.position;
        direction.y = 0f;

        transform.position += direction.normalized * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        Vector3 posEnemy = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 posWaypoint = new Vector3(targetWaypoint.position.x, 0f, targetWaypoint.position.z);
        float distancia = Vector3.Distance(posEnemy, posWaypoint);

        if (distancia < 0.2f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypointPath.GetWaypointCount())
            {
                AttackCastle();
            }
        }
    }

    void AttackCastle()
    {
        // Busca el castillo y le hace daño
        CastleHealth castle = FindFirstObjectByType<CastleHealth>();
        if (castle != null)
        {
            castle.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}