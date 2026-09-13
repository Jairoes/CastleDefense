using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Estado visual de un boton de torre, estilo Plants vs Zombies:
/// - Precio abajo, en rojo si no te llega.
/// - Tras colocar la torre, una sombra que se va retirando hacia arriba segun
///   avanza la recarga, y un pequeno "pop" cuando vuelve a estar lista.
/// - Icono atenuado mientras no tengas cristales suficientes.
///
/// No sustituye al EventTrigger del boton: pulsar sigue llamando a
/// TowerPlacer.SelectTower, que es quien decide si deja seleccionar y muestra
/// el aviso. Por eso el boton NO se desactiva al no tener cristales: si lo
/// hiciera, pulsarlo no daria ningun aviso.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class TowerButtonUI : MonoBehaviour
{
    [Tooltip("0 arquero, 1 mago, 2 hielo, 3 canon, 4 fuego. Con -1 se deduce del " +
             "nombre del objeto: TowerBtn_01 -> 0, TowerBtn_05 -> 4.")]
    public int towerIndex = -1;

    [Header("Opcional: se crean solos si los dejas vacios")]
    public TextMeshProUGUI costText;
    public Image cooldownOverlay;

    [Header("Aspecto")]
    public Color affordableColor   = Color.white;
    public Color unaffordableColor = new Color(1f, 0.35f, 0.35f);
    public Color overlayColor      = new Color(0f, 0f, 0f, 0.65f);

    [Range(0f, 1f)]
    [Tooltip("Cuanto se oscurece el icono cuando no hay cristales suficientes.")]
    public float unaffordableDim = 0.5f;

    private static Sprite whiteSprite;

    private Image buttonImage;
    private Color buttonBaseColor;
    private Vector3 baseScale;

    private int lastCost = -1;
    private bool lastAffordable = true;
    private float lastFraction = -1f;
    private float popTimer;

    const float PopDuration = 0.22f;

    void Start()
    {
        if (towerIndex < 0)
            towerIndex = IndexFromName(name);

        if (towerIndex < 0 || towerIndex >= TowerPlacer.TowerCount)
        {
            Debug.LogWarning("TowerButtonUI: no pude deducir el indice de '" + name +
                             "'. Asigna Tower Index a mano (0-4).", this);
            enabled = false;
            return;
        }

        buttonImage = GetComponent<Image>();
        if (buttonImage != null) buttonBaseColor = buttonImage.color;

        baseScale = transform.localScale;

        // La fuente se toma ANTES de crear la etiqueta, del texto que ya tiene
        // el boton, para que el precio salga con tu PressStart2P.
        TMP_FontAsset font = FindExistingFont();

        if (cooldownOverlay == null) cooldownOverlay = CreateOverlay();
        if (costText == null)        costText        = CreateCostLabel(font);

        cooldownOverlay.enabled = false;
        Refresh(force: true);
    }

    void Update()
    {
        Refresh(force: false);
        AnimatePop();
    }

    void Refresh(bool force)
    {
        TowerPlacer placer = TowerPlacer.Instance;
        if (placer == null) return;

        // Precio y color: solo se reescribe al cambiar, porque tocar el texto de
        // TMP regenera su malla.
        int cost        = placer.GetCost(towerIndex);
        int crystals    = GameManager.Instance != null ? GameManager.Instance.crystals : 0;
        bool affordable = crystals >= cost;

        if (force || cost != lastCost)
        {
            if (costText != null) costText.text = cost.ToString();
            lastCost = cost;
        }

        if (force || affordable != lastAffordable)
        {
            if (costText != null)
                costText.color = affordable ? affordableColor : unaffordableColor;

            if (buttonImage != null)
                buttonImage.color = affordable
                    ? buttonBaseColor
                    : Color.Lerp(buttonBaseColor, Color.black, unaffordableDim);

            lastAffordable = affordable;
        }

        // Recarga: la sombra ocupa la fraccion que queda y se retira hacia
        // arriba, asi que el boton se "rellena" de color desde abajo.
        float fraction = placer.GetCooldownFraction(towerIndex);

        if (force || !Mathf.Approximately(fraction, lastFraction))
        {
            bool cooling = fraction > 0f;
            cooldownOverlay.enabled = cooling;
            if (cooling) cooldownOverlay.fillAmount = fraction;

            // Acaba de quedar lista: pequeno salto para que se note.
            if (!force && lastFraction > 0f && !cooling)
                popTimer = PopDuration;

            lastFraction = fraction;
        }
    }

    void AnimatePop()
    {
        if (popTimer <= 0f) return;

        popTimer -= Time.unscaledDeltaTime;
        float t = 1f - Mathf.Clamp01(popTimer / PopDuration);   // 0 -> 1

        // Sube hasta 1.15 a mitad y vuelve a su tamano.
        float bump = Mathf.Sin(t * Mathf.PI) * 0.15f;
        transform.localScale = baseScale * (1f + bump);

        if (popTimer <= 0f)
            transform.localScale = baseScale;
    }

    // ---------------------------------------------------------------------

    static int IndexFromName(string objectName)
    {
        Match m = Regex.Match(objectName, @"(\d+)\s*$");
        if (!m.Success) return -1;
        return int.Parse(m.Groups[1].Value) - 1;
    }

    TMP_FontAsset FindExistingFont()
    {
        TextMeshProUGUI existing = GetComponentInChildren<TextMeshProUGUI>(true);
        if (existing != null) return existing.font;

        if (GameManager.Instance != null && GameManager.Instance.crystalsText != null)
            return GameManager.Instance.crystalsText.font;

        return null;
    }

    Image CreateOverlay()
    {
        GameObject go = new GameObject("CooldownOverlay",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);

        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = go.GetComponent<Image>();
        img.sprite        = GetWhiteSprite();   // Filled ignora fillAmount sin sprite
        img.color         = overlayColor;
        img.type          = Image.Type.Filled;
        img.fillMethod    = Image.FillMethod.Vertical;
        img.fillOrigin    = (int)Image.OriginVertical.Top;
        img.raycastTarget = false;              // no robar el toque al boton

        return img;
    }

    TextMeshProUGUI CreateCostLabel(TMP_FontAsset font)
    {
        // Franja oscura abajo del boton para que el numero se lea sobre cualquier
        // icono.
        GameObject tag = new GameObject("CostTag",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        tag.transform.SetParent(transform, false);

        RectTransform tagRect = (RectTransform)tag.transform;
        tagRect.anchorMin        = new Vector2(0f, 0f);
        tagRect.anchorMax        = new Vector2(1f, 0f);
        tagRect.pivot            = new Vector2(0.5f, 0f);
        tagRect.anchoredPosition = new Vector2(0f, 2f);
        tagRect.sizeDelta        = new Vector2(-6f, 15f);

        Image tagImage = tag.GetComponent<Image>();
        tagImage.color         = new Color(0f, 0f, 0f, 0.6f);
        tagImage.raycastTarget = false;

        GameObject label = new GameObject("Cost",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        label.transform.SetParent(tag.transform, false);

        RectTransform labelRect = (RectTransform)label.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = label.GetComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;

        text.alignment        = TextAlignmentOptions.Center;
        text.raycastTarget    = false;
        text.enableAutoSizing = true;
        text.fontSizeMin      = 6f;
        text.fontSizeMax      = 11f;

        // El precio por encima de la sombra de recarga, siempre legible.
        tag.transform.SetAsLastSibling();

        return text;
    }

    static Sprite GetWhiteSprite()
    {
        if (whiteSprite != null) return whiteSprite;

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;

        whiteSprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        whiteSprite.hideFlags = HideFlags.HideAndDontSave;

        return whiteSprite;
    }
}
