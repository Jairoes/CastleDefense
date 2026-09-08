using UnityEngine;

/// <summary>
/// Definicion de un tipo de enemigo. Las estadisticas viven aqui y no en el
/// prefab, para que "orco fuerte" sea un asset nuevo y no un prefab duplicado
/// con numeros retocados a mano.
/// </summary>
[CreateAssetMenu(fileName = "Enemy_", menuName = "CastleDefense/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identidad")]
    public string displayName = "Enemigo";
    public GameObject prefab;

    [Header("Estadisticas base")]
    [Tooltip("Los multiplicadores del LevelData se aplican sobre estos valores.")]
    public float maxHealth = 50f;
    public float moveSpeed = 3.5f;
    public float damage    = 10f;
    public int crystalReward = 8;
}
