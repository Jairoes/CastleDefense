using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Genera los LevelData de un mapa y su catalogo.
///
/// Sigue el modelo hibrido: aqui se decide la COMPOSICION de cada nivel (que
/// enemigos entran y cuando), que es la parte interesante, y el escalado fino
/// lo resuelven los multiplicadores del LevelData. Lo generado son assets
/// normales y corrientes: retocalos a mano todo lo que quieras despues.
///
/// No sobrescribe niveles existentes salvo que marques la casilla.
/// </summary>
public class LevelGenerator : EditorWindow
{
    // ---- Mapa ----
    private string mapId        = "forest";
    private string outputFolder = "Assets/Levels/Forest";
    private string layoutPrefix = "Forest_Layout_";
    private int levelCount      = 10;
    private bool assignLayoutScenes = false;
    private bool overwrite      = false;

    // ---- Curva de dificultad ----
    // Vida y recompensa suben juntas a proposito: si sube solo la vida, al
    // jugador no le da la economia para construir y el nivel se vuelve injusto
    // por un motivo que no se ve en pantalla.
    private float healthPerLevel = 0.20f;   // nivel 10 -> x2.8
    private float countPerLevel  = 0.08f;   // nivel 10 -> x1.72
    private float rewardPerLevel = 0.10f;   // nivel 10 -> x1.9
    private int baseCrystals     = 100;
    private int crystalsPerLevel = 10;
    private float castleHp       = 100f;

    // ---- Cuando entra cada enemigo ----
    private int orcUnlockLevel = 3;

    private GameObject skeletonPrefab;
    private GameObject goblinPrefab;
    private GameObject orcPrefab;

    [MenuItem("CastleDefense/Generar niveles")]
    public static void Open()
    {
        GetWindow<LevelGenerator>("Generar niveles").minSize = new Vector2(430, 580);
    }

    void OnEnable()
    {
        skeletonPrefab = Load("Assets/Prefabs/Enemies/Enemy_Skeleton.prefab");
        goblinPrefab   = Load("Assets/Prefabs/Enemies/Enemy_Goblin.prefab");
        orcPrefab      = Load("Assets/Prefabs/Enemies/Enemy_Orc_Junior.prefab");
    }

    static GameObject Load(string path)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Mapa", EditorStyles.boldLabel);
        mapId        = EditorGUILayout.TextField("Id del mapa", mapId);
        outputFolder = EditorGUILayout.TextField("Carpeta destino", outputFolder);
        levelCount   = EditorGUILayout.IntSlider("Numero de niveles", levelCount, 1, 20);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Enemigos", EditorStyles.boldLabel);
        skeletonPrefab = (GameObject)EditorGUILayout.ObjectField("Esqueleto", skeletonPrefab, typeof(GameObject), false);
        goblinPrefab   = (GameObject)EditorGUILayout.ObjectField("Goblin",    goblinPrefab,   typeof(GameObject), false);
        orcPrefab      = (GameObject)EditorGUILayout.ObjectField("Orco",      orcPrefab,      typeof(GameObject), false);
        orcUnlockLevel = EditorGUILayout.IntSlider("El orco entra en el nivel", orcUnlockLevel, 1, 10);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Curva de dificultad", EditorStyles.boldLabel);
        healthPerLevel   = EditorGUILayout.Slider("Vida +/nivel",       healthPerLevel, 0f, 0.5f);
        countPerLevel    = EditorGUILayout.Slider("Cantidad +/nivel",   countPerLevel,  0f, 0.5f);
        rewardPerLevel   = EditorGUILayout.Slider("Recompensa +/nivel", rewardPerLevel, 0f, 0.5f);
        baseCrystals     = EditorGUILayout.IntField("Cristales nivel 1", baseCrystals);
        crystalsPerLevel = EditorGUILayout.IntField("Cristales +/nivel", crystalsPerLevel);
        castleHp         = EditorGUILayout.FloatField("Vida del castillo", castleHp);

        EditorGUILayout.HelpBox(
            "Nivel " + levelCount + ":  vida x" + Mult(healthPerLevel, levelCount).ToString("0.00") +
            "   cantidad x" + Mult(countPerLevel, levelCount).ToString("0.00") +
            "   recompensa x" + Mult(rewardPerLevel, levelCount).ToString("0.00"),
            MessageType.Info);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Layout", EditorStyles.boldLabel);
        assignLayoutScenes = EditorGUILayout.Toggle("Asignar escenas de layout", assignLayoutScenes);

        using (new EditorGUI.DisabledScope(!assignLayoutScenes))
            layoutPrefix = EditorGUILayout.TextField("Prefijo de escena", layoutPrefix);

        if (!assignLayoutScenes)
        {
            EditorGUILayout.HelpBox(
                "Sin marcar, los niveles se generan sin escena de layout y usan la " +
                "geometria que ya haya en la escena actual. Marcalo cuando tengas " +
                "creadas las escenas de layout y esten en Build Settings.",
                MessageType.None);
        }

