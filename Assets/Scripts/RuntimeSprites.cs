using UnityEngine;

/// <summary>
/// Sprites generados por codigo y compartidos por todo el juego.
///
/// Se crean una sola vez y se reutilizan. Antes el circulo se generaba dentro de
/// TowerPlacer cada vez que arrancaba la escena, y como la textura se marca
/// HideAndDontSave (para que sobreviva al cambio de escena) nadie la liberaba:
/// cada reinicio de partida dejaba 256 KB huerfanos en memoria.
/// </summary>
public static class RuntimeSprites
{
    private static Sprite circle;

    /// <summary>
    /// Circulo blanco de 1 unidad de diametro: relleno translucido y borde
    /// marcado. Se tinta con SpriteRenderer.color y se escala al diametro.
    /// </summary>
    public static Sprite Circle
    {
        get
        {
            if (circle == null) circle = CreateCircle();
            return circle;
        }
    }

    static Sprite CreateCircle()
    {
        const int size = 256;
        const float fillAlpha = 0.16f;
        const float ringAlpha = 0.85f;
        const float ringWidth = 5f;      // en pixeles de la textura

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode   = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;   // circulo liso aunque se escale mucho

        float radius   = size * 0.5f - 1f;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);

                float alpha;
                if (d > radius)                  alpha = 0f;
                else if (d > radius - ringWidth) alpha = ringAlpha;
                else                             alpha = fillAlpha;

                // Suavizado de 1.5 px en el borde exterior.
                alpha *= Mathf.Clamp01((radius - d) / 1.5f + 0.5f);

                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;

        // Pixels per unit = tamano: el sprite mide 1 unidad y la escala es el diametro.
        Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, size, size),
                                      new Vector2(0.5f, 0.5f), size);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }
}
