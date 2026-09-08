using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class MapProgress
{
    public string mapId;
    public int highestUnlocked = 1;
}

[System.Serializable]
public class GameProgress
{
    public List<MapProgress> maps = new List<MapProgress>();
}

/// <summary>
/// Progreso guardado en local, en un JSON dentro de Application.persistentDataPath.
/// El juego es offline y no hay cuentas: nada sale del dispositivo.
///
/// Se usa un fichero en vez de PlayerPrefs porque en cuanto haya estrellas o
/// mejor puntuacion por nivel, PlayerPrefs obliga a inventar claves por cada
/// dato; aqui basta con anadir campos al modelo.
/// </summary>
public static class SaveSystem
{
    private const string FileName = "progress.json";

    private static GameProgress cached;

    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static GameProgress Progress
    {
        get
        {
            if (cached == null) Load();
            return cached;
        }
    }

    static void Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                cached = JsonUtility.FromJson<GameProgress>(json);
            }
        }
        catch (System.Exception e)
        {
            // Un guardado corrupto no debe impedir jugar: se empieza de cero.
            Debug.LogWarning("No se pudo leer el progreso, se reinicia: " + e.Message);
        }

        if (cached == null)
            cached = new GameProgress();
    }

    public static void Save()
    {
        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(Progress, true));
        }
        catch (System.Exception e)
        {
            Debug.LogError("No se pudo guardar el progreso: " + e.Message);
        }
    }

    static MapProgress GetOrCreate(string mapId)
    {
        foreach (MapProgress m in Progress.maps)
            if (m.mapId == mapId) return m;

        MapProgress created = new MapProgress { mapId = mapId, highestUnlocked = 1 };
        Progress.maps.Add(created);
        return created;
    }

    public static int GetHighestUnlocked(string mapId)
    {
        return GetOrCreate(mapId).highestUnlocked;
    }

    public static bool IsUnlocked(string mapId, int levelNumber)
    {
        return levelNumber <= GetHighestUnlocked(mapId);
    }

    /// <summary>Desbloquea el nivel siguiente al completado. No retrocede nunca.</summary>
    public static void CompleteLevel(string mapId, int levelNumber)
    {
        MapProgress map = GetOrCreate(mapId);

        if (levelNumber + 1 > map.highestUnlocked)
        {
            map.highestUnlocked = levelNumber + 1;
            Save();
        }
    }

    public static void ResetProgress()
    {
        cached = new GameProgress();
        Save();
    }
}
