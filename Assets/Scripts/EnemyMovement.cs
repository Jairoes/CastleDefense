using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Configuración")]
    public WaypointPath waypointPath;
    public float moveSpeed = 5f;

    private int currentWaypointIndex = 0;

    void Update()
    {
        if (waypointPath == null) return;

        Transform targetWaypoint = waypointPath.GetWaypoint(currentWaypointIndex);
        Vector3 direction = targetWaypoint.position - transform.position;
        direction.y = 0f;

        transform.position += direction.normalized * moveSpeed * Time.deltaTime;

        // Rotar hacia el waypoint
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        // Verificar si llegó al waypoint
        Vector3 posEnemy = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 posWaypoint = new Vector3(targetWaypoint.position.x, 0f, targetWaypoint.position.z);
        float distancia = Vector3.Distance(posEnemy, posWaypoint);

        if (distancia < 0.2f)
        {
            currentWaypointIndex++;

            // Si llegó al final (castillo)
            if (currentWaypointIndex >= waypointPath.GetWaypointCount())
            {
                Debug.Log("¡Enemigo llegó al castillo!");
                Destroy(gameObject);
            }
        }
    }
}