using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawn
{
    public GameObject enemyPrefab;
    public int count;
}

[System.Serializable]
public class BigWave
{
    public string waveName;
    public float triggerAtTime;        // segundos desde el inicio para activarse
    public List<EnemySpawn> enemies;
    public float timeBetweenEnemies = 0.5f;
}

public class WaveManager : MonoBehaviour
{
    [Header("Configuración")]
    public WaypointPath waypointPath;

    [Header("Flujo continuo")]
    public List<EnemySpawn> continuousEnemies;
    public float timeBetweenGroups   = 8f;
    public int   minPerGroup         = 2;
    public int   maxPerGroup         = 4;
    public float timeBetweenEnemies  = 1f;
    public float continuousSpeedUp   = 0.85f;

    [Header("Oleadas especiales")]
    public List<BigWave> bigWaves;

    [Header("Estado")]
    public int  currentWave     = 0;
    public bool isBigWaveActive = false;

    private float gameTimer         = 0f;
    private int   bigWaveIndex      = 0;
    private bool  gameFinished      = false;
    private float currentGroupTimer = 0f;

    void Start()
    {
        currentGroupTimer = timeBetweenGroups;
        StartCoroutine(CheckBigWaves());
    }

    void Update()
    {
        if (gameFinished) return;
        if (GameManager.Instance != null && GameManager.Instance.gameOver) return;

        gameTimer         += Time.deltaTime;
        currentGroupTimer += Time.deltaTime;

        if (!isBigWaveActive && bigWaveIndex < bigWaves.Count && currentGroupTimer >= timeBetweenGroups)
        {
            currentGroupTimer = 0f;
            StartCoroutine(SpawnContinuousGroup());
        }
    }

    IEnumerator CheckBigWaves()
    {
        while (bigWaveIndex < bigWaves.Count)
        {
            BigWave nextWave = bigWaves[bigWaveIndex];

            yield return new WaitUntil(() => gameTimer >= nextWave.triggerAtTime);

            yield return StartCoroutine(LaunchBigWave(nextWave));
            bigWaveIndex++;

            if (bigWaveIndex == 1)
            {
                timeBetweenGroups  *= continuousSpeedUp;
                timeBetweenEnemies *= continuousSpeedUp;
            }
        }

        // Todas las oleadas grandes terminaron — esperar que mueran todos
        yield return new WaitUntil(() => EnemyRegistry.Count == 0);

        // Verificar que no haya Game Over antes de dar victoria
        if (GameManager.Instance.gameOver)
        {
            gameFinished = true;
            yield break;
        }

        gameFinished = true;
        GameManager.Instance.TriggerVictory();
    }

    IEnumerator LaunchBigWave(BigWave wave)
    {
        isBigWaveActive = true;
        currentWave++;

        bool isLastWave = (bigWaveIndex == bigWaves.Count - 1);

        Debug.Log("¡OLEADA GRANDE: " + wave.waveName + "!");

        foreach (EnemySpawn spawn in wave.enemies)
        {
            for (int i = 0; i < spawn.count; i++)
            {
                SpawnEnemy(spawn.enemyPrefab);
                yield return new WaitForSeconds(wave.timeBetweenEnemies);
            }
        }

        if (!isLastWave)
            isBigWaveActive = false;
    }

    IEnumerator SpawnContinuousGroup()
    {
        if (continuousEnemies.Count == 0) yield break;

        int groupSize = Random.Range(minPerGroup, maxPerGroup + 1);

        for (int i = 0; i < groupSize; i++)
        {
            EnemySpawn spawn = continuousEnemies[Random.Range(0, continuousEnemies.Count)];
            SpawnEnemy(spawn.enemyPrefab);
            yield return new WaitForSeconds(timeBetweenEnemies);
        }
    }

    void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null || waypointPath == null) return;

        Transform start = waypointPath.GetWaypoint(0);
        if (start == null)
        {
            Debug.LogError("WaveManager: el WaypointPath no tiene waypoints.", this);
            return;
        }

        Vector3 spawnPos = start.position;
        spawnPos.y       = 0.5f;

        GameObject enemy       = Instantiate(prefab, spawnPos, Quaternion.identity);
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();

        if (movement != null)
            movement.waypointPath = waypointPath;
        else
            Debug.LogError("El prefab " + prefab.name + " no tiene EnemyMovement.", enemy);
    }
}