using UnityEngine;

public class TowerIce : MonoBehaviour
{
    [Header("Configuración")]
    public float range        = 5f;
    public float damage       = 15f;
    public float slowPercent  = 0.5f;
    public float slowDuration = 2f;
    public float slowRadius   = 2.5f;

    [Header("Proyectil")]
    public GameObject projectilePrefab;
    public Transform shootPoint;

    private GameObject target;
    private Animator snowAnimator;

    void Start()
    {
        Animator[] animators = GetComponentsInChildren<Animator>();
        foreach (Animator a in animators)
        {
            if (a.gameObject.name == "snow_sprite")
                snowAnimator = a;
        }
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
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                nearestEnemy = enemy;
            }
        }

        target = (nearestEnemy != null && shortestDistance <= range) ? nearestEnemy : null;
    }

    void UpdateAnimation()
    {
        if (snowAnimator == null) return;

        if (target == null)
        {
            snowAnimator.speed = 0f;
            snowAnimator.Play("snow_attack", 0, 0f);
            return;
        }

        snowAnimator.speed = 1f;

        Vector3 dir = (target.transform.position - transform.position).normalized;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
            snowAnimator.SetFloat("dirZ", 0f);
        else
            snowAnimator.SetFloat("dirZ", dir.z);

        snowAnimator.SetFloat("dirX", dir.x);

        SpriteRenderer sr = snowAnimator.GetComponent<SpriteRenderer>();
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
            Debug.LogWarning("TowerIce: no tiene proyectil asignado!");
            return;
        }

        Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position;
        GameObject proj  = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        IceProjectile ip = proj.GetComponent<IceProjectile>();

        if (ip != null)
            ip.SetTarget(target, damage, slowPercent, slowDuration, slowRadius);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}