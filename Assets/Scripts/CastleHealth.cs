using UnityEngine;
using System.Collections;

public class CastleHealth : MonoBehaviour
{
    [Header("Vida del Castillo")]
    public float maxHealth     = 100f;
    public float currentHealth;

    [Header("UI")]
    public RectTransform healthBarFill;

    [Header("Efecto de destrucción")]
    public float shakeDuration  = 0.5f;
    public float shakeMagnitude = 0.3f;
    public GameObject castleDestroyedSprite;

    private Vector3 originalPosition;
    private bool isDestroyed = false;

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
        if (GameManager.Instance.gameOver) return;
        if (isDestroyed) return;

        currentHealth -= damage;
        currentHealth  = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        StopAllCoroutines();
        StartCoroutine(Shake(0.15f, 0.1f));

        if (currentHealth <= 0)
            StartCoroutine(DestroySequence());
    }

    IEnumerator DestroySequence()
    {
        isDestroyed = true;

        // 1. Shake fuerte
        yield return StartCoroutine(Shake(shakeDuration, shakeMagnitude));

        // 2. Partículas
        SpawnDustParticles();

        // 3. Activar sprite destruido PRIMERO
        if (castleDestroyedSprite != null)
            castleDestroyedSprite.SetActive(true);

        // 4. Ocultar renderers EXCEPTO el destruido
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (castleDestroyedSprite != null &&
                r.transform.IsChildOf(castleDestroyedSprite.transform))
                continue;
            r.enabled = false;
        }

        // 5. Pausa dramática
        yield return new WaitForSecondsRealtime(0.8f);

        // 6. Game Over
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
            float fillAmount = currentHealth / maxHealth;
            healthBarFill.localScale = new Vector3(fillAmount, 1, 1);
        }
    }
}