using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    [Header("Configuración")]
    public WaypointPath waypointPath;
    public float moveSpeed = 5f;
    public float damage    = 10f;

    [Header("Ataque al castillo")]
    public float attackRate  = 1f;
    public float attackRange = 2f;

    private NavMeshAgent agent;
    private int currentWaypointIndex  = 0;
    private bool isAttackingCastle    = false;
    private float attackCountdown     = 0f;
    private CastleHealth castle;

    // --- SISTEMA DE SLOW ---
    private float baseSpeed;
    private bool isSlowed   = false;
    private float slowTimer = 0f;

    void Start()
    {
        agent     = GetComponent<NavMeshAgent>();
        castle    = FindFirstObjectByType<CastleHealth>();
        baseSpeed = moveSpeed;

        // Configurar agente
        agent.speed       = moveSpeed;
        agent.stoppingDistance = 0.2f;
        agent.height      = 1f;
        agent.radius      = 0.15f; // separación entre enemigos

        // Ir al primer waypoint
        GoToNextWaypoint();
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

        // Verificar si llegó al waypoint actual
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypointPath.GetWaypointCount())
            {
                // Llegó al castillo
                agent.isStopped    = true;
                isAttackingCastle  = true;
                attackCountdown    = 0f;
            }
            else
            {
                GoToNextWaypoint();
            }
        }
    }

    void GoToNextWaypoint()
    {
        if (waypointPath == null) return;
        Transform wp = waypointPath.GetWaypoint(currentWaypointIndex);
        if (wp != null)
            agent.SetDestination(wp.position);
    }

    void AttackCastle()
    {
        if (castle == null) return;

        Vector3 dir = castle.transform.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), 10f * Time.deltaTime);

        attackCountdown -= Time.deltaTime;
        if (attackCountdown <= 0f)
        {
            castle.TakeDamage(damage);
            attackCountdown = 1f / attackRate;
        }
    }

    public void ApplySlow(float slowPercent, float duration)
    {
        moveSpeed    = baseSpeed * (1f - slowPercent);
        slowTimer    = duration;
        isSlowed     = true;
        agent.speed  = moveSpeed;
    }

    void RemoveSlow()
    {
        moveSpeed   = baseSpeed;
        isSlowed    = false;
        slowTimer   = 0f;
        agent.speed = moveSpeed;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}