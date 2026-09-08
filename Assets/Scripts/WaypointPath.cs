using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    [Header("Configuracion")]
    public Color pathColor      = Color.yellow;
    public float waypointRadius = 0.5f;

    /// <summary>
    /// Waypoint por indice, o null si el indice se sale del camino.
    /// Devolver null en vez de dejar que GetChild lance permite a quien llama
    /// tratar "he llegado al final" sin excepciones.
    /// </summary>
    public Transform GetWaypoint(int index)
    {
        if (index < 0 || index >= transform.childCount) return null;
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
            Gizmos.DrawSphere(transform.GetChild(i).position, waypointRadius);

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
