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
    private static Sprite droplet;

    public const int RemainsVariants = 3;
    private static readonly Sprite[] splats = new Sprite[RemainsVariants];
    private static readonly Sprite[] boneBits = new Sprite[RemainsVariants];

    /// <summary>Gota de 3x3 en pixel art, para salpicaduras. Mide 1 unidad.</summary>
    public static Sprite Droplet
    {
        get
        {
            if (droplet == null) droplet = CreateDroplet();
            return droplet;
        }
    }

    /// <summary>
    /// Mancha de sangre en pixel art (24x24): un charco irregular y gotas sueltas
    /// alrededor. Hay varias variantes; cada una sale siempre igual (semilla fija).
    /// </summary>
    public static Sprite Splat(int variant)
    {
        int v = Mathf.Abs(variant) % RemainsVariants;
        if (splats[v] == null) splats[v] = CreateSplat(v);
        return splats[v];
    }

    /// <summary>Huesos rotos y polvo en pixel art (24x24), en varias variantes.</summary>
    public static Sprite BoneBits(int variant)
    {
        int v = Mathf.Abs(variant) % RemainsVariants;
        if (boneBits[v] == null) boneBits[v] = CreateBoneBits(v);
        return boneBits[v];
    }

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

    static Sprite CreateDroplet()
    {
        //   . X .
        //   X X X
        //   . X .
        const int size = 3;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                pixels[y * size + x] = new Color(1f, 1f, 1f, (x == 1 || y == 1) ? 1f : 0f);

        return MakePixelSprite(pixels, size);
    }

    static Sprite CreateSplat(int variant)
    {
        const int size = 24;
        Color[] pixels = new Color[size * size];
        System.Random rng = new System.Random(1000 + variant);   // misma forma siempre

        // Charco central: union de varios circulos desplazados, para que el borde
        // salga irregular y no un circulo perfecto.
        int blobs = 5 + rng.Next(2);
        for (int b = 0; b < blobs; b++)
        {
            float cx = 12f + (float)(rng.NextDouble() * 6.0 - 3.0);
            float cy = 12f + (float)(rng.NextDouble() * 6.0 - 3.0);
            float r  = 3f + (float)(rng.NextDouble() * 2.5);
            PaintDisc(pixels, size, cx, cy, r, 1f);
        }

        // Gotas sueltas alrededor.
        int drops = 4 + rng.Next(3);
        for (int d = 0; d < drops; d++)
        {
            double angle = rng.NextDouble() * Mathf.PI * 2.0;
            float dist   = 7.5f + (float)(rng.NextDouble() * 3.0);
            float r      = 0.8f + (float)(rng.NextDouble() * 0.9);
            PaintDisc(pixels, size,
                      12f + (float)System.Math.Cos(angle) * dist,
                      12f + (float)System.Math.Sin(angle) * dist, r, 1f);
        }

        return MakePixelSprite(pixels, size);
    }

    static Sprite CreateBoneBits(int variant)
    {
        const int size = 24;
        Color[] pixels = new Color[size * size];
        System.Random rng = new System.Random(2000 + variant);

        // Solo 4 direcciones (horizontal, vertical y las dos diagonales): con
        // lineas de 1 px, cualquier otro angulo sale dentado.
        int[,] dirs = { { 1, 0 }, { 0, 1 }, { 1, 1 }, { 1, -1 } };

        int bones = 3 + rng.Next(2);
        for (int b = 0; b < bones; b++)
        {
            int d   = rng.Next(4);
            int dx  = dirs[d, 0], dy = dirs[d, 1];
            int len = 5 + rng.Next(3);
            int x0  = 12 + rng.Next(-6, 5) - dx * len / 2;
            int y0  = 12 + rng.Next(-6, 5) - dy * len / 2;

            for (int i = 0; i <= len; i++)
                SetPixel(pixels, size, x0 + dx * i, y0 + dy * i, 1f);

            // Nudillos en los extremos, lo que hace que se lea como hueso.
            PaintDisc(pixels, size, x0 + 0.5f, y0 + 0.5f, 1.3f, 1f);
            PaintDisc(pixels, size, x0 + dx * len + 0.5f, y0 + dy * len + 0.5f, 1.3f, 1f);
        }

        // Polvo: motas sueltas semitransparentes.
        int specks = 8 + rng.Next(5);
        for (int s = 0; s < specks; s++)
            SetPixel(pixels, size, 3 + rng.Next(18), 3 + rng.Next(18), 0.55f);

        return MakePixelSprite(pixels, size);
    }

    static void PaintDisc(Color[] pixels, int size, float cx, float cy, float r, float alpha)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(cx - r)), maxX = Mathf.Min(size - 1, Mathf.CeilToInt(cx + r));
        int minY = Mathf.Max(0, Mathf.FloorToInt(cy - r)), maxY = Mathf.Min(size - 1, Mathf.CeilToInt(cy + r));

        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                if ((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy) <= r * r)
                    SetPixel(pixels, size, x, y, alpha);
    }

    static void SetPixel(Color[] pixels, int size, int x, int y, float alpha)
    {
        if (x < 0 || y < 0 || x >= size || y >= size) return;

        int i = y * size + x;
        // Nunca rebajar un pixel ya pintado mas opaco.
        if (pixels[i].a < alpha) pixels[i] = new Color(1f, 1f, 1f, alpha);
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
