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

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!isPlacing) return;

        // Mover preview con el mouse
        Vector3 mousePos = GetMouseWorldPosition();
        if (towerPreview != null)
            towerPreview.transform.position = mousePos;

        // Colocar torre al hacer clic
        if (Input.GetMouseButtonDown(0))
        {
            PlaceTower(mousePos);
        }

        // Cancelar con clic derecho
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
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

        // Cancelar selección anterior
        CancelPlacement();

        selectedTowerPrefab = prefab;
        selectedCost = cost;
        isPlacing = true;

        // Crear preview
        towerPreview = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        // Hacer el preview semitransparente
        SetPreviewTransparency(towerPreview, 0.5f);
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

    void SetPreviewTransparency(GameObject obj, float alpha)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            Color c = r.material.color;
            c.a = alpha;
            r.material.color = c;
        }
    }
}