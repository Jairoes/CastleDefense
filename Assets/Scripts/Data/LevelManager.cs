using UnityEngine;

/// <summary>
/// Resuelve que LevelData usa la partida y lo deja disponible para el resto.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Tooltip("Nivel que se carga si entras directo a la escena desde el editor, " +
             "sin pasar por el menu. Sin esto, darle a Play aqui no funcionaria.")]
    public LevelData fallbackLevel;

    public LevelData Level { get; private set; }

    void Awake()
    {
        Instance = this;

        Level = LevelSession.Requested != null ? LevelSession.Requested : fallbackLevel;

        if (Level == null)
            Debug.LogError("LevelManager no tiene nivel: asigna un fallbackLevel.", this);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
