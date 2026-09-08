using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Configuracion")]
    public WaypointPath waypointPath;
    public float moveSpeed = 5f;
    public float damage    = 10f;

    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private bool isAttackingCastle   = false;
    private CastleHealth castle;
    private Animator animator;
    private SpriteRotationFix spriteRotFix;

    // --- SISTEMA DE SLOW ---
    private float baseSpeed;
    private bool isSlowed   = false;
    private float slowTimer = 0f;

    void Start()
    {
        agent        = GetComponent<NavMeshAgent>();
        castle       = CastleHealth.Instance;
        baseSpeed    = moveSpeed;
        animator     = GetComponentInChildren<Animator>();
        spriteRotFix = GetComponentInChildren<SpriteRotationFix>();

        agent.speed            = moveSpeed;
        agent.stoppingDistance = 0.5f;
        agent.height           = 1f;
        agent.radius           = 0.15f;
        agent.updateRotation    = false;

        GoToNextWaypoint();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.gameOver) return;

        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
                RemoveSlow();
        }

        // Ya en el castillo: el dano lo disparan los AnimationEvent del clip de
        // ataque (ver DealDamage), asi que la cadencia la marca la duracion de
        // la animacion, no un temporizador.
        if (isAttackingCastle) return;

        if (waypointPath == null) return;

        if (agent.velocity.sqrMagnitude > 0.01f)
            UpdateAnimation(agent.velocity);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypointPath.GetWaypointCount())
                StartAttackingCastle();
            else
                GoToNextWaypoint();
        }
    }

    void StartAttackingCastle()
    {
        agent.isStopped   = true;
        isAttackingCastle = true;

        if (animator != null)
            animator.SetBool("isAttacking", true);
    }

    void UpdateAnimation(Vector3 velocity)
    {
        if (animator == null) return;

        Vector3 dir = velocity.normalized;

        animator.SetFloat("dirX", dir.x);
        animator.SetFloat("dirZ", dir.z);

        if (spriteRotFix != null)
            spriteRotFix.SetDirection(velocity);
    }

    void GoToNextWaypoint()
    {
        if (waypointPath == null) return;

        Transform wp = waypointPath.GetWaypoint(currentWaypointIndex);
        if (wp != null)
            agent.SetDestination(wp.position);
    }

    public void ApplySlow(float slowPercent, float duration)
    {
        moveSpeed   = baseSpeed * (1f - slowPercent);
        slowTimer   = duration;
        isSlowed    = true;
        agent.speed = moveSpeed;
    }

    void RemoveSlow()
    {
        moveSpeed   = baseSpeed;
        isSlowed    = false;
        slowTimer   = 0f;
        agent.speed = moveSpeed;
    }

    /// <summary>Lo llaman los AnimationEvent del clip de ataque de cada enemigo.</summary>
    public void DealDamage()
    {
        if (castle != null && isAttackingCastle)
            castle.TakeDamage(damage);
    }
}
