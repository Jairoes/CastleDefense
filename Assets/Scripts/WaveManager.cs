using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Wave
{
    public string waveName;
    public List<EnemySpawn> enemies;
    public float timeBetweenEnemies = 1.5f;
}

[System.Serializable]
public class EnemySpawn
{
    public GameObject enemyPrefab;
    public int count;
}

public class WaveManager : MonoBehaviour
{
    [Header("Configuración")]
    public WaypointPath waypointPath;
    public float timeBetweenWaves = 5f;

    [Header("Oleadas")]
    public List<Wave> waves;

    public int currentWave = 0;
    private bool isSpawning = false;

    void Start()
    {
        StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        if (currentWave >= waves.Count)
        {
            Debug.Log("¡Todas las oleadas completadas!");

            yield return new WaitUntil(() => 
                GameObject.FindGameObjectsWithTag("Enemy").Length == 0);
            
            GameManager.Instance.TriggerVictory();
            yield break;
        }

        Wave wave = waves[currentWave];
        currentWave++;

        Debug.Log("Oleada " + currentWave + " comenzando!");

        // Actualizar UI
        // if (GameUI.Instance != null)
        //     GameUI.Instance.UpdateWaveText(currentWave);

        // Spawnear todos los grupos de enemigos de la oleada
        foreach (EnemySpawn spawn in wave.enemies)
        {
            for (int i = 0; i < spawn.count; i++)
            {
                SpawnEnemy(spawn.enemyPrefab);
                yield return new WaitForSeconds(wave.timeBetweenEnemies);
            }
        }

        // Esperar antes de la siguiente oleada
        yield return new WaitForSeconds(timeBetweenWaves);
        StartCoroutine(StartNextWave());
    }

    void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null || waypointPath == null) return;

        Vector3 spawnPos = waypointPath.GetWaypoint(0).position;
        spawnPos.y = 0.5f;

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        enemy.GetComponent<EnemyMovement>().waypointPath = waypointPath;
    }
}