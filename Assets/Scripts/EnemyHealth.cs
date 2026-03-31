using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth    = 50f;
    public float currentHealth;

    [Header("Recompensa")]
    public int crystalReward = 8;

    [Header("Barra de vida")]
    public EnemyHealthBar healthBar;

    private bool isBurning        = false;
    private float pendingBurnDamage = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        Invoke(nameof(InitHealthBar), 0.1f);        
    }

    void InitHealthBar ()
    {
        if (healthBar != null)
            healthBar.UpdateBar(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth  = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Daño recibido: {damage} | Vida actual: {currentHealth}/{maxHealth} | HealthBar: {healthBar}");

        if (healthBar != null)
            healthBar.UpdateBar(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    public void ApplyBurn(float burnDamage, float delay)
    {
        if (isBurning) return;
        isBurning          = true;
        pendingBurnDamage  = burnDamage;
        Invoke(nameof(BurnTick), delay);
    }

    void BurnTick()
    {
        isBurning = false;
        if (gameObject == null) return;
        TakeDamage(pendingBurnDamage);
    }

    void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddCrystals(crystalReward);
        Destroy(gameObject);
    }
}