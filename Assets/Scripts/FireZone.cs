using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Suelo en llamas que deja el proyectil de fuego: quema cada cierto tiempo a
/// todos los enemigos que esten dentro mientras dura.
///
/// Da a la torre de fuego un papel que ninguna otra tiene: dano en una zona a lo
/// largo del tiempo. Rinde en esquinas y en la entrada del castillo, donde los
/// enemigos se amontonan, y combina con el hielo: un enemigo ralentizado pasa el
/// doble de tiempo dentro de las llamas.
///
/// Las zonas NO se suman sobre un mismo enemigo (ver EnemyHealth.TryBurnTick):
/// si la torre dispara cada segundo, habria varias zonas solapadas en el mismo
/// sitio y el dano se multiplicaria sin control.
/// </summary>
public class FireZone : MonoBehaviour
{
    public struct Settings
    {
        public float radius;
        public float duration;
        public float tickInterval;
        public float damagePerTick;

        public Color fireColor;
        public Color emberColor;
        public float effectScale;
    }

    const float GroundY       = 0.065f;   // bajo la onda de impacto (0.07)
    const float FadeIn        = 0.15f;
    const float FadeOut       = 0.5f;
    const float FlameInterval = 0.12f;    // cada cuanto brotan llamas nuevas

    private Settings s;
    private Vector3 center;
    private float age;
    private float tickTimer;
    private float flameTimer;
    private SpriteRenderer patch;

    // Compartida entre zonas: evita asignar una lista en cada golpe de fuego.
    private static readonly List<EnemyHealth> hits = new List<EnemyHealth>(16);

    public static void Spawn(Vector3 impact, Settings settings)
    {
        if (settings.radius <= 0f || settings.duration <= 0f) return;

        settings.tickInterval = Mathf.Max(0.1f, settings.tickInterval);
        settings.effectScale  = Mathf.Max(0.1f, settings.effectScale);

        GameObject go = new GameObject("FireZone");
        go.transform.SetPositionAndRotation(
            new Vector3(impact.x, GroundY, impact.z),
            Quaternion.Euler(90f, 0f, 0f));                   // tumbado en el suelo

        FireZone zone = go.AddComponent<FireZone>();
        zone.s      = settings;
        zone.center = impact;   // a la altura de los enemigos, para buscarlos bien

        // La mancha que marca la zona durante toda su vida.
        zone.patch = go.AddComponent<SpriteRenderer>();
        zone.patch.sprite       = RuntimeSprites.Circle;
        zone.patch.sortingOrder = -10;
        float diameter = settings.radius * 2f;               // el sprite mide 1 unidad
        go.transform.localScale = new Vector3(diameter, diameter, 1f);

        // Primer golpe a mitad de intervalo: rapido pero sin pisar el impacto.
        zone.tickTimer = settings.tickInterval * 0.5f;
        zone.UpdatePatch();

        // La onda del impacto, igual que hielo y mago: marca el area al instante.
        AreaBurstEffect.Spawn(impact, settings.radius, settings.fireColor);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        age += dt;

        if (age >= s.duration)
        {
            Destroy(gameObject);
            return;
        }

        tickTimer -= dt;
        if (tickTimer <= 0f)
        {
            tickTimer += s.tickInterval;
            Burn();
        }

        flameTimer -= dt;
        if (flameTimer <= 0f)
        {
            flameTimer += FlameInterval;
            EmitFlames();
        }

        UpdatePatch();
    }

    void Burn()
    {
        EnemyRegistry.FindInRadius(center, s.radius, hits);

        for (int i = 0; i < hits.Count; i++)
        {
            EnemyHealth enemy = hits[i];

            // Si otra zona ya le quemo en este intervalo, esta no suma.
            if (!enemy.TryBurnTick(s.tickInterval)) continue;

            // Las chispas van antes del dano: si lo mata, Destroy se aplica al
            // final del frame y el cuerpo aun esta donde tiene que estar.
            EmberPuff(enemy);
            enemy.TakeDamage(s.damagePerTick);
        }
    }

    void UpdatePatch()
    {
        float envelope = 1f;
        if (age < FadeIn)                    envelope = age / FadeIn;
        else if (age > s.duration - FadeOut) envelope = (s.duration - age) / FadeOut;

        // Parpadeo leve, como brasas vivas.
        float flicker = 0.85f + 0.15f * Mathf.Sin(age * 14f);

        Color c = s.fireColor;
        c.a *= Mathf.Clamp01(envelope) * flicker;
        patch.color = c;
    }

    /// <summary>Llamas que brotan del suelo y se apagan al subir, y alguna brasa.</summary>
    void EmitFlames()
    {
        // En los ultimos instantes ya no brota nada nuevo: la zona se apaga.
        if (age > s.duration - FadeOut) return;

        float k = s.effectScale;

        SpriteParticles.Burst(center, new SpriteParticles.Settings
        {
            sprite       = RuntimeSprites.Puff,
            count        = 2,
            spawnRadius  = s.radius * 0.85f,
            speedMin     = 0f,    speedMax = 0.15f,
            riseMin      = 1.0f,  riseMax  = 1.8f,
            drag         = 0.5f,
            lifeMin      = 0.35f, lifeMax  = 0.6f,
            sizeStart    = 0.55f * k,
            sizeEnd      = 0.15f * k,   // se estrechan al subir, como una llama
            colorA       = WithAlpha(s.fireColor, 0.9f),
            colorB       = WithAlpha(Color.Lerp(s.fireColor, Color.red, 0.5f), 0.9f),
            sortingOrder = 5,
        });

        SpriteParticles.Burst(center, new SpriteParticles.Settings
        {
            sprite       = RuntimeSprites.Sparkle,
            count        = 1,
            spawnRadius  = s.radius * 0.8f,
            speedMin     = 0.1f,  speedMax = 0.4f,
            riseMin      = 1.5f,  riseMax  = 2.5f,
            drag         = 0.3f,
            lifeMin      = 0.5f,  lifeMax  = 0.9f,
            sizeStart    = 0.25f * k,
            sizeEnd      = 0.1f  * k,
            colorA       = s.emberColor,
            colorB       = Color.Lerp(s.emberColor, Color.white, 0.4f),
            sortingOrder = 6,
        });
    }

    /// <summary>Chispas sobre el enemigo que se quema, para ver a quien afecta.</summary>
    void EmberPuff(EnemyHealth enemy)
    {
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        Vector3 at = movement != null ? movement.BodyCenter : enemy.transform.position;

        SpriteParticles.Burst(at, new SpriteParticles.Settings
        {
            sprite       = RuntimeSprites.Sparkle,
            count        = 2,
            spawnRadius  = 0.2f * s.effectScale,
            speedMin     = 0.3f,  speedMax = 0.8f,
            riseMin      = 0.8f,  riseMax  = 1.4f,
            drag         = 1.5f,
            lifeMin      = 0.3f,  lifeMax  = 0.5f,
            sizeStart    = 0.3f  * s.effectScale,
            sizeEnd      = 0.1f  * s.effectScale,
            colorA       = s.emberColor,
            colorB       = s.fireColor,
            sortingOrder = 6,
        });
    }

    static Color WithAlpha(Color c, float a)
    {
        c.a = a;
        return c;
    }
}
