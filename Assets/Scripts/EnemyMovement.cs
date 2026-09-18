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

    // Sprite del cuerpo y su color original, para el tinte de congelado.
    private SpriteRenderer bodyRenderer;
    private Color bodyBaseColor = Color.white;

    void Start()
    {
        agent        = GetComponent<NavMeshAgent>();
        castle       = CastleHealth.Instance;
        baseSpeed    = moveSpeed;
        animator     = GetComponentInChildren<Animator>();
        spriteRotFix = GetComponentInChildren<SpriteRotationFix>();

        // El cuerpo es el sprite que lleva SpriteRotationFix. No vale
        // GetComponentInChildren<SpriteRenderer>(): la barra de vida tambien
        // crea SpriteRenderer hijos y podria tintarse ella en lugar del enemigo.
        if (spriteRotFix != null)
            bodyRenderer = spriteRotFix.GetComponent<SpriteRenderer>();
        if (bodyRenderer != null)
            bodyBaseColor = bodyRenderer.color;

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

    /// <summary>
    /// Ralentiza al enemigo: se mueve mas lento y tambien ataca mas lento.
    ///
    /// Lo segundo sale gratis por como esta montado el ataque: el dano al
    /// castillo lo disparan los AnimationEvent del clip, asi que frenar el
    /// Animator alarga el tiempo entre golpes en la misma proporcion. De paso,
    /// la animacion de andar deja de ir a velocidad normal mientras el enemigo
    /// avanza despacio, que era lo que hacia parecer que patinaba.
    /// </summary>
    public void ApplySlow(float slowPercent, float duration)
    {
        ApplySlow(slowPercent, duration, Color.white);   // sin tinte
    }

    public void ApplySlow(float slowPercent, float duration, Color tint)
    {
        // Tinte multiplicativo sobre el color original del sprite: con blanco no
        // cambia nada, con celeste los tonos claros viran a azul helado.
        if (bodyRenderer != null)
            bodyRenderer.color = bodyBaseColor * tint;

        moveSpeed   = baseSpeed * (1f - slowPercent);
        slowTimer   = duration;
        isSlowed    = true;
        agent.speed = moveSpeed;

        if (animator != null)
        {
            // Nunca 0: con el Animator parado no llegaria ningun AnimationEvent
            // y el enemigo se quedaria congelado para siempre delante del
            // castillo sin llegar a golpear.
            animator.speed = Mathf.Max(0.05f, 1f - slowPercent);
        }
    }

    void RemoveSlow()
    {
        moveSpeed   = baseSpeed;
        isSlowed    = false;
        slowTimer   = 0f;
        agent.speed = moveSpeed;

        if (animator != null)
            animator.speed = 1f;

        if (bodyRenderer != null)
            bodyRenderer.color = bodyBaseColor;
    }

    /// <summary>Lo llaman los AnimationEvent del clip de ataque de cada enemigo.</summary>
    public void DealDamage()
    {
        if (castle != null && isAttackingCastle)
            castle.TakeDamage(damage);
    }
}
