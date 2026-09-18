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
    private static Sprite puff;
    private static Sprite sparkle;

    /// <summary>
    /// Bocanada de humo en pixel art: nucleo opaco y borde semitransparente,
    /// 16x16 con filtro Point para que case con el resto del arte. Mide 1 unidad.
    /// </summary>
    public static Sprite Puff
    {
        get
        {
            if (puff == null) puff = CreatePuff();
            return puff;
        }
    }

    /// <summary>Destello en cruz de 7x7 en pixel art. Mide 1 unidad.</summary>
    public static Sprite Sparkle
    {
        get
        {
            if (sparkle == null) sparkle = CreateSparkle();
            return sparkle;
        }
    }

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

    static Sprite CreatePuff()
    {
        const int size = 16;
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);

                float alpha;
                if (d <= 4.5f)      alpha = 1f;      // nucleo
                else if (d <= 7.5f) alpha = 0.55f;   // borde
                else                alpha = 0f;

                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        return MakePixelSprite(pixels, size);
    }

    static Sprite CreateSparkle()
    {
        // Cruz con el centro engordado:
        //   . . . X . . .
        //   . . . X . . .
        //   . . X X X . .
        //   X X X X X X X
        //   . . X X X . .
        //   . . . X . . .
        //   . . . X . . .
        const int size = 7;
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = Mathf.Abs(x - 3);
                int dy = Mathf.Abs(y - 3);

                bool onCross  = dx == 0 || dy == 0;
                bool nearCore = dx <= 1 && dy <= 1;

                pixels[y * size + x] = new Color(1f, 1f, 1f, (onCross || nearCore) ? 1f : 0f);
            }
        }

        return MakePixelSprite(pixels, size);
    }

    static Sprite MakePixelSprite(Color[] pixels, int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode   = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;   // pixeles nitidos, como el resto del arte
        tex.SetPixels(pixels);
        tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;

        Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, size, size),
                                      new Vector2(0.5f, 0.5f), size);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }
}
