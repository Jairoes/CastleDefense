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

    public enum RemainsType
    {
        [InspectorName("Sangre")]         Blood,
        [InspectorName("Huesos y polvo")] Bones,
        [InspectorName("Nada")]           None,
    }

    [Header("Restos al morir")]
    [Tooltip("Que deja al morir. Cada personaje el suyo: los esqueletos no sangran.")]
    public RemainsType remainsType = RemainsType.Blood;

    [Tooltip("Color de la sangre o de los huesos. Conviene sacarlo de los colores " +
             "del propio personaje.")]
    public Color remainsColor = new Color(0.20f, 0.45f, 0.18f, 1f);

    [Tooltip("Ancho de la mancha respecto al ancho del enemigo. 1 = igual de ancha.")]
    public float remainsScale = 0.9f;

    [Tooltip("Opcional: tus propios dibujos de la mancha; se elige uno al azar. " +
             "Vacio = se usa uno generado en pixel art.")]
    public Sprite[] remainsSprites;

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

        SpawnRemains();

        if (GameManager.Instance != null)
            GameManager.Instance.AddCrystals(crystalReward);

        Destroy(gameObject);
    }

    // ---------------------------------------------------------------------
    // Restos al morir
    // ---------------------------------------------------------------------

    void SpawnRemains()
    {
        if (remainsType == RemainsType.None) return;

        EnemyMovement movement = GetComponent<EnemyMovement>();
        Vector3 body = movement != null ? movement.BodyCenter : transform.position;

        // El ancho del propio sprite: asi la mancha escala sola con el personaje.
        float width = movement != null ? movement.BodySize.x : 1f;
        float k     = Mathf.Clamp(width, 0.6f, 2.5f);   // escala de la salpicadura

        // La mancha va bajo los pies (la raiz del prefab), no bajo el centro del
        // sprite, que al mirar a camara queda desplazado hacia delante.
        GroundDecals.Spawn(transform.position, PickRemainsSprite(),
                           remainsColor, width * remainsScale);

        if (remainsType == RemainsType.Blood)
            SplashBlood(body, k);
        else
            SplashBones(body, k);
    }

    Sprite PickRemainsSprite()
    {
        if (remainsSprites != null && remainsSprites.Length > 0)
        {
            Sprite chosen = remainsSprites[Random.Range(0, remainsSprites.Length)];
            if (chosen != null) return chosen;
        }

        int variant = Random.Range(0, RuntimeSprites.RemainsVariants);
        return remainsType == RemainsType.Bones
            ? RuntimeSprites.BoneBits(variant)
            : RuntimeSprites.Splat(variant);
    }

    /// <summary>Gotas que saltan y caen.</summary>
    void SplashBlood(Vector3 at, float k)
    {
        SpriteParticles.Burst(at, new SpriteParticles.Settings
        {
            sprite       = RuntimeSprites.Droplet,
            count        = 9,
            spawnRadius  = 0.1f * k,
            speedMin     = 1.2f,  speedMax = 2.6f,
            riseMin      = 1.5f,  riseMax  = 2.6f,
            gravity      = 9f,
            drag         = 0.4f,
            lifeMin      = 0.35f, lifeMax  = 0.55f,
            sizeStart    = 0.2f  * k,
            sizeEnd      = 0.14f * k,
            colorA       = remainsColor,
            colorB       = Color.Lerp(remainsColor, Color.black, 0.3f),
            sortingOrder = 4,
        });
    }

    /// <summary>Nube de polvo y trocitos de hueso que saltan.</summary>
    void SplashBones(Vector3 at, float k)
    {
        Color dust = Color.Lerp(remainsColor, Color.gray, 0.45f);
        dust.a = 0.75f;

        SpriteParticles.Burst(at, new SpriteParticles.Settings
        {
            sprite       = RuntimeSprites.Puff,
            count        = 6,
            spawnRadius  = 0.3f * k,
            speedMin     = 0.3f,  speedMax = 0.8f,
            riseMin      = 0.3f,  riseMax  = 0.8f,
            drag         = 2f,
            lifeMin      = 0.5f,  lifeMax  = 0.8f,
            sizeStart    = 0.4f * k,
            sizeEnd      = 0.9f * k,
            colorA       = dust,
            colorB       = Color.Lerp(dust, Color.white, 0.3f),
            sortingOrder = 4,
        });

        SpriteParticles.Burst(at, new SpriteParticles.Settings
        {
            sprite       = RuntimeSprites.Droplet,
            count        = 6,
            spawnRadius  = 0.1f * k,
            speedMin     = 1.0f,  speedMax = 2.0f,
            riseMin      = 1.5f,  riseMax  = 2.2f,
            gravity      = 9f,
            drag         = 0.4f,
            lifeMin      = 0.35f, lifeMax  = 0.5f,
            sizeStart    = 0.16f * k,
            sizeEnd      = 0.12f * k,
            colorA       = remainsColor,
            colorB       = Color.Lerp(remainsColor, Color.white, 0.3f),
            sortingOrder = 5,
        });
    }
}
