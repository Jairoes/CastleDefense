using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Particulas ligeras hechas con SpriteRenderer, para humo y destellos.
///
/// Por que no ParticleSystem: necesita un material de particulas, y el shader de
/// particulas de URP se busca por nombre. En el build del movil ese shader puede
/// no venir incluido si ningun material del proyecto lo usa, y entonces el
/// efecto no aparece o lanza una excepcion (el polvo del castillo ya va envuelto
/// en try/catch por si falla). Los SpriteRenderer usan el material por defecto
/// de sprites, el mismo que las barras de vida y el circulo de alcance, que ya
/// se ven en el movil.
///
/// Rendimiento: todas las particulas viven bajo un unico objeto que las mueve en
/// un solo Update, y los SpriteRenderer se reciclan en vez de crearse y
/// destruirse en cada impacto.
/// </summary>
public class SpriteParticles : MonoBehaviour
{
    /// <summary>Como es una rafaga. Todas las medidas en unidades de mundo.</summary>
    public struct Settings
    {
        public Sprite sprite;
        public int count;

        public float spawnRadius;        // dispersion inicial en el suelo
        public float speedMin, speedMax; // velocidad hacia fuera
        public float riseMin, riseMax;   // velocidad hacia arriba
        public float drag;               // frenado: mayor = se paran antes
        public float gravity;            // tira hacia abajo (gotas); 0 = flotan

        public float lifeMin, lifeMax;
        public float sizeStart, sizeEnd;

        public Color colorA, colorB;     // cada particula sale con un color entre ambos
        public int sortingOrder;
    }

    struct Particle
    {
        public SpriteRenderer renderer;
        public Vector3 velocity;
        public float age, life, drag, gravity;
        public float sizeStart, sizeEnd;
        public Color color;
    }

    // Tope de seguridad: si se dispara todo a la vez, se descartan las nuevas
    // antes que dejar que el movil se atasque.
    const int MaxParticles = 300;

    private static SpriteParticles instance;

    private readonly List<Particle> active = new List<Particle>(128);
    private readonly Stack<SpriteRenderer> pool = new Stack<SpriteRenderer>(128);
    private Transform camTransform;

    public static void Burst(Vector3 center, Settings settings)
    {
        if (settings.sprite == null || settings.count <= 0) return;
        GetInstance().Emit(center, settings);
    }

    static SpriteParticles GetInstance()
    {
        // Objeto de escena y no DontDestroyOnLoad: al reiniciar el nivel se
        // destruye junto con sus particulas y el siguiente impacto crea otro.
        if (instance == null)
            instance = new GameObject("SpriteParticles").AddComponent<SpriteParticles>();

        return instance;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic()
    {
        instance = null;
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    void Emit(Vector3 center, Settings s)
    {
        if (camTransform == null && Camera.main != null)
            camTransform = Camera.main.transform;

        // De cara a la camara, como los sprites de los enemigos. Sin giros
        // aleatorios: rotar pixel art en angulos libres lo emborrona.
        Quaternion facing = camTransform != null ? camTransform.rotation : Quaternion.identity;

        for (int i = 0; i < s.count; i++)
        {
            if (active.Count >= MaxParticles) return;

            Vector2 offset = Random.insideUnitCircle * s.spawnRadius;
            Vector2 dir    = offset.sqrMagnitude > 0.0001f
                ? offset.normalized
                : Random.insideUnitCircle.normalized;

            float speed = Random.Range(s.speedMin, s.speedMax);

            SpriteRenderer sr = Rent();
            sr.sprite       = s.sprite;
            sr.sortingOrder = s.sortingOrder;
            sr.transform.SetPositionAndRotation(
                center + new Vector3(offset.x, 0f, offset.y), facing);

            Particle p = new Particle
            {
                renderer  = sr,
                velocity  = new Vector3(dir.x * speed, Random.Range(s.riseMin, s.riseMax), dir.y * speed),
                age       = 0f,
                life      = Mathf.Max(0.05f, Random.Range(s.lifeMin, s.lifeMax)),
                drag      = s.drag,
                gravity   = s.gravity,
                sizeStart = s.sizeStart,
                sizeEnd   = s.sizeEnd,
                color     = Color.Lerp(s.colorA, s.colorB, Random.value),
            };

            Apply(ref p);
            active.Add(p);
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            Particle p = active[i];

            if (p.renderer == null)
            {
                RemoveAt(i);
                continue;
            }

            p.age += dt;
            if (p.age >= p.life)
            {
                Return(p.renderer);
                RemoveAt(i);
                continue;
            }

            p.velocity   *= Mathf.Max(0f, 1f - p.drag * dt);
            p.velocity.y -= p.gravity * dt;
            p.renderer.transform.position += p.velocity * dt;

            Apply(ref p);
            active[i] = p;
        }
    }

    static void Apply(ref Particle p)
    {
        float t = p.age / p.life;

        float size = Mathf.Lerp(p.sizeStart, p.sizeEnd, t);
        p.renderer.transform.localScale = new Vector3(size, size, 1f);

        // Aparece rapido y se apaga en la segunda mitad de su vida.
        float alpha = t < 0.15f ? t / 0.15f
                    : t > 0.45f ? 1f - (t - 0.45f) / 0.55f
                    : 1f;

        Color c = p.color;
        c.a *= alpha;
        p.renderer.color = c;
    }

    // Quitar cambiando por el ultimo: O(1). Seguro porque se recorre de atras
    // hacia delante y el ultimo ya se proceso en esta pasada.
    void RemoveAt(int i)
    {
        int last = active.Count - 1;
        active[i] = active[last];
        active.RemoveAt(last);
    }

    SpriteRenderer Rent()
    {
        while (pool.Count > 0)
        {
            SpriteRenderer pooled = pool.Pop();
            if (pooled != null)
            {
                pooled.gameObject.SetActive(true);
                return pooled;
            }
        }

        GameObject go = new GameObject("Particle");
        go.transform.SetParent(transform, false);
        return go.AddComponent<SpriteRenderer>();
    }

    void Return(SpriteRenderer sr)
    {
        sr.gameObject.SetActive(false);
        pool.Push(sr);
    }
}