        EditorGUILayout.Space();
        overwrite = EditorGUILayout.Toggle("Sobrescribir existentes", overwrite);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(!Valid()))
        {
            if (GUILayout.Button("Generar", GUILayout.Height(34)))
                Generate();
        }

        if (!Valid())
            EditorGUILayout.HelpBox("Asigna los tres prefabs de enemigo.", MessageType.Warning);
    }

    bool Valid()
    {
        return skeletonPrefab != null && goblinPrefab != null && orcPrefab != null;
    }

    static float Mult(float perLevel, int level)
    {
        return 1f + (level - 1) * perLevel;
    }

    /// <summary>Oleadas grandes segun lo avanzado que este el nivel.</summary>
    static int BigWaveCount(int level)
    {
        if (level <= 2) return 1;
        if (level <= 6) return 2;
        return 3;
    }

    void Generate()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            AssetDatabase.Refresh();
        }

        List<LevelData> niveles = new List<LevelData>();
        int escritos = 0, conservados = 0;

        for (int level = 1; level <= levelCount; level++)
        {
            string path = outputFolder + "/Level_" + mapId + "_" + level.ToString("00") + ".asset";
            LevelData existente = AssetDatabase.LoadAssetAtPath<LevelData>(path);

            if (existente != null && !overwrite)
            {
                niveles.Add(existente);
                conservados++;
                continue;
            }

            LevelData data = existente != null ? existente : CreateInstance<LevelData>();
            Configure(data, level);

            if (existente == null)
                AssetDatabase.CreateAsset(data, path);
            else
                EditorUtility.SetDirty(data);

            niveles.Add(data);
            escritos++;
        }

        CreateCatalog(niveles);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Niveles: " + escritos + " escritos, " + conservados +
                  " conservados. Carpeta: " + outputFolder);
    }

    void Configure(LevelData data, int level)
    {
        data.mapId           = mapId;
        data.levelNumber     = level;
        data.levelName       = "Nivel " + level;
        data.layoutSceneName = assignLayoutScenes ? layoutPrefix + level.ToString("00") : "";

        data.startingCrystals = baseCrystals + (level - 1) * crystalsPerLevel;
        data.castleHealth     = castleHp;

        data.healthMultiplier = Mult(healthPerLevel, level);
        data.countMultiplier  = Mult(countPerLevel,  level);
        data.rewardMultiplier = Mult(rewardPerLevel, level);

        // Flujo continuo: relleno entre oleadas grandes. El orco no entra aqui
        // nunca, solo en oleada grande, para que ver un orco siga siendo un aviso.
        data.continuousEnemies = new List<WaveGroup>
        {
            new WaveGroup { enemyPrefab = skeletonPrefab, count = 1 },
            new WaveGroup { enemyPrefab = goblinPrefab,   count = 1 },
        };

        data.timeBetweenGroups  = Mathf.Max(4f, 9f - level * 0.3f);
        data.minPerGroup        = 2 + level / 4;
        data.maxPerGroup        = 4 + level / 3;
        data.timeBetweenEnemies = Mathf.Max(0.35f, 0.9f - level * 0.04f);
        data.continuousSpeedUp  = 0.85f;

        int totalOleadas = BigWaveCount(level);
        data.bigWaves = new List<WaveDefinition>();

        for (int w = 0; w < totalOleadas; w++)
        {
            bool esUltima = (w == totalOleadas - 1);

            WaveDefinition wave = new WaveDefinition
            {
                waveName           = esUltima ? "Oleada final" : "Oleada " + (w + 1),
                triggerAtTime      = 60f + w * 80f,
                timeBetweenEnemies = Mathf.Max(0.30f, 0.55f - level * 0.02f),
                groups             = new List<WaveGroup>(),
            };

            // La mezcla se endurece dentro del propio nivel: la ultima oleada
            // trae mas de todo que la primera.
            float peso = 1f + w * 0.5f;

            int esqueletos = Mathf.RoundToInt((8f + level * 1.2f) * peso);
            int goblins    = Mathf.RoundToInt((4f + level * 0.8f) * peso);

            wave.groups.Add(new WaveGroup { enemyPrefab = skeletonPrefab, count = esqueletos });
            wave.groups.Add(new WaveGroup { enemyPrefab = goblinPrefab,   count = goblins });

            if (level >= orcUnlockLevel)
            {
                int orcos = Mathf.Max(1,
                    Mathf.RoundToInt((level - orcUnlockLevel + 1) * 0.8f * peso));
                wave.groups.Add(new WaveGroup { enemyPrefab = orcPrefab, count = orcos });
            }

            data.bigWaves.Add(wave);
        }
    }

    void CreateCatalog(List<LevelData> niveles)
    {
        string path = outputFolder + "/Catalog_" + mapId + ".asset";
        LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(path);

        bool esNuevo = catalog == null;
        if (esNuevo) catalog = CreateInstance<LevelCatalog>();

        catalog.mapId  = mapId;
        catalog.levels = new List<LevelData>(niveles);

        if (esNuevo)
            AssetDatabase.CreateAsset(catalog, path);
        else
            EditorUtility.SetDirty(catalog);
    }
}
