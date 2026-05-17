using UnityEngine;

public class TowerCannon : MonoBehaviour
{
    [Header("Configuración")]
    public float range  = 6f;
    public float damage = 55f;

    [Header("Proyectil")]
    public GameObject projectilePrefab;
    public Transform shootPoint;

    private GameObject target;
    private Animator cannonAnimator;
    private bool firstShot     = true;
    private float noTargetTimer = 0f;
    private float rechargeTime  = 0f;

    void Start()
    {
        Animator[] animators = GetComponentsInChildren<Animator>();
        foreach (Animator a in animators)
        {
            if (a.gameObject.name == "cannon_sprite")
                cannonAnimator = a;
        }

        // Obtener duración del clip de ataque
        if (cannonAnimator != null)
        {
            AnimationClip[] clips = cannonAnimator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == "cannon_attack")
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
        if (cannonAnimator == null) return;

        if (target == null)
        {
            cannonAnimator.speed = 0f;
            cannonAnimator.Play("cannon_attack", 0, 0f);

            // Contar tiempo sin enemigos
            noTargetTimer += Time.deltaTime;
            if (noTargetTimer >= rechargeTime)
                firstShot = true;
            return;
        }

        // Hay target — resetear timer
        noTargetTimer = 0f;

        if (firstShot)
        {
            cannonAnimator.Play("cannon_attack", 0, 0.8f);
            firstShot = false;
        }

        cannonAnimator.speed = 1f;

        Vector3 dir = (target.transform.position - transform.position).normalized;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
            cannonAnimator.SetFloat("dirZ", 0f);
        else
            cannonAnimator.SetFloat("dirZ", dir.z);

        cannonAnimator.SetFloat("dirX", dir.x);

        SpriteRenderer sr = cannonAnimator.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (dir.x > 0.1f)
                sr.flipX = false;
            else if (dir.x < -0.1f)
                sr.flipX = true;
        }
    }

    public void Shoot()
    {
        if (target == null) return;

        if (projectilePrefab == null)
        {
            Debug.LogWarning("TowerCannon: no tiene proyectil asignado!");
            return;
        }

        Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position;
        GameObject proj  = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        CannonProjectile cp = proj.GetComponent<CannonProjectile>();

        if (cp != null)
            cp.SetTarget(target, damage);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}