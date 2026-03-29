using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Configuración")]
    public WaypointPath waypointPath;
    public float moveSpeed = 5f;
    public float damage    = 10f;

    [Header("Ataque al castillo")]
    public float attackRate     = 1f;   // ataques por segundo
    public float attackRange    = 2f;   // distancia para empezar a atacar

    private int currentWaypointIndex = 0;
    private bool isAttackingCastle   = false;
    private float attackCountdown    = 0f;
    private CastleHealth castle;

    // --- SISTEMA DE SLOW ---
    private float baseSpeed;
    private float slowTimer = 0f;
    private bool isSlowed   = false;

    void Start()
    {
        baseSpeed = moveSpeed;
        castle    = FindFirstObjectByType<CastleHealth>();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.gameOver) return;

        // Tick del slow
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
                RemoveSlow();
        }

        if (isAttackingCastle)
        {
            AttackCastle();
            return;
        }

        if (waypointPath == null) return;

        Transform targetWaypoint = waypointPath.GetWaypoint(currentWaypointIndex);
        Vector3 direction        = targetWaypoint.position - transform.position;
        direction.y              = 0f;

        transform.position += direction.normalized * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        Vector3 posEnemy    = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 posWaypoint = new Vector3(targetWaypoint.position.x, 0f, targetWaypoint.position.z);
        float distancia     = Vector3.Distance(posEnemy, posWaypoint);

        if (distancia < 0.2f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypointPath.GetWaypointCount())
            {
                // Llegó al castillo — se queda a atacar
                isAttackingCastle = true;
                attackCountdown   = 0f;
            }
        }
    }

    void AttackCastle()
    {
        if (castle == null) return;

        // Rotar hacia el castillo
        Vector3 dir = castle.transform.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), 10f * Time.deltaTime);

        // Atacar cada X segundos
        attackCountdown -= Time.deltaTime;
        if (attackCountdown <= 0f)
        {
            castle.TakeDamage(damage);
            attackCountdown = 1f / attackRate;
            Debug.Log(gameObject.name + " atacó el castillo!");
        }
    }

    public void ApplySlow(float slowPercent, float duration)
    {
        moveSpeed = baseSpeed * (1f - slowPercent);
        slowTimer = duration;
        isSlowed  = true;
    }

    void RemoveSlow()
    {
        moveSpeed = baseSpeed;
        isSlowed  = false;
        slowTimer = 0f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}