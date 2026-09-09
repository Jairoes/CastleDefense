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

            if (rechargeTime <= 0f)
                Debug.LogWarning("TowerCannon: no encuentro el clip cannon_attack en el Animator; rechargeTime se queda a 0 y la torre repetira la animacion en cada frame.", this);
        }
    }

    void Update()
    {
        FindTarget();
        UpdateAnimation();
    }

    void FindTarget()
    {
        // Antes esto hacia FindGameObjectsWithTag: un barrido completo de la
        // escena y un array nuevo, por torre y por frame. El registro ya
        // mantiene la lista de enemigos vivos.
        target = EnemyRegistry.FindNearest(transform.position, range);
    }

    void UpdateAnimation()
    {
        if (cannonAnimator == null) return;
    
        if (target == null)
        {
            cannonAnimator.speed = 0f;
            noTargetTimer += Time.deltaTime;
            if (noTargetTimer >= rechargeTime)
            {
                firstShot = true;
                cannonAnimator.Play("cannon_attack", 0, 0f);
            }
            return;
        }
    
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

    public void ResetTimer()
    {
        noTargetTimer = 0f;
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