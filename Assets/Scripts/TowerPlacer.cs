using UnityEngine;

public class TowerPlacer : MonoBehaviour
{
    public static TowerPlacer Instance;

    [Header("Torres disponibles")]
    public GameObject archerTowerPrefab;
    public GameObject mageTowerPrefab;
    public GameObject iceTowerPrefab;
    public GameObject cannonTowerPrefab;

    [Header("Costos")]
    public int archerCost = 50;
    public int mageCost = 70;
    public int iceCost = 70;
    public int cannonCost = 90;

    private GameObject selectedTowerPrefab;
    private GameObject towerPreview;
    private int selectedCost = 0;
    private bool isPlacing = false;

    // Colores del preview
    private Color validColor   = new Color(0f, 1f, 0f, 0.5f);
    private Color invalidColor = new Color(1f, 0f, 0f, 0.5f);

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!isPlacing) return;

        Vector3 mousePos = GetMouseWorldPosition();

        if (towerPreview != null)
        {
            towerPreview.transform.position = mousePos;

            // Cambiar color según zona válida o no
            bool valid = IsValidPlacement(mousePos);
            SetPreviewColor(towerPreview, valid ? validColor : invalidColor);
        }

        // Colocar torre al hacer clic izquierdo
        if (Input.GetMouseButtonDown(0))
        {
            if (IsValidPlacement(mousePos))
                PlaceTower(mousePos);
            else
                Debug.Log("Zona no válida para colocar torre!");
        }

        // Cancelar con clic derecho
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    bool IsValidPlacement(Vector3 position)
    {
        // Verificar que no haya objetos de Path u Obstacle debajo
        Collider[] colliders = Physics.OverlapSphere(position, 1f);
        foreach (Collider col in colliders)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Path") ||
                col.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                return false;
            }
        }
        return true;
    }

    public void SelectTower(int towerIndex)
    {
        int cost = 0;
        GameObject prefab = null;

        switch (towerIndex)
        {
            case 0: prefab = archerTowerPrefab; cost = archerCost; break;
            case 1: prefab = mageTowerPrefab;   cost = mageCost;   break;
            case 2: prefab = iceTowerPrefab;    cost = iceCost;    break;
            case 3: prefab = cannonTowerPrefab; cost = cannonCost; break;
        }

        if (prefab == null) return;
        if (GameManager.Instance.crystals < cost)
        {
            Debug.Log("No hay suficientes cristales!");
            return;
        }

        CancelPlacement();

        selectedTowerPrefab = prefab;
        selectedCost = cost;
        isPlacing = true;

        towerPreview = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        SetPreviewColor(towerPreview, validColor);
    }

    void PlaceTower(Vector3 position)
    {
        if (GameManager.Instance.SpendCrystals(selectedCost))
        {
            Destroy(towerPreview);
            Instantiate(selectedTowerPrefab, position, Quaternion.identity);
            isPlacing = false;
            towerPreview = null;
        }
    }

    void CancelPlacement()
    {
        if (towerPreview != null)
            Destroy(towerPreview);
        isPlacing = false;
        towerPreview = null;
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
        {
            r.material.color = color;
        }
    }
}