using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Configuración")]
    public WaypointPath waypointPath;
    public float moveSpeed = 5f;
    public float damage = 10f;

    private int currentWaypointIndex = 0;

    private float baseSpeed;
    private float slowTimer = 0f;
    private bool isSlowed = false;

    void Start()
    {
        baseSpeed = moveSpeed; // guardamos la velocidad original
    }

    void Update()
    {
        // Tick del slow
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
                RemoveSlow();
        }

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

        Vector3 posEnemy    = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 posWaypoint = new Vector3(targetWaypoint.position.x, 0f, targetWaypoint.position.z);
        float distancia = Vector3.Distance(posEnemy, posWaypoint);

        if (distancia < 0.2f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypointPath.GetWaypointCount())
                AttackCastle();
        }
    }

    // Llamado por IceProjectile al impactar
    public void ApplySlow(float slowPercent, float duration)
    {
        // Si ya está slowed, solo refrescamos el timer
        moveSpeed  = baseSpeed * (1f - slowPercent);
        slowTimer  = duration;
        isSlowed   = true;
    }

    void RemoveSlow()
    {
        moveSpeed = baseSpeed;
        isSlowed  = false;
        slowTimer = 0f;
    }

    void AttackCastle()
    {
        CastleHealth castle = FindFirstObjectByType<CastleHealth>();
        if (castle != null)
            castle.TakeDamage(damage);
        Destroy(gameObject);
    }
}