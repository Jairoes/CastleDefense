using UnityEngine;
using System.Collections;

public class CastleHealth : MonoBehaviour
{
    /// <summary>
    /// Referencia unica al castillo activo. Antes cada enemigo hacia su propio
    /// FindFirstObjectByType al nacer, que recorre toda la escena; con oleadas
    /// de 30+ enemigos eran decenas de barridos completos.
    /// </summary>
    public static CastleHealth Instance;

    [Header("Vida del Castillo")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public RectTransform healthBarFill;

    [Header("Efecto de destruccion")]
    public float shakeDuration  = 0.5f;
    public float shakeMagnitude = 0.3f;
    public GameObject castleDestroyedSprite;

    private Vector3 originalPosition;
    private bool isDestroyed = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        currentHealth    = maxHealth;
        originalPosition = transform.position;
        UpdateUI();

        if (castleDestroyedSprite != null)
            castleDestroyedSprite.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        if (isDestroyed) return;
        if (GameManager.Instance != null && GameManager.Instance.gameOver) return;

        currentHealth -= damage;
        currentHealth  = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();

        if (currentHealth <= 0f)
        {
            Destruir();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(Shake(0.15f, 0.1f));
    }

    void Destruir()
    {
        isDestroyed = true;

        if (castleDestroyedSprite != null)
            castleDestroyedSprite.SetActive(true);

        // Ocultar todo menos el sprite de castillo destruido.
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (castleDestroyedSprite != null &&
                r.transform.IsChildOf(castleDestroyedSprite.transform))
                continue;

            r.enabled = false;
        }

        // Las particulas son decorativas: si fallan, el Game Over debe ocurrir
        // igualmente.
        try { SpawnDustParticles(); }
        catch (System.Exception e) { Debug.LogWarning("Particulas de polvo fallaron: " + e.Message); }

        if (GameManager.Instance != null)
            GameManager.Instance.TriggerGameOver();
    }

    IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude;

            transform.position = originalPosition + new Vector3(x, 0f, z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }

    void SpawnDustParticles()
    {
        GameObject dustGO = new GameObject("DustEffect");
        dustGO.transform.position = transform.position;

        ParticleSystem ps = dustGO.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main             = ps.main;
        main.loop            = false;
        main.startLifetime   = 1.5f;
        main.startSpeed      = 4f;
        main.startSize       = 0.5f;
        main.startColor      = new Color(0.8f, 0.7f, 0.5f, 1f);
        main.maxParticles    = 50;
        main.gravityModifier = 0.3f;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 50)
        });

        var shape       = ps.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 1.5f;

        var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
        psRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));

        ps.Play();
        Destroy(dustGO, 3f);
    }

    void UpdateUI()
    {
        if (healthBarFill != null)
        {
            float fillAmount = maxHealth > 0f ? currentHealth / maxHealth : 0f;
            healthBarFill.localScale = new Vector3(fillAmount, 1f, 1f);
        }
    }
}
