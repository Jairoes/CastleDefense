using UnityEngine;

public class TowerFire : MonoBehaviour
{
    [Header("Configuración")]
    public float range      = 6f;
    public float damage     = 25f;

    [Header("Suelo en llamas")]
    [Tooltip("Radio de la zona que arde donde cae el proyectil.")]
    public float zoneRadius        = 2f;

    [Tooltip("Segundos que arde el suelo.")]
    public float zoneDuration      = 2f;

    [Tooltip("Cada cuanto quema a los enemigos que estan dentro.")]
    public float zoneTickInterval  = 0.5f;

    [Tooltip("Dano de cada golpe de fuego. Las zonas solapadas no se suman sobre " +
             "el mismo enemigo, asi que el maximo por enemigo es " +
             "(duracion / intervalo) golpes.")]
    public float zoneDamagePerTick = 6f;

    [Header("Aspecto del fuego")]
    public Color fireColor   = new Color(1f, 0.45f, 0.1f, 1f);   // naranja: suelo y llamas
    public Color emberColor  = new Color(1f, 0.85f, 0.3f, 1f);   // amarillo: brasas

    [Tooltip("Tamano de las llamas y las brasas. 1 = normal.")]
    public float effectScale = 1f;

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

            if (rechargeTime <= 0f)
                Debug.LogWarning("TowerFire: no encuentro el clip fire_tower_idle en el Animator; rechargeTime se queda a 0 y la torre repetira la animacion en cada frame.", this);
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
            fp.SetTarget(target, damage, new FireZone.Settings
            {
                radius        = zoneRadius,
                duration      = zoneDuration,
                tickInterval  = zoneTickInterval,
                damagePerTick = zoneDamagePerTick,
                fireColor     = fireColor,
                emberColor    = emberColor,
                effectScale   = effectScale,
            });
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