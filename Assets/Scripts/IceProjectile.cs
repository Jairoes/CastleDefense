using System.Collections.Generic;
using UnityEngine;

public class IceProjectile : MonoBehaviour
{
    private GameObject target;
    private float damage;
    private float slowPercent;
    private float slowDuration;
    private float slowRadius;
    private Color frostColor = Color.white;
    private float speed = 18f;

    // Compartida entre explosiones: evita asignar una lista en cada impacto.
    private static readonly List<EnemyHealth> slowHits = new List<EnemyHealth>(16);

    public void SetTarget(GameObject _target, float _damage, float _slowPercent,
                          float _slowDuration, float _slowRadius, Color _frostColor)
    {
        target       = _target;
        damage       = _damage;
        slowPercent  = _slowPercent;
        slowDuration = _slowDuration;
        slowRadius   = _slowRadius;
        frostColor   = _frostColor;
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

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void Explode()
    {
        EnemyHealth primary = target.GetComponent<EnemyHealth>();
        if (primary != null)
            primary.TakeDamage(damage);

        EnemyMovement primaryMovement = target.GetComponent<EnemyMovement>();
        if (primaryMovement != null)
            primaryMovement.ApplySlow(slowPercent, slowDuration, frostColor);

        // La onda se dibuja con el mismo radio con el que se buscan enemigos
        // justo debajo: lo que el jugador ve es exactamente lo que se ralentiza.
        FrostBurstEffect.Spawn(transform.position, slowRadius, frostColor);

        // Antes: Physics.OverlapSphere sin mascara de capas. El registro da
        // directamente los enemigos dentro del radio.
        EnemyRegistry.FindInRadius(transform.position, slowRadius, slowHits);

        for (int i = 0; i < slowHits.Count; i++)
        {
            if (slowHits[i] == primary) continue;

            EnemyMovement movement = slowHits[i].GetComponent<EnemyMovement>();
            if (movement != null)
                movement.ApplySlow(slowPercent, slowDuration, frostColor);
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, slowRadius);
    }
}
