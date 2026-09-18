using UnityEngine;

/// <summary>
/// Onda de escarcha en el suelo cuando impacta el proyectil de hielo.
///
/// Se expande hasta el radio EXACTO de la ralentizacion y se desvanece, para que
/// el jugador vea que enemigos han quedado dentro. Primero crece rapido (la
/// mitad del tiempo) y despues se queda quieta a tamano completo mientras se
/// apaga: asi el borde final, que es el que importa, se llega a leer.
/// </summary>
public class FrostBurstEffect : MonoBehaviour
{
    const float Duration   = 0.5f;
    const float GrowPart   = 0.5f;    // fraccion del tiempo que dura el crecimiento
    const float StartScale = 0.35f;   // tamano inicial respecto al final
    const float GroundY    = 0.07f;   // encima del indicador de rango (0.06) y del camino

    private SpriteRenderer sr;
    private Color baseColor;
    private float diameter;
    private float elapsed;

    public static void Spawn(Vector3 impact, float radius, Color color)
    {
        if (radius <= 0f) return;

        GameObject go = new GameObject("FrostBurst");
        go.transform.SetPositionAndRotation(
            new Vector3(impact.x, GroundY, impact.z),
            Quaternion.Euler(90f, 0f, 0f));                  // tumbada en el suelo

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite       = RuntimeSprites.Circle;
        renderer.sortingOrder = -9;                          // bajo torres y enemigos

        FrostBurstEffect fx = go.AddComponent<FrostBurstEffect>();
        fx.sr        = renderer;
        fx.baseColor = color;
        fx.diameter  = radius * 2f;   // el sprite mide 1 unidad
        fx.Apply(0f);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float p = Mathf.Clamp01(elapsed / Duration);

        Apply(p);

        if (p >= 1f)
            Destroy(gameObject);
    }

    void Apply(float p)
    {
        // Crecimiento con frenada al final (ease-out).
        float g    = Mathf.Clamp01(p / GrowPart);
        float grow = 1f - (1f - g) * (1f - g) * (1f - g);
        float size = diameter * Mathf.Lerp(StartScale, 1f, grow);
        transform.localScale = new Vector3(size, size, 1f);

        // Opaca mientras crece, se apaga una vez alcanzado el radio.
        float fade = p <= GrowPart ? 1f : 1f - (p - GrowPart) / (1f - GrowPart);

        Color c = baseColor;
        c.a *= fade;
        sr.color = c;
    }
}
