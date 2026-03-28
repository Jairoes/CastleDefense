using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 50f;
    public float currentHealth;

    [Header("Recompensa")]
    public int crystalReward = 8;

    // --- SISTEMA DE QUEMADURA ---
    private bool isBurning = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    // Llamado por FireProjectile al impactar
    public void ApplyBurn(float burnDamage, float delay)
    {
        if (isBurning) return; // si ya está quemándose, ignorar
        isBurning = true;
        Invoke(nameof(BurnTick), delay);
        // guardamos el daño para usarlo en BurnTick
        pendingBurnDamage = burnDamage;
    }

    private float pendingBurnDamage = 0f;

    void BurnTick()
    {
        isBurning = false;
        if (gameObject == null) return; // por si murió antes del tick
        TakeDamage(pendingBurnDamage);
    }

    void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddCrystals(crystalReward);
        Debug.Log("Enemigo eliminado! +" + crystalReward + " cristales");
        Destroy(gameObject);
    }
}