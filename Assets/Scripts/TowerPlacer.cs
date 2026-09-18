using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;

public class TowerPlacer : MonoBehaviour
{
    public static TowerPlacer Instance;

    [Header("Torres disponibles")]
    public GameObject archerTowerPrefab;
    public GameObject mageTowerPrefab;
    public GameObject iceTowerPrefab;
    public GameObject cannonTowerPrefab;
    public GameObject fireTowerPrefab;

    [Header("Costos")]
    public int archerCost = 50;
    public int mageCost   = 70;
    public int iceCost    = 70;
    public int cannonCost = 90;
    public int fireCost   = 80;

    [Header("Recarga tras colocar (segundos)")]
    [Tooltip("Como en PvZ: al colocar una torre, su boton queda bloqueado este " +
             "tiempo. Cuanto mejor la torre, mas larga la espera.")]
    public float archerCooldown = 4f;
    public float mageCooldown   = 10f;
    public float iceCooldown    = 8f;
    public float cannonCooldown = 14f;
    public float fireCooldown   = 10f;

    public const int TowerCount = 5;

    // Momento (en Time.time) en que cada torre vuelve a estar disponible.
    // Time.time se congela con timeScale = 0, asi que la recarga se pausa sola
    // en Game Over o victoria.
    private readonly float[] cooldownEnd = new float[TowerCount];
    private int selectedIndex = -1;

    [Header("Radio de ataque al colocar")]
    [Tooltip("Opcional: tu propio dibujo de circulo. Vacio = se genera uno.")]
    public Sprite rangeSprite;
    public Color rangeValidColor   = new Color(1f, 1f, 1f, 0.9f);
    public Color rangeInvalidColor = new Color(1f, 0.25f, 0.25f, 0.9f);

    [Tooltip("Altura del circulo. Justo por encima del camino (y 0.02) para que " +
             "no parpadee al coincidir con el suelo.")]
    public float rangeHeight = 0.06f;

    private GameObject rangeIndicator;
    private SpriteRenderer rangeRenderer;
    private float selectedRange;

    private GameObject selectedTowerPrefab;
    private GameObject towerPreview;
    private int selectedCost = 0;
    private bool isPlacing   = false;
    private bool isDragging  = false;
    private bool isShaking   = false;

    private Color previewColor = new Color(1f, 1f, 1f, 0.5f);

    private InputAction pressAction;
    private InputAction releaseAction;
    private Vector3 lastWorldPos;

    void Awake()
    {
        Instance = this;

        pressAction   = new InputAction("Press",   binding: "<Pointer>/press");
        releaseAction = new InputAction("Release", binding: "<Pointer>/press");

        pressAction.started    += OnTouchStart;
        releaseAction.canceled += OnTouchEnd;
    }

    void OnEnable()
    {
        pressAction.Enable();
        releaseAction.Enable();
    }

    void OnDisable()
    {
        pressAction.Disable();
        releaseAction.Disable();
    }

    void OnDestroy()
    {
        pressAction.started    -= OnTouchStart;
        releaseAction.canceled -= OnTouchEnd;
    }

    void Update()
    {
        if (!isPlacing || !isDragging) return;

        if (IsPointerOverUI())
        {
            if (towerPreview != null && !isShaking)
                towerPreview.transform.position = new Vector3(0, -100f, 0);
            return;
        }

        Vector3 worldPos = GetPointerWorldPosition();
        worldPos.y = 2f;
        lastWorldPos = worldPos; // ← guardar última posición

        if (towerPreview != null && !isShaking)
            towerPreview.transform.position = worldPos;
    }

    void OnTouchStart(InputAction.CallbackContext context)
    {
        if (!isPlacing) return;
        if (isDragging) return; // ya está arrastrando desde el botón

        // Si el toque empezó sobre la UI, no hacer nada aquí
        if (IsPointerOverUI()) return;

        // Toque en el mapa (modo tap) → empezar a seguir el dedo
        isDragging = true;

        Vector3 worldPos = GetPointerWorldPosition();
        worldPos.y = 2f;
        if (towerPreview != null)
            towerPreview.transform.position = worldPos;
    }

    void OnTouchEnd(InputAction.CallbackContext context)
    {
        if (!isPlacing) return;
        if (!isDragging) return;
    
        if (IsPointerOverUI())
        {
            isDragging = false;
            return;
        }
    
        isDragging = false;
    
        // Usar la última posición guardada durante el arrastre
        if (IsValidPlacement(lastWorldPos))
            PlaceTower(lastWorldPos);
        else
            StartCoroutine(ShakePreview());
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return EventSystem.current.IsPointerOverGameObject(
                Touchscreen.current.primaryTouch.touchId.ReadValue());

        return EventSystem.current.IsPointerOverGameObject();
    }

