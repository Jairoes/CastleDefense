using UnityEngine;

public class TowerMage : MonoBehaviour
{
    [Header("Configuración")]
    public float range        = 5f;
    public float damage       = 40f;
    public float splashDamage = 10f;
    public float splashRadius = 3f;

    [Header("Aspecto del hechizo")]
    [Tooltip("Verde veneno de la onda en el suelo y del humo.")]
    public Color poisonColor  = new Color(0.20f, 0.55f, 0.15f, 1f);

    [Tooltip("Color de los destellos que saltan en el impacto.")]
    public Color sparkleColor = new Color(0.75f, 1f, 0.35f, 1f);

    [Tooltip("Tamano del humo y los destellos. 1 = normal, 1.5 = mas grande.")]
    public float effectScale  = 1f;

    [Header("Proyectil")]
    public GameObject projectilePrefab;
    public Transform shootPoint;

    private GameObject target;
    private Animator magicianAnimator;
    private bool firstShoot     = true;
    private float noTargetTimer = 0f;
    private float rechargeTime  = 0f;

    void Start()
    {
        Animator[] animators = GetComponentsInChildren<Animator>();
        foreach (Animator a in animators)
        {
            if (a.gameObject.name == "magician_sprite")
                magicianAnimator = a;
        }

        if (magicianAnimator != null)
        {
            AnimationClip[] clips = magicianAnimator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == "magician_attack")
                {
                    rechargeTime = clip.length;
                    break;
                }
            }

            if (rechargeTime <= 0f)
                Debug.LogWarning("TowerMage: no encuentro el clip magician_attack en el Animator; rechargeTime se queda a 0 y la torre repetira la animacion en cada frame.", this);
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
        if (magicianAnimator == null) return;

        if (target == null)
        {
            magicianAnimator.speed = 0f;
            magicianAnimator.Play("magician_attack", 0, 0f);

            noTargetTimer += Time.deltaTime;
            if (noTargetTimer >= rechargeTime)
                firstShoot = true;
            return;
        }

        noTargetTimer = 0f;

        if (firstShoot)
        {
            magicianAnimator.Play("magician_attack", 0, 0.8f);
            firstShoot = false;
            noTargetTimer = 0f;
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
            mp.SetTarget(target, damage, splashDamage, splashRadius,
                         poisonColor, sparkleColor, effectScale);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}