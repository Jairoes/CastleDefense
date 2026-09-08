using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registro central de enemigos vivos.
///
/// Sustituye a GameObject.FindGameObjectsWithTag("Enemy"), que cada torre
/// llamaba en cada frame: con 5 torres y 30 enemigos eran 5 barridos completos
/// de la escena y 5 arrays nuevos cada frame. Aqui los enemigos se dan de alta
/// solos al activarse y de baja al morir, asi que buscar objetivo es recorrer
/// una lista que ya existe.
/// </summary>
public static class EnemyRegistry
{
    private static readonly List<EnemyHealth> alive = new List<EnemyHealth>(64);

    public static int Count => alive.Count;

    public static void Register(EnemyHealth enemy)
    {
        if (enemy != null && !alive.Contains(enemy))
            alive.Add(enemy);
    }

    public static void Unregister(EnemyHealth enemy)
    {
        alive.Remove(enemy);
    }

    /// <summary>
    /// Enemigo mas cercano dentro de 'range', o null si no hay ninguno.
    /// Compara distancias al cuadrado para ahorrar una raiz cuadrada por
    /// enemigo y por torre.
    /// </summary>
    public static GameObject FindNearest(Vector3 from, float range)
    {
        float bestSqr   = range * range;
        GameObject best = null;

        for (int i = alive.Count - 1; i >= 0; i--)
        {
            EnemyHealth enemy = alive[i];
            if (enemy == null) { alive.RemoveAt(i); continue; }

            float sqr = (enemy.transform.position - from).sqrMagnitude;
            if (sqr <= bestSqr)
            {
                bestSqr = sqr;
                best    = enemy.gameObject;
            }
        }

        return best;
    }

    /// <summary>
    /// Enemigos dentro de un radio. Escribe en la lista que se le pasa para no
    /// asignar memoria en cada explosion.
    /// </summary>
    public static void FindInRadius(Vector3 center, float radius, List<EnemyHealth> results)
    {
        results.Clear();
        float sqrRadius = radius * radius;

        for (int i = alive.Count - 1; i >= 0; i--)
        {
            EnemyHealth enemy = alive[i];
            if (enemy == null) { alive.RemoveAt(i); continue; }

            if ((enemy.transform.position - center).sqrMagnitude <= sqrRadius)
                results.Add(enemy);
        }
    }

    /// <summary>
    /// El estado estatico sobrevive al cambio de escena y al "Enter Play Mode"
    /// sin domain reload. Limpiar aqui evita arrastrar enemigos fantasma que
    /// impedirian la victoria en la siguiente partida.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        alive.Clear();
    }
}