    bool IsValidPlacement(Vector3 position)
    {
        if (towerPreview == null) return false;

        BoxCollider col = towerPreview.GetComponent<BoxCollider>();
        if (col == null) return false;

        Vector3 center = position + col.center;
        Vector3 halfExtents = new Vector3(
            col.size.x * towerPreview.transform.localScale.x * 0.5f,
            col.size.y * towerPreview.transform.localScale.y * 0.5f,
            col.size.z * towerPreview.transform.localScale.z * 0.5f
        );

        int pathLayer     = 1 << LayerMask.NameToLayer("Path");
        int obstacleLayer = 1 << LayerMask.NameToLayer("Obstacle");
        int blockedMask   = pathLayer | obstacleLayer;

        if (Physics.CheckBox(center, halfExtents, Quaternion.identity, blockedMask))
            return false;

        int placementMask = 1 << LayerMask.NameToLayer("PlacementZone");

        Vector3[] corners = new Vector3[]
        {
            center + new Vector3( halfExtents.x, 0,  halfExtents.z),
            center + new Vector3(-halfExtents.x, 0,  halfExtents.z),
            center + new Vector3( halfExtents.x, 0, -halfExtents.z),
            center + new Vector3(-halfExtents.x, 0, -halfExtents.z),
        };

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 checkBox = new Vector3(0.1f, 50f, 0.1f);
            if (!Physics.CheckBox(corners[i], checkBox, Quaternion.identity, placementMask))
                return false;
        }

