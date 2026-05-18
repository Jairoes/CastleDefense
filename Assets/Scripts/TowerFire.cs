using UnityEngine;

public class TowerFire : MonoBehaviour
{
    [Header("Configuración")]
    public float range      = 6f;
    public float damage     = 25f;
    public float burnDamage = 10f;
    public float burnDelay  = 1f;

    [Header("Proyectil")]
    public GameObject projectilePrefab;
    public Transform shootPoint;

    private GameObject target;
    private Animator fireAnimator;
    private bool firstShoot     = true;
    private float noTargetTimer = 0f;
    private float rechargeTime  = 0f;

    void Start()
    {
        fireAnimator = GetComponentInChildren<Animator>();

        if (fireAnimator != null)
        {
            AnimationClip[] clips = fireAnimator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == "fire_tower_idle")
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
        if (fireAnimator == null) return;

        if (target == null)
        {
            fireAnimator.speed = 0f;
            fireAnimator.Play("fire_tower_idle", 0, 0f);

            noTargetTimer += Time.deltaTime;
            if (noTargetTimer >= rechargeTime)
                firstShoot = true;
            return;
        }

        noTargetTimer = 0f;

        if (firstShoot)
        {
            fireAnimator.Play("fire_tower_idle", 0, 0.8f);
            firstShoot = false;
            noTargetTimer = 0f;
        }

        fireAnimator.speed = 1f;
    }

    public void Shoot()
    {
        if (target == null) return;

        if (projectilePrefab == null)
        {
            Debug.LogWarning("TowerFire: no tiene proyectil asignado!");
            return;
        }

        Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position;
        GameObject proj  = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        FireProjectile fp = proj.GetComponent<FireProjectile>();

        if (fp != null)
            fp.SetTarget(target, damage, burnDamage, burnDelay);
        else
            Debug.LogWarning("FireProjectile component no encontrado!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
    public void ResetTimer ()
    {
        noTargetTimer = 0f;
    }
}