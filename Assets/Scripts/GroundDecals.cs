using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manchas planas en el suelo (restos de los enemigos) que se desvanecen solas.
///
/// Tienen vida limitada y un tope: una oleada grande son decenas de muertes, y
/// si las manchas se quedaran para siempre el camino se llenaria y acabarian
/// tapando las llamas del fuego y la onda del hielo. Al llegar al tope, la mas
/// vieja se reaprovecha para la nueva.
///
/// Como SpriteParticles: un unico objeto las actualiza todas y los
/// SpriteRenderer se reciclan en vez de crearse y destruirse.
/// </summary>
public class GroundDecals : MonoBehaviour
{
    const int   MaxDecals = 40;
    const float Hold      = 6f;      // segundos a opacidad completa
    const float Fade      = 2f;      // segundos que tarda en desaparecer
    const float Pop       = 0.12f;   // crece un poco al aparecer, como un chapoteo
    const float GroundY   = 0.055f;  // sobre el camino, bajo llamas y ondas

    struct Decal
    {
        public SpriteRenderer renderer;
        public float age;
        public float size;
        public Color color;
    }

    private static GroundDecals instance;

    // En orden de aparicion: la primera es siempre la mas vieja.
    private readonly List<Decal> active = new List<Decal>(MaxDecals);
    private readonly Stack<SpriteRenderer> pool = new Stack<SpriteRenderer>(MaxDecals);

    /// <param name="position">Solo se usan X y Z: la mancha va siempre al suelo.</param>
    /// <param name="size">Ancho en unidades de mundo.</param>
    public static void Spawn(Vector3 position, Sprite sprite, Color color, float size)
    {
        if (sprite == null || size <= 0f) return;

        if (instance == null)
            instance = new GameObject("GroundDecals").AddComponent<GroundDecals>();

        instance.Add(position, sprite, color, size);
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

    void Add(Vector3 position, Sprite sprite, Color color, float size)
    {
        SpriteRenderer sr;

        if (active.Count >= MaxDecals)
        {
            // Tope alcanzado: la mas vieja se reutiliza para esta.
            sr = active[0].renderer;
            active.RemoveAt(0);
        }
        else
        {
            sr = Rent();
        }

        if (sr == null) sr = Rent();

        sr.sprite       = sprite;
        sr.sortingOrder = -12;   // bajo el fuego (-10), las ondas (-9) y todo lo demas

        // Tumbada en el suelo. La variedad sale de girarla en pasos de 90 grados y
        // voltearla: con angulos libres el pixel art se emborrona.
        sr.transform.SetPositionAndRotation(
            new Vector3(position.x, GroundY, position.z),
            Quaternion.Euler(90f, 90f * Random.Range(0, 4), 0f));
        sr.flipX = Random.value < 0.5f;
        sr.flipY = Random.value < 0.5f;

        // El sprite puede venir de fuera con cualquier tamano.
        float spriteWidth = Mathf.Max(0.0001f, sprite.bounds.size.x);

        Decal d = new Decal
        {
            renderer = sr,
            age      = 0f,
            size     = size / spriteWidth,
            color    = color,
        };

        Apply(ref d);
        active.Add(d);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            Decal d = active[i];

            if (d.renderer == null)
            {
                active.RemoveAt(i);
                continue;
            }

            d.age += dt;

            if (d.age >= Hold + Fade)
            {
                Return(d.renderer);
                active.RemoveAt(i);   // conserva el orden: la 0 sigue siendo la mas vieja
                continue;
            }

            Apply(ref d);
            active[i] = d;
        }
    }

    static void Apply(ref Decal d)
    {
        float pop = Mathf.Lerp(0.6f, 1f, Mathf.Clamp01(d.age / Pop));
        d.renderer.transform.localScale = new Vector3(d.size * pop, d.size * pop, 1f);

        float alpha = d.age <= Hold ? 1f : 1f - (d.age - Hold) / Fade;

        Color c = d.color;
        c.a *= Mathf.Clamp01(alpha);
        d.renderer.color = c;
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

        GameObject go = new GameObject("Decal");
        go.transform.SetParent(transform, false);
        return go.AddComponent<SpriteRenderer>();
    }

    void Return(SpriteRenderer sr)
    {
        sr.gameObject.SetActive(false);
        pool.Push(sr);
    }
}
