using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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

    private Color validColor   = new Color(0f, 1f, 0f, 0.5f);
    private Color invalidColor = new Color(1f, 0f, 0f, 0.5f);

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
            Debug.Log("Zona no válida para colocar torre!");
    }

    bool IsValidPlacement(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 1f);
        foreach (Collider col in colliders)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Path") ||
                col.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
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

        // Cancela cualquier torre seleccionada anteriormente
        CancelPlacement();

        selectedTowerPrefab = prefab;
        selectedCost        = cost;
        isPlacing           = true;

        towerPreview = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        DisableAttackScripts(towerPreview);
        SetPreviewColor(towerPreview, validColor);
    }

    void PlaceTower(Vector3 position)
    {
        if (GameManager.Instance.SpendCrystals(selectedCost))
        {
            Destroy(towerPreview);
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
}