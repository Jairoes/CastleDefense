using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Camino")]
    [Tooltip("Opcional. Si se deja vacio se busca en la escena de layout una vez " +
             "esta cargada, que es lo normal con escenas aditivas.")]
    public WaypointPath waypointPath;

    [Header("Estado (solo lectura)")]
    public int currentWave      = 0;
    public bool isBigWaveActive = false;

    public int TotalBigWaves => level != null ? level.bigWaves.Count : 0;

    private LevelData level;

    // Copias de trabajo. Los tiempos se aceleran durante la partida y el asset
    // NO debe tocarse: escribir en un ScriptableObject en el editor persiste en
    // disco, y el nivel se volveria mas rapido en cada partida hasta romperse.
    private float groupInterval;
    private float enemyInterval;

    private float gameTimer         = 0f;
    private int bigWaveIndex        = 0;
    private bool gameFinished       = false;
    private float currentGroupTimer = 0f;
    private bool running            = false;

    IEnumerator Start()
    {
        // El layout (y con el, el camino) puede tardar un frame o varios en
        // cargarse de forma aditiva.
        yield return new WaitUntil(() =>
            LevelManager.Instance == null || LevelManager.Instance.Ready);

        if (LevelManager.Instance == null || LevelManager.Instance.Level == null)
        {
            Debug.LogError("WaveManager: no hay nivel que jugar.", this);
            yield break;
        }

        level = LevelManager.Instance.Level;

        if (waypointPath == null)
            waypointPath = FindFirstObjectByType<WaypointPath>();

        if (waypointPath == null)
        {
            Debug.LogError("WaveManager: no hay WaypointPath en la escena.", this);
            yield break;
        }

        groupInterval     = level.timeBetweenGroups;
        enemyInterval     = level.timeBetweenEnemies;
        currentGroupTimer = groupInterval;
        running           = true;

        StartCoroutine(CheckBigWaves());
    }

    void Update()
    {
        if (!running || gameFinished) return;
        if (GameManager.Instance != null && GameManager.Instance.gameOver) return;

        gameTimer         += Time.deltaTime;
        currentGroupTimer += Time.deltaTime;

        // El flujo continuo solo rellena entre oleadas grandes: cuando ya no
        // quedan, deja de generar para que la partida pueda terminar.
        bool quedanOleadas = bigWaveIndex < level.bigWaves.Count;

        if (!isBigWaveActive && quedanOleadas && currentGroupTimer >= groupInterval)
        {
            currentGroupTimer = 0f;
            StartCoroutine(SpawnContinuousGroup());
        }
    }

    IEnumerator CheckBigWaves()
    {
        while (bigWaveIndex < level.bigWaves.Count)
        {
            WaveDefinition next = level.bigWaves[bigWaveIndex];

            yield return new WaitUntil(() => gameTimer >= next.triggerAtTime);
            yield return StartCoroutine(LaunchBigWave(next));

            bigWaveIndex++;

            if (bigWaveIndex == 1)
            {
                groupInterval *= level.continuousSpeedUp;
                enemyInterval *= level.continuousSpeedUp;
            }
        }

        yield return new WaitUntil(() => EnemyRegistry.Count == 0);

        gameFinished = true;

        if (GameManager.Instance != null && !GameManager.Instance.gameOver)
            GameManager.Instance.TriggerVictory();
    }

    IEnumerator LaunchBigWave(WaveDefinition wave)
    {
        isBigWaveActive = true;
        currentWave++;

        bool isLastWave = (bigWaveIndex == level.bigWaves.Count - 1);

        foreach (WaveGroup group in wave.groups)
        {
            int total = level.ScaledCount(group.count);

            for (int i = 0; i < total; i++)
            {
                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(wave.timeBetweenEnemies);
            }
        }

        // Tras la ultima oleada se deja activo a proposito, para que el flujo
        // continuo no siga generando y la partida pueda cerrarse en victoria.
        if (!isLastWave)
            isBigWaveActive = false;
    }

    IEnumerator SpawnContinuousGroup()
    {
        List<WaveGroup> pool = level.continuousEnemies;
        if (pool == null || pool.Count == 0) yield break;

        int groupSize = Random.Range(level.minPerGroup, level.maxPerGroup + 1);

        for (int i = 0; i < groupSize; i++)
        {
            WaveGroup pick = pool[Random.Range(0, pool.Count)];
            SpawnEnemy(pick.enemyPrefab);
            yield return new WaitForSeconds(enemyInterval);
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

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        if (movement != null)
            movement.waypointPath = waypointPath;
        else
            Debug.LogError("El prefab " + prefab.name + " no tiene EnemyMovement.", enemy);

        // Los multiplicadores se aplican sobre los valores del prefab. Se hace
        // antes del primer Start del enemigo, que es donde EnemyHealth copia
        // maxHealth a currentHealth.
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.maxHealth    *= level.healthMultiplier;
            health.crystalReward = Mathf.Max(
                1, Mathf.RoundToInt(health.crystalReward * level.rewardMultiplier));
        }
    }
}
