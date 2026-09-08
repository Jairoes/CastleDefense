using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 50f;
    public float currentHealth;

    [Header("Recompensa")]
    public int crystalReward = 8;

    [Header("Barra de vida")]
    public EnemyHealthBar healthBar;

    private bool isBurning         = false;
    private float pendingBurnDamage = 0f;

    void OnEnable()
    {
        EnemyRegistry.Register(this);
    }

    void OnDisable()
    {
        EnemyRegistry.Unregister(this);
    }

    void Start()
    {
        currentHealth = maxHealth;

        // La barra se construye en su propio Awake, que ya ha corrido para
        // cuando llega este Start. Antes hacia falta un Invoke con 0.1s de
        // retraso porque la barra se montaba en Start y el orden no estaba
        // garantizado.
        if (healthBar != null)
            healthBar.UpdateBar(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth  = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (healthBar != null)
            healthBar.UpdateBar(currentHealth, maxHealth);

        if (currentHealth <= 0f)
            Die();
    }

    public void ApplyBurn(float burnDamage, float delay)
    {
        if (isBurning) return;

        isBurning         = true;
        pendingBurnDamage = burnDamage;
        Invoke(nameof(BurnTick), delay);
    }

    void BurnTick()
    {
        isBurning = false;
        TakeDamage(pendingBurnDamage);
    }

    void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddCrystals(crystalReward);

        Destroy(gameObject);
    }
}
