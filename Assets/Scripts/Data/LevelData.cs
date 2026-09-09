using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Un grupo de enemigos del mismo tipo dentro de una oleada.
/// Las estadisticas (vida, velocidad, dano, recompensa) viven en el prefab; el
/// nivel solo las escala con sus multiplicadores al instanciar.
/// </summary>
[System.Serializable]
public class WaveGroup
{
    public GameObject enemyPrefab;
    public int count = 5;
}

/// <summary>Oleada grande, programada a un segundo concreto de la partida.</summary>
[System.Serializable]
public class WaveDefinition
{
    public string waveName = "Oleada";

    [Tooltip("Segundos desde el inicio del nivel.")]
    public float triggerAtTime = 60f;

    public List<WaveGroup> groups = new List<WaveGroup>();
    public float timeBetweenEnemies = 0.5f;
}

/// <summary>
/// Las reglas de un nivel como asset: oleadas, economia y dificultad.
///
/// El reparto es: la geometria del nivel (camino, NavMesh, zonas de colocacion,
/// decoracion) vive en su escena de layout, que se carga de forma aditiva sobre
/// la escena compartida de juego. Las reglas viven aqui. Asi se retoca el
/// balanceo sin abrir ninguna escena, y los managers y la UI existen una sola
/// vez en lugar de repetirse en cada nivel.
/// </summary>
[CreateAssetMenu(fileName = "Level_", menuName = "CastleDefense/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Layout")]
    [Tooltip("Escena aditiva con el camino, la NavMesh y la decoracion. Dejalo " +
             "vacio si el layout ya esta dentro de la escena actual.")]
    public string layoutSceneName = "";

    [Header("Identidad")]
    [Tooltip("Mapa al que pertenece. Agrupa el progreso guardado.")]
    public string mapId = "forest";
    public int levelNumber = 1;
    public string levelName = "Nivel 1";

    [Header("Arranque")]
    public int startingCrystals = 100;
    public float castleHealth   = 100f;

    [Header("Escalado de dificultad")]
    [Tooltip("Multiplica la vida de todos los enemigos del nivel.")]
    public float healthMultiplier = 1f;

    [Tooltip("Multiplica cuantos enemigos salen en cada grupo.")]
    public float countMultiplier = 1f;

    [Tooltip("Multiplica los cristales que sueltan al morir. Si sube la vida y " +
             "no la recompensa, al jugador no le da la economia para construir.")]
    public float rewardMultiplier = 1f;

    [Header("Flujo continuo")]
    [Tooltip("De estos se elige al azar para los grupos de relleno entre oleadas.")]
    public List<WaveGroup> continuousEnemies = new List<WaveGroup>();
    public float timeBetweenGroups  = 8f;
    public int minPerGroup          = 2;
    public int maxPerGroup          = 4;
    public float timeBetweenEnemies = 1f;

    [Tooltip("Tras la primera oleada grande, los tiempos del flujo continuo se " +
             "multiplican por esto. Menor que 1 = mas rapido.")]
    public float continuousSpeedUp = 0.85f;

    [Header("Oleadas grandes")]
    public List<WaveDefinition> bigWaves = new List<WaveDefinition>();

    /// <summary>Cantidad de un grupo ya escalada por la dificultad del nivel.</summary>
    public int ScaledCount(int baseCount)
    {
        return Mathf.Max(1, Mathf.RoundToInt(baseCount * countMultiplier));
    }
}
