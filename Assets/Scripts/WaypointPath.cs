using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    [Header("Configuración")]
    public Color pathColor = Color.yellow;
    public float waypointRadius = 0.5f;

    public Transform GetWaypoint(int index)
    {
        return transform.GetChild(index);
    }

    public int GetWaypointCount()
    {
        return transform.childCount;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = pathColor;

        for (int i = 0; i < transform.childCount; i++)
        {
            // Dibuja esfera en cada waypoint
            Gizmos.DrawSphere(transform.GetChild(i).position, waypointRadius);

            // Dibuja línea al siguiente waypoint
            if (i + 1 < transform.childCount)
            {
                Gizmos.DrawLine(
                    transform.GetChild(i).position,
                    transform.GetChild(i + 1).position
                );
            }
        }
    }
}