        return true;
    }

    // ---- Consultas por indice (las usan los botones de la UI) ----

    public GameObject GetPrefab(int towerIndex)
    {
        switch (towerIndex)
        {
            case 0: return archerTowerPrefab;
            case 1: return mageTowerPrefab;
            case 2: return iceTowerPrefab;
            case 3: return cannonTowerPrefab;
            case 4: return fireTowerPrefab;
            default: return null;
        }
    }

    public int GetCost(int towerIndex)
    {
        switch (towerIndex)
        {
            case 0: return archerCost;
            case 1: return mageCost;
            case 2: return iceCost;
            case 3: return cannonCost;
            case 4: return fireCost;
            default: return 0;
        }
    }

    public float GetCooldownDuration(int towerIndex)
    {
        switch (towerIndex)
        {
            case 0: return archerCooldown;
            case 1: return mageCooldown;
            case 2: return iceCooldown;
            case 3: return cannonCooldown;
            case 4: return fireCooldown;
            default: return 0f;
        }
    }

    public float GetCooldownRemaining(int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= TowerCount) return 0f;
        return Mathf.Max(0f, cooldownEnd[towerIndex] - Time.time);
    }

    /// <summary>1 recien colocada, 0 lista para usar.</summary>
    public float GetCooldownFraction(int towerIndex)
    {
        float duration = GetCooldownDuration(towerIndex);
        if (duration <= 0f) return 0f;
        return Mathf.Clamp01(GetCooldownRemaining(towerIndex) / duration);
    }

    public void SelectTower(int towerIndex)
    {
        GameObject prefab = GetPrefab(towerIndex);
        int cost          = GetCost(towerIndex);

        if (prefab == null) return;

        float remaining = GetCooldownRemaining(towerIndex);
        if (remaining > 0f)
        {
            ShowToast("Recargando... " + Mathf.CeilToInt(remaining) + "s");
            return;
        }

        int faltan = cost - GameManager.Instance.crystals;
        if (faltan > 0)
        {
            ShowToast("Cristales insuficientes (faltan " + faltan + ")");
            return;
        }

        CancelPlacement();

        selectedTowerPrefab = prefab;
        selectedCost        = cost;
        selectedIndex       = towerIndex;
        isPlacing           = true;
        isDragging          = true; // permite arrastre inmediato desde el botón

        towerPreview = Instantiate(prefab, new Vector3(0, -100f, 0), Quaternion.identity);
        DisableAttackScripts(towerPreview);
        SetPreviewColor(towerPreview, previewColor);

        selectedRange = GetTowerRange(prefab);
    }

    void PlaceTower(Vector3 position)
    {
        if (GameManager.Instance.SpendCrystals(selectedCost))
        {
            Destroy(towerPreview);
            position.y = 2f;
            GameObject newTower = Instantiate(selectedTowerPrefab, position, Quaternion.identity);
            newTower.layer = LayerMask.NameToLayer("Obstacle");

            // La recarga empieza al COLOCAR, no al seleccionar: si el jugador
            // cancela o no encuentra sitio valido, no pierde el turno.
            if (selectedIndex >= 0 && selectedIndex < TowerCount)
                cooldownEnd[selectedIndex] = Time.time + GetCooldownDuration(selectedIndex);

            isPlacing    = false;
            isDragging   = false;
            towerPreview = null;
        }
    }

    void ShowToast(string message)
    {
        if (GameUI.Instance != null)
            GameUI.Instance.ShowToast(message);
    }

    // ---------------------------------------------------------------------
    // Radio de ataque mientras se coloca
    // ---------------------------------------------------------------------

    // Va en LateUpdate a proposito: Update ya ha movido el preview siguiendo el
    // dedo, y aqui solo se lee donde quedo. El manejo de toques no se toca.
    void LateUpdate()
    {
        UpdateRangeIndicator();
    }

    void UpdateRangeIndicator()
    {
        // El preview se aparca en y = -100 cuando el dedo esta sobre la UI o
        // antes de empezar a arrastrar: ahi no hay nada que mostrar.
        bool visible = isPlacing
                    && towerPreview != null
                    && selectedRange > 0f
                    && towerPreview.transform.position.y > -50f;

        if (!visible)
        {
            if (rangeIndicator != null && rangeIndicator.activeSelf)
                rangeIndicator.SetActive(false);
            return;
        }

        EnsureRangeIndicator();

        Vector3 p = towerPreview.transform.position;
        rangeIndicator.transform.position = new Vector3(p.x, rangeHeight, p.z);

        // El sprite puede venir de fuera con cualquier tamano: se escala segun
        // lo que mide de verdad para que el diametro sea exactamente 2 * range.
        float spriteWidth = rangeRenderer.sprite.bounds.size.x;
        float scale = (selectedRange * 2f) / Mathf.Max(0.0001f, spriteWidth);
        rangeIndicator.transform.localScale = new Vector3(scale, scale, 1f);

        // Rojo si ahi no se puede construir: el jugador lo sabe antes de soltar.
        rangeRenderer.color = IsValidPlacement(p) ? rangeValidColor : rangeInvalidColor;

        if (!rangeIndicator.activeSelf)
            rangeIndicator.SetActive(true);
    }

    void EnsureRangeIndicator()
    {
        if (rangeIndicator != null) return;

        // Objeto aparte y no hijo del preview: asi SetPreviewColor no le cambia
        // el color y no se destruye y recrea con cada torre.
        rangeIndicator = new GameObject("RangeIndicator");
        rangeIndicator.transform.rotation = Quaternion.Euler(90f, 0f, 0f);   // tumbado en el suelo

        rangeRenderer = rangeIndicator.AddComponent<SpriteRenderer>();
        rangeRenderer.sprite       = rangeSprite != null ? rangeSprite : RuntimeSprites.Circle;
        rangeRenderer.sortingOrder = -10;   // por debajo de torres y enemigos

        rangeIndicator.SetActive(false);
    }

    /// <summary>
    /// Las torres no comparten clase base todavia, asi que se pregunta a cada
    /// tipo. El componente esta desactivado en el preview, pero GetComponent lo
    /// encuentra igual y su campo range sigue siendo el del prefab.
    /// </summary>
    static float GetTowerRange(GameObject tower)
    {
        if (tower == null) return 0f;

        TowerArcher archer = tower.GetComponentInChildren<TowerArcher>(true);
        if (archer != null) return archer.range;

        TowerMage mage = tower.GetComponentInChildren<TowerMage>(true);
        if (mage != null) return mage.range;

        TowerIce ice = tower.GetComponentInChildren<TowerIce>(true);
        if (ice != null) return ice.range;

        TowerCannon cannon = tower.GetComponentInChildren<TowerCannon>(true);
        if (cannon != null) return cannon.range;

        TowerFire fire = tower.GetComponentInChildren<TowerFire>(true);
        if (fire != null) return fire.range;

        return 0f;
    }

    public void CancelPlacement()
    {
        if (towerPreview != null)
            Destroy(towerPreview);
        isPlacing    = false;
        isDragging   = false;
        isShaking    = false;
        towerPreview = null;
    }

    Vector3 GetPointerWorldPosition()
    {
        Vector2 screenPos;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        else
            screenPos = Mouse.current.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float distance;

        if (groundPlane.Raycast(ray, out distance))
            return ray.GetPoint(distance);

        return Vector3.zero;
    }

    void SetPreviewColor(GameObject obj, Color color)
    {
        // Antes: r.material.color, que instancia un material nuevo por renderer
        // y no se destruye nunca, asi que cada seleccion de torre dejaba
        // materiales huerfanos. Las torres son SpriteRenderer y su .color es
        // una propiedad del propio renderer: tintar no cuesta un material.
        SpriteRenderer[] sprites = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in sprites)
            sr.color = color;
    }

    void DisableAttackScripts(GameObject preview)
    {
        MonoBehaviour[] scripts = preview.GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            // SpriteRotationFix tiene que seguir activo o el sprite del preview
            // no mira a camara y se ve tumbado sobre el suelo.
            if (script is TowerPlacer || script is SpriteRotationFix) continue;

            script.enabled = false;
        }
    }

    IEnumerator ShakePreview()
    {
        if (towerPreview == null) yield break;

        isShaking = true;
        Vector3 originalPos = towerPreview.transform.position;
        float duration  = 0.3f;
        float elapsed   = 0f;
        float magnitude = 0.3f;

        while (elapsed < duration)
        {
            float x = originalPos.x + Random.Range(-magnitude, magnitude);
            float z = originalPos.z + Random.Range(-magnitude, magnitude);
            towerPreview.transform.position = new Vector3(x, originalPos.y, z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        towerPreview.transform.position = originalPos;
        isShaking = false;
    }
}