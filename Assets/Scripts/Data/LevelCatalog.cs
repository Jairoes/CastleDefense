using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lista ordenada de los niveles de un mapa. La usan el menu (para arrancar en
/// el nivel mas alto desbloqueado) y la pantalla de victoria (para saber cual
/// es el siguiente).
/// </summary>
[CreateAssetMenu(fileName = "Catalog_", menuName = "CastleDefense/Level Catalog")]
public class LevelCatalog : ScriptableObject
{
    public string mapId = "forest";

    [Tooltip("Escena de juego de este mapa.")]
    public string sceneName = "Forest_Level_01";

    [Tooltip("Ordenados del 1 al 10.")]
    public List<LevelData> levels = new List<LevelData>();

    public int Count => levels.Count;

    public LevelData Get(int levelNumber)
    {
        foreach (LevelData l in levels)
            if (l != null && l.levelNumber == levelNumber) return l;

        return null;
    }

    /// <summary>Siguiente nivel, o null si el que se pasa era el ultimo.</summary>
    public LevelData Next(LevelData current)
    {
        if (current == null) return null;
        return Get(current.levelNumber + 1);
    }

    /// <summary>Nivel mas alto desbloqueado, acotado a los que existen.</summary>
    public LevelData HighestUnlocked()
    {
        int highest = Mathf.Clamp(SaveSystem.GetHighestUnlocked(mapId), 1, Mathf.Max(1, Count));
        return Get(highest) ?? Get(1);
    }
}
