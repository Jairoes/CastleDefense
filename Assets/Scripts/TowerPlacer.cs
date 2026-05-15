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
    private int selectedCost  = 0;
    private bool isPlacing    = false;

    private Color validColor   = new Color(1f, 1f, 1f, 0.5f);
    private Color invalidColor = new Color(1f, 1f, 1f, 0.5f);
    private bool isShaking = false;

    // Input Actions
    private InputAction tapAction;

    void Awake()
    {
        Instance = this;

        // Acción de tap — funciona tanto en móvil (touch) como en PC (clic)
        tapAction = new InputAction("Tap", binding: "<Pointer>/press");
        tapAction.performed += OnTap;
    }

    void OnEnable()  { tapAction.Enable(); }
    void OnDisable() { tapAction.Disable(); }

    void OnDestroy() { tapAction.performed -= OnTap; }

    void Update()
    {
        if (!isPlacing) return;

        Vector3 worldPos = GetPointerWorldPosition();
        worldPos.y = 2f;

        if (towerPreview != null)
        {
            towerPreview.transform.position = worldPos;
            bool valid = IsValidPlacement(worldPos);
            SetPreviewColor(towerPreview, valid ? validColor : invalidColor);
        }
    }

    void OnTap(InputAction.CallbackContext context)
    {
        if (!isPlacing) return;

        // Ignorar si el tap fue sobre la UI (botones de torres)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // En móvil verificar con el ID del primer toque
        if (Touchscreen.current != null &&
            EventSystem.current.IsPointerOverGameObject(
                Touchscreen.current.primaryTouch.touchId.ReadValue()))
            return;

        Vector3 worldPos = GetPointerWorldPosition();

        if (IsValidPlacement(worldPos))
            PlaceTower(worldPos);
        else
            StartCoroutine(ShakePrevief());
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
        int combinedMask  = pathLayer | obstacleLayer;
    
        return !Physics.CheckBox(center, halfExtents, Quaternion.identity, combinedMask);
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

        // Cancela cualquier torre seleccionada anteriormente
        CancelPlacement();

        selectedTowerPrefab = prefab;
        selectedCost        = cost;
        isPlacing           = true;

        towerPreview = Instantiate(prefab, new Vector3(0, 2f, 0), Quaternion.identity);
        DisableAttackScripts(towerPreview);
        SetPreviewColor(towerPreview, validColor);
    }

    void PlaceTower(Vector3 position)
    {
        if (GameManager.Instance.SpendCrystals(selectedCost))
        {
            Destroy(towerPreview);
            position.y = 2f;
            GameObject newTower = Instantiate(selectedTowerPrefab, position, Quaternion.identity);
            newTower.layer = LayerMask.NameToLayer("Obstacle");
            isPlacing      = false;
            towerPreview   = null;
        }
    }

    public void CancelPlacement()
    {
        if (towerPreview != null)
            Destroy(towerPreview);
        isPlacing    = false;
        towerPreview = null;
    }

    Vector3 GetPointerWorldPosition()
    {
        // Funciona tanto con mouse (PC/editor) como touch (móvil)
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
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.material.color = color;
    }

    void DisableAttackScripts(GameObject preview)
    {
        MonoBehaviour[] scripts = preview.GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script is not TowerPlacer)
                script.enabled = false;
        }
    }
    
    IEnumerator ShakePrevief()
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