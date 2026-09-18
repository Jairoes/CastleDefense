using System.Collections.Generic;
using UnityEngine;

public class MageProjectile : MonoBehaviour
{
    private GameObject target;
    private float damage;
    private float splashDamage;
    private float splashRadius;
    private float speed = 18f;

    private Color poisonColor  = Color.green;
    private Color sparkleColor = Color.white;
    private float effectScale  = 1f;

    // Compartida entre explosiones: evita asignar una lista en cada impacto.
    private static readonly List<EnemyHealth> splashHits = new List<EnemyHealth>(16);

    public void SetTarget(GameObject _target, float _damage, float _splashDamage, float _splashRadius,
                          Color _poisonColor, Color _sparkleColor, float _effectScale)
    {
        target       = _target;
        damage       = _damage;
        splashDamage = _splashDamage;
        splashRadius = _splashRadius;
        poisonColor  = _poisonColor;
        sparkleColor = _sparkleColor;
        effectScale  = Mathf.Max(0.1f, _effectScale);
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction       = target.transform.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            Explode();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
    }

    void Explode()
    {
        Vector3 impact = transform.position;

        EnemyHealth primary = target.GetComponent<EnemyHealth>();
        if (primary != null)
        {
            PuffOn(primary);
            primary.TakeDamage(damage);
        }

        // Onda con el mismo radio con el que se buscan los enemigos justo
        // debajo: lo que el jugador ve es exactamente lo que recibe dano.
        AreaBurstEffect.Spawn(impact, splashRadius, poisonColor);
        SpriteParticles.Burst(impact, SmokeCloud());
        SpriteParticles.Burst(impact, Sparkles());

        // Antes: Physics.OverlapSphere sin mascara de capas, que devolvia todos
        // los colliders del radio (terreno, torres, decoracion) para filtrarlos
        // despues por tag. El registro da directamente los enemigos.
        EnemyRegistry.FindInRadius(impact, splashRadius, splashHits);

        for (int i = 0; i < splashHits.Count; i++)
        {
            if (splashHits[i] == primary) continue;

            // La bocanada va antes del dano: si lo mata, Destroy se aplica al
            // final del frame y el cuerpo aun esta donde tiene que estar.
            PuffOn(splashHits[i]);
            splashHits[i].TakeDamage(splashDamage);
        }

        Destroy(gameObject);
    }

    /// <summary>Pequena bocanada sobre un enemigo alcanzado, para ver a quien le dio.</summary>
    void PuffOn(EnemyHealth enemy)
    {
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        Vector3 at = movement != null ? movement.BodyCenter : enemy.transform.position;

        SpriteParticles.Burst(at, new SpriteParticles.Settings
        {
            sprite       = RuntimeSprites.Puff,
            count        = 3,
            spawnRadius  = 0.25f * effectScale,
            speedMin     = 0.1f,  speedMax = 0.3f,
            riseMin      = 0.6f,  riseMax  = 1.0f,
            drag         = 1f,
            lifeMin      = 0.5f,  lifeMax  = 0.8f,
            sizeStart    = 0.4f * effectScale,
            sizeEnd      = 0.8f * effectScale,
            colorA       = WithAlpha(poisonColor, 0.85f),
            colorB       = WithAlpha(Color.Lerp(poisonColor, sparkleColor, 0.35f), 0.85f),
            sortingOrder = 5,
        });
    }

    /// <summary>Nube de humo toxico que se abre desde el punto de impacto y sube.</summary>
    SpriteParticles.Settings SmokeCloud()
    {
        return new SpriteParticles.Settings
        {
            sprite       = RuntimeSprites.Puff,
            count        = 10,
            spawnRadius  = splashRadius * 0.35f,
            speedMin     = 0.3f,  speedMax = 0.9f,
            riseMin      = 0.4f,  riseMax  = 1.0f,
            drag         = 1.5f,
            lifeMin      = 0.7f,  lifeMax  = 1.1f,
            sizeStart    = 0.7f * effectScale,
            sizeEnd      = 1.4f * effectScale,
            colorA       = WithAlpha(Color.Lerp(poisonColor, Color.black, 0.35f), 0.85f),
            colorB       = WithAlpha(poisonColor, 0.85f),
            sortingOrder = 5,
        };
    }

    /// <summary>Destellos que saltan rapido hacia fuera y se apagan.</summary>
    SpriteParticles.Settings Sparkles()
    {
        return new SpriteParticles.Settings
        {
            sprite       = RuntimeSprites.Sparkle,
            count        = 8,
            spawnRadius  = 0.2f,
            speedMin     = 1.5f,  speedMax = 3.0f,
            riseMin      = 0.5f,  riseMax  = 1.5f,
            drag         = 3f,
            lifeMin      = 0.3f,  lifeMax  = 0.55f,
            sizeStart    = 0.45f * effectScale,
            sizeEnd      = 0.15f * effectScale,
            colorA       = sparkleColor,
            colorB       = Color.Lerp(sparkleColor, Color.white, 0.5f),
            sortingOrder = 6,   // por encima del humo
        };
    }

    static Color WithAlpha(Color c, float a)
    {
        c.a = a;
        return c;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}
