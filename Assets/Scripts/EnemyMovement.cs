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
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private SpriteRotationFix spriteRotFix;

    // --- SISTEMA DE SLOW ---
    private float baseSpeed;
    private bool isSlowed   = false;
    private float slowTimer = 0f;

    void Start()
    {
        agent        = GetComponent<NavMeshAgent>();
        castle       = FindFirstObjectByType<CastleHealth>();
        baseSpeed    = moveSpeed;
        animator     = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRotFix = GetComponentInChildren<SpriteRotationFix>(); // ← una sola t

        agent.speed            = moveSpeed;
        agent.stoppingDistance = 0.5f;
        agent.height           = 1f;
        agent.radius           = 0.15f;
        agent.updateRotation = false;

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
            if (animator != null)
                animator.SetBool("isAttacking", true);
            AttackCastle();
            return;
        }

        if (waypointPath == null) return;

        // Actualizar animación según dirección del agente
        if (agent.velocity.magnitude > 0.1f)
            UpdateAnimation(agent.velocity);

        // Verificar si llegó al waypoint actual
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypointPath.GetWaypointCount())
            {
                agent.isStopped   = true;
                isAttackingCastle = true;
                attackCountdown   = 0f;
            }
            else
            {
                GoToNextWaypoint();
            }
        }
    }

    void UpdateAnimation(Vector3 velocity)
    {
        if (animator == null) return;

        Vector3 dir = velocity.normalized;

        animator.SetFloat("dirX", dir.x);
        animator.SetFloat("dirZ", dir.z);

        // Flip del sprite según dirección X
        if (spriteRenderer != null)
            spriteRotFix.SetDirection(velocity);
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

    }

    public void ApplySlow(float slowPercent, float duration)
    {
        moveSpeed   = baseSpeed * (1f - slowPercent);
        slowTimer   = duration;
        isSlowed    = true;
        agent.speed = moveSpeed;
    }

    public void DealDamage()
    {
        if (castle != null && isAttackingCastle)
            castle.TakeDamage(damage);
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