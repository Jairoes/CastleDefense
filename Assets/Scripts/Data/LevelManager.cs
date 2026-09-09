using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Resuelve que nivel se juega y carga su escena de layout de forma aditiva
/// sobre la escena compartida de juego.
///
/// El resto de sistemas espera a que Ready sea true antes de arrancar, porque
/// hasta entonces el camino, el castillo y las zonas de colocacion todavia no
/// existen en memoria.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Tooltip("Nivel que se carga si entras directo a la escena desde el editor, " +
             "sin pasar por el menu. Sin esto, darle a Play aqui no funcionaria.")]
    public LevelData fallbackLevel;

    public LevelData Level { get; private set; }

    /// <summary>True cuando el layout ya esta cargado y se puede jugar.</summary>
    public bool Ready { get; private set; }

    private Scene loadedLayout;

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

    IEnumerator Start()
    {
        if (Level == null) yield break;

        // Layout vacio = la geometria ya esta en esta misma escena. Es el caso
        // mientras la escena original siga sin partirse en dos.
        if (string.IsNullOrEmpty(Level.layoutSceneName))
        {
            Ready = true;
            yield break;
        }

        Scene existing = SceneManager.GetSceneByName(Level.layoutSceneName);
        if (existing.IsValid() && existing.isLoaded)
        {
            loadedLayout = existing;
            Ready = true;
            yield break;
        }

        AsyncOperation load = SceneManager.LoadSceneAsync(
            Level.layoutSceneName, LoadSceneMode.Additive);

        if (load == null)
        {
            Debug.LogError(
                "No se pudo cargar el layout '" + Level.layoutSceneName +
                "'. Comprueba que la escena esta en Build Settings.", this);
            yield break;
        }

        yield return load;

        loadedLayout = SceneManager.GetSceneByName(Level.layoutSceneName);
        Ready = true;
    }
}
