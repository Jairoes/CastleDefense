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
    public List<EnemySpawn> continuousEnemies;  // enemigos del flujo normal
    public float timeBetweenGroups   = 8f;      // segundos entre grupos
    public int   minPerGroup         = 2;       // mínimo enemigos por grupo
    public int   maxPerGroup         = 4;       // máximo enemigos por grupo
    public float timeBetweenEnemies  = 1f;      // tiempo entre cada enemigo del grupo
    public float continuousSpeedUp   = 0.85f;   // factor de aceleración tras oleada 1

    [Header("Oleadas especiales")]
    public List<BigWave> bigWaves;              // oleadas grandes en momentos clave

    [Header("Estado")]
    public int  currentWave    = 0;             // oleada especial actual (para UI)
    public bool isBigWaveActive = false;

    private float gameTimer      = 0f;
    private int   bigWaveIndex   = 0;
    private bool  gameFinished   = false;
    private float currentGroupTimer = 0f;

    void Start()
    {
        currentGroupTimer = timeBetweenGroups; // primer grupo sale al inicio
        StartCoroutine(CheckBigWaves());
    }

    void Update()
    {
        if (gameFinished) return;
        if (GameManager.Instance != null && GameManager.Instance.gameOver) return;

        gameTimer         += Time.deltaTime;
        currentGroupTimer += Time.deltaTime;

        // Flujo continuo — solo si no hay oleada grande activa
        if (!isBigWaveActive && bigWaveIndex < bigWaves.Count && currentGroupTimer >= timeBetweenGroups)
        {
            currentGroupTimer = 0f;
            StartCoroutine(SpawnContinuousGroup());
        }
    }

    // Coroutine que vigila cuándo lanzar oleadas grandes
    IEnumerator CheckBigWaves()
    {
        while (bigWaveIndex < bigWaves.Count)
        {
            BigWave nextWave = bigWaves[bigWaveIndex];

            // Esperar hasta que llegue el momento de la oleada
            yield return new WaitUntil(() => gameTimer >= nextWave.triggerAtTime);

            // Lanzar oleada grande
            yield return StartCoroutine(LaunchBigWave(nextWave));
            bigWaveIndex++;

            // Después de la oleada 1, acelerar el flujo continuo
            if (bigWaveIndex == 1)
            {
                timeBetweenGroups   *= continuousSpeedUp;
                timeBetweenEnemies  *= continuousSpeedUp;
            }
        }

        // Todas las oleadas grandes terminaron — esperar que mueran todos
        yield return new WaitUntil(() =>
            GameObject.FindGameObjectsWithTag("Enemy").Length == 0);

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

        // Cantidad aleatoria del grupo
        int groupSize = Random.Range(minPerGroup, maxPerGroup + 1);

        for (int i = 0; i < groupSize; i++)
        {
            // Elegir enemigo aleatorio de la lista de flujo continuo
            EnemySpawn spawn = continuousEnemies[Random.Range(0, continuousEnemies.Count)];
            SpawnEnemy(spawn.enemyPrefab);
            yield return new WaitForSeconds(timeBetweenEnemies);
        }
    }

    void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null || waypointPath == null) return;

        Vector3 spawnPos  = waypointPath.GetWaypoint(0).position;
        spawnPos.y        = 0.5f;
        GameObject enemy  = Instantiate(prefab, spawnPos, Quaternion.identity);
        enemy.GetComponent<EnemyMovement>().waypointPath = waypointPath;
    }
}