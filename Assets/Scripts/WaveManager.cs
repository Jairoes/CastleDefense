using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Configuración de Oleadas")]
    public GameObject enemyPrefab;
    public WaypointPath waypointPath;
    public int enemiesPerWave = 5;
    public float timeBetweenEnemies = 1.5f;
    public float timeBetweenWaves = 5f;

    private int currentWave = 0;

    void Start()
    {
        StartCoroutine(StartWave());
    }

    IEnumerator StartWave()
    {
        currentWave++;
        Debug.Log("Oleada " + currentWave + " comenzando!");

        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenEnemies);
        }

        yield return new WaitForSeconds(timeBetweenWaves);
        StartCoroutine(StartWave());
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || waypointPath == null) return;

        Vector3 spawnPos = waypointPath.GetWaypoint(0).position;
        spawnPos.y = 0.5f;

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        enemy.GetComponent<EnemyMovement>().waypointPath = waypointPath;
    }
}