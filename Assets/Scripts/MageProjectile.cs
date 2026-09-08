using System.Collections.Generic;
using UnityEngine;

public class MageProjectile : MonoBehaviour
{
    private GameObject target;
    private float damage;
    private float splashDamage;
    private float splashRadius;
    private float speed = 18f;

    // Compartida entre explosiones: evita asignar una lista en cada impacto.
    private static readonly List<EnemyHealth> splashHits = new List<EnemyHealth>(16);

    public void SetTarget(GameObject _target, float _damage, float _splashDamage, float _splashRadius)
    {
        target       = _target;
        damage       = _damage;
        splashDamage = _splashDamage;
        splashRadius = _splashRadius;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction       = target.transform.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            Explode();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
    }

    void Explode()
    {
        EnemyHealth primary = target.GetComponent<EnemyHealth>();
        if (primary != null)
            primary.TakeDamage(damage);

        // Antes: Physics.OverlapSphere sin mascara de capas, que devolvia todos
        // los colliders del radio (terreno, torres, decoracion) para filtrarlos
        // despues por tag. El registro da directamente los enemigos.
        EnemyRegistry.FindInRadius(transform.position, splashRadius, splashHits);

        for (int i = 0; i < splashHits.Count; i++)
        {
            if (splashHits[i] == primary) continue;
            splashHits[i].TakeDamage(splashDamage);
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}
