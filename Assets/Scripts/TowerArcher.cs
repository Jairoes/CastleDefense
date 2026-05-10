using UnityEngine;

public class TowerArcher : MonoBehaviour
{
    [Header("Configuración")]
    public float range  = 7f;
    public float damage = 20f;

    [Header("Proyectil")]
    public GameObject projectilePrefab;

    private GameObject target;
    private Animator archerAnimator;
    private SpriteRenderer archerSprite;

    void Start()
    {
        // Busca el Animator del arquero (hijo archer_sprite)
        Animator[] animators = GetComponentsInChildren<Animator>();
        foreach (Animator a in animators)
        {
            if (a.gameObject.name == "archer_sprite")
                archerAnimator = a;
        }

        archerSprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        FindTarget();
        UpdateAnimation();
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

    void UpdateAnimation()
    {
        if (archerAnimator == null) return;
    
        if (target == null)
        {
            archerAnimator.speed = 0f;
            archerAnimator.Play("archer_idle", 0, 0f);
            return;
        }
    
        archerAnimator.speed = 1f;
        archerAnimator.SetBool("isAttacking", true);
    
        Vector3 dir = (target.transform.position - transform.position).normalized;
    
        archerAnimator.SetFloat("dirX", dir.x);
        archerAnimator.SetFloat("dirZ", dir.z);
    
        // Si dirX es más fuerte que dirZ → disparar de lado
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
        {
            archerAnimator.SetFloat("dirZ", 0f); // forzar animación de lado
        }
    
        // Flip según dirección X
        SpriteRenderer sr = archerAnimator.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (dir.x > 0.1f)
                sr.flipX = true;
            else if (dir.x < -0.1f)
                sr.flipX = false;
        }
    }

    public void Shoot()
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