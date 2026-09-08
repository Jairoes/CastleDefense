using UnityEngine;

/// <summary>
/// Barra de vida dibujada con dos quads (SpriteRenderer), sin uGUI.
///
/// La version anterior creaba un Canvas World Space + Slider + 3 Image por
/// enemigo. Un Canvas que se mueve fuerza un rebuild de layout cada frame
/// (Canvas.SendWillRenderCanvases, en el hilo principal y sin paralelizar), asi
/// que el coste se disparaba con el numero de enemigos en pantalla. Dos
/// SpriteRenderer no reconstruyen nada.
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Posicion y tamano (en unidades de mundo)")]
    [Tooltip("Altura sobre el origen del enemigo. Independiente de su escala.")]
    public float offsetY = 2.2f;
    public float width   = 2.0f;
    public float height  = 0.28f;

    [Header("Colores")]
    public Color backColor = new Color(0.15f, 0.04f, 0.04f, 1f);
    public Color fillColor = new Color(0.15f, 0.80f, 0.15f, 1f);

    [Tooltip("Debe quedar por encima del sprite del enemigo (que usa 0).")]
    public int sortingOrder = 100;

    // Sprite 1x1 blanco compartido por todas las barras del juego.
    private static Sprite sharedSprite;

    private Transform barRoot;
    private Transform fillPivot;
    private Transform camTransform;
    private float lastRatio = -1f;

    void Awake()
    {
        if (Camera.main != null)
            camTransform = Camera.main.transform;

        Build();
    }

    void Build()
    {
        barRoot = new GameObject("HealthBar").transform;
        barRoot.SetParent(transform, false);

        // Los prefabs de enemigo tienen escalas de raiz distintas (goblin 1.5,
        // orco 4...). Sin compensar, la barra del orco salia casi tres veces
        // mas grande que la del goblin. Anulando la escala heredada, offsetY,
        // width y height son unidades de mundo reales e iguales para todos.
        Vector3 parent = transform.lossyScale;
        barRoot.localScale = new Vector3(
            Mathf.Approximately(parent.x, 0f) ? 1f : 1f / parent.x,
            Mathf.Approximately(parent.y, 0f) ? 1f : 1f / parent.y,
            Mathf.Approximately(parent.z, 0f) ? 1f : 1f / parent.z);

        barRoot.localPosition = new Vector3(
            0f,
            Mathf.Approximately(parent.y, 0f) ? offsetY : offsetY / parent.y,
            0f);

        CreateQuad("Back", barRoot, backColor, sortingOrder)
            .localScale = new Vector3(width, height, 1f);

        // El relleno cuelga de un pivote en el borde izquierdo: escalar el
        // pivote en X lo encoge hacia la izquierda sin recolocar nada.
        fillPivot = new GameObject("FillPivot").transform;
        fillPivot.SetParent(barRoot, false);
        fillPivot.localPosition = new Vector3(-width * 0.5f, 0f, -0.01f);

        Transform fill = CreateQuad("Fill", fillPivot, fillColor, sortingOrder + 1);
        fill.localScale    = new Vector3(width, height, 1f);
        fill.localPosition = new Vector3(width * 0.5f, 0f, 0f);
    }

    Transform CreateQuad(string name, Transform parent, Color color, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = GetSharedSprite();
        sr.color        = color;
        sr.sortingOrder = order;

        return go.transform;
    }

    static Sprite GetSharedSprite()
    {
        if (sharedSprite != null) return sharedSprite;

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;

        sharedSprite = Sprite.Create(
            tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        sharedSprite.hideFlags = HideFlags.HideAndDontSave;

        return sharedSprite;
    }

    void LateUpdate()
    {
        if (camTransform == null)
        {
            if (Camera.main == null) return;
            camTransform = Camera.main.transform;
        }

        // Se escribe cada frame a proposito: si el enemigo llegara a rotar, la
        // barra debe seguir mirando a camara. Lo caro era el Camera.main de
        // cada frame, no esta asignacion.
        barRoot.rotation = camTransform.rotation;
    }

    public void UpdateBar(float currentHealth, float maxHealth)
    {
        if (fillPivot == null) return;

        float ratio = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;
        if (Mathf.Approximately(ratio, lastRatio)) return;

        lastRatio = ratio;
        fillPivot.localScale = new Vector3(ratio, 1f, 1f);
    }
}
