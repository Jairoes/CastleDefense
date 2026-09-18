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

    // Momento a partir del cual puede volver a recibir dano de fuego. Lo comparten
    // todas las zonas en llamas: aunque se solapen varias, el enemigo recibe como
    // mucho un golpe de fuego por intervalo.
    private float nextBurnTime = 0f;

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

    /// <summary>
    /// Pregunta si este enemigo puede recibir ahora un golpe de fuego. Si puede,
    /// lo reserva y devuelve true; si ya le quemo otra zona hace poco, false.
    ///
    /// El bloqueo dura el 90 % del intervalo y no el 100 %: el temporizador de
    /// cada zona acumula pequenos errores de coma flotante, y si su golpe cayera
    /// una milesima antes de liberarse el bloqueo, se saltaria un intervalo
    /// entero y el fuego quemaria a la mitad de ritmo.
    /// </summary>
    public bool TryBurnTick(float interval)
    {
        if (Time.time < nextBurnTime) return false;

        nextBurnTime = Time.time + interval * 0.9f;
        return true;
    }

    void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddCrystals(crystalReward);

        Destroy(gameObject);
    }
}
