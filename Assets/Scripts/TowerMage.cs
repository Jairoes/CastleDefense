using UnityEngine;

public class TowerMage : MonoBehaviour
{
    [Header("Configuración")]
    public float range       = 5f;
    public float damage      = 40f;
    public float splashDamage = 10f;
    public float splashRadius = 3f;

    [Header("Proyectil")]
    public GameObject projectilePrefab;
    public Transform shootPoint;

    private GameObject target;
    private Animator magicianAnimator;

    void Start()
    {
        Animator[] animators = GetComponentsInChildren<Animator>();
        foreach (Animator a in animators)
        {
            if (a.gameObject.name == "magician_sprite")
                magicianAnimator = a;
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
        if (magicianAnimator == null) return;

        if (target == null)
        {
            magicianAnimator.speed = 0f;
            magicianAnimator.Play("magician_attack", 0, 0f);
            return;
        }

        magicianAnimator.speed = 1f;

        Vector3 dir = (target.transform.position - transform.position).normalized;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
            magicianAnimator.SetFloat("dirZ", 0f);
        else
            magicianAnimator.SetFloat("dirZ", dir.z);

        magicianAnimator.SetFloat("dirX", dir.x);

        SpriteRenderer sr = magicianAnimator.GetComponent<SpriteRenderer>();
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
            Debug.LogWarning("TowerMage no tiene proyectil asignado!");
            return;
        }

        Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position;
        GameObject proj  = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        MageProjectile mp = proj.GetComponent<MageProjectile>();

        if (mp != null)
            mp.SetTarget(target, damage, splashDamage, splashRadius);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}