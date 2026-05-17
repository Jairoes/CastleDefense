using UnityEngine;

public class TowerArcher : MonoBehaviour
{
    [Header("Configuración")]
    public float range  = 7f;
    public float damage = 20f;

    [Header("Proyectil")]
    public GameObject projectilePrefab;

    [Header("Punto de disparo")]
    public Transform shootPoint;

    private GameObject target;
    private Animator archerAnimator;
    private bool firstShoot     = true;
    private float noTargetTimer = 0f;
    private float rechargeTime  = 0f;

    void Start()
    {
        Animator[] animators = GetComponentsInChildren<Animator>();
        foreach (Animator a in animators)
        {
            if (a.gameObject.name == "archer_sprite")
                archerAnimator = a;
        }

        // Obtener duración del clip
        if (archerAnimator != null)
        {
            AnimationClip[] clips = archerAnimator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == "archer_idle")
                {
                    rechargeTime = clip.length;
                    break;
                }
            }
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
        if (archerAnimator == null) return;

        if (target == null)
        {
            archerAnimator.speed = 0f;
            archerAnimator.Play("archer_idle", 0, 0f);

            noTargetTimer += Time.deltaTime;
            if (noTargetTimer >= rechargeTime)
                firstShoot = true;
            return;
        }

        noTargetTimer = 0f;

        if (firstShoot)
        {
            archerAnimator.Play("archer_idle", 0, 0.8f);
            firstShoot = false;
        }

        archerAnimator.speed = 1f;
        archerAnimator.SetBool("isAttacking", true);

        Vector3 dir = (target.transform.position - transform.position).normalized;

        archerAnimator.SetFloat("dirX", dir.x);
        archerAnimator.SetFloat("dirZ", dir.z);

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
            archerAnimator.SetFloat("dirZ", 0f);

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

        Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position;
        GameObject proj  = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
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