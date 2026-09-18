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

    // Ultimo frame en que recibio dano de fuego. Lo comparten todas las zonas en
    // llamas: aunque se solapen varias, el enemigo solo arde una vez por frame.
    private int lastBurnFrame = -1;

    // Cuando pueden volver a salirle chispas, para no soltarlas cada frame.
    private float nextBurnFxTime = 0f;

    private bool isDead = false;

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
        // Destroy no actua hasta el final del frame, asi que un enemigo que acaba
        // de morir sigue recibiendo golpes hasta entonces. Sin este corte, una
        // flecha y el fuego rematandolo en el mismo frame llamaban dos veces a
        // Die() y el jugador cobraba la recompensa dos veces.
        if (isDead) return;

        currentHealth -= damage;
        currentHealth  = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (healthBar != null)
            healthBar.UpdateBar(currentHealth, maxHealth);

        if (currentHealth <= 0f)
            Die();
    }

    /// <summary>
    /// Aplica el dano de fuego de este frame, salvo que otra zona ya lo haya
    /// hecho: estar dentro de dos llamas solapadas quema igual que estar en una.
    /// </summary>
    public bool TryBurn(float damage)
    {
        if (isDead || lastBurnFrame == Time.frameCount) return false;

        lastBurnFrame = Time.frameCount;
        TakeDamage(damage);
        return true;
    }

    /// <summary>True si toca soltar chispas ahora; reserva el siguiente turno.</summary>
    public bool TryBurnFx(float interval)
    {
        if (isDead || Time.time < nextBurnFxTime) return false;

        nextBurnFxTime = Time.time + interval;
        return true;
    }

    void Die()
    {
        isDead = true;

        // Fuera del registro ya, sin esperar al OnDisable de fin de frame: asi
        // ninguna torre lo elige como objetivo en ese ultimo instante. Llamar
        // despues otra vez a Unregister desde OnDisable no hace nada.
        EnemyRegistry.Unregister(this);

        if (GameManager.Instance != null)
            GameManager.Instance.AddCrystals(crystalReward);

        Destroy(gameObject);
    }
}
