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

    public void SelectTower(int towerIndex)
    {
        int cost          = 0;
        GameObject prefab = null;

        switch (towerIndex)
        {
            case 0: prefab = archerTowerPrefab; cost = archerCost; break;
            case 1: prefab = mageTowerPrefab;   cost = mageCost;   break;
            case 2: prefab = iceTowerPrefab;    cost = iceCost;    break;
            case 3: prefab = cannonTowerPrefab; cost = cannonCost; break;
            case 4: prefab = fireTowerPrefab;   cost = fireCost;   break;
        }

        if (prefab == null) return;

        if (GameManager.Instance.crystals < cost)
        {
            Debug.Log("No hay suficientes cristales!");
            return;
        }

        CancelPlacement();

        selectedTowerPrefab = prefab;
        selectedCost        = cost;
        isPlacing           = true;
        isDragging          = true; // permite arrastre inmediato desde el botón

        towerPreview = Instantiate(prefab, new Vector3(0, -100f, 0), Quaternion.identity);
        DisableAttackScripts(towerPreview);
        SetPreviewColor(towerPreview, previewColor);
    }

    void PlaceTower(Vector3 position)
    {
        if (GameManager.Instance.SpendCrystals(selectedCost))
        {
            Destroy(towerPreview);
            position.y = 2f;
            GameObject newTower = Instantiate(selectedTowerPrefab, position, Quaternion.identity);
            newTower.layer = LayerMask.NameToLayer("Obstacle");
            isPlacing    = false;
            isDragging   = false;
            towerPreview = null;
        }
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