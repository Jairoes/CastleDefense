using UnityEngine;
using TMPro;
using System.Collections;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [Header("Referencias UI")]
    public TextMeshProUGUI waveText;

    [Tooltip("Relleno de la barra de vida del castillo. Se escala en X. Antes lo " +
             "referenciaba CastleHealth, pero el castillo vive en la escena de " +
             "layout y Unity no serializa referencias entre escenas.")]
    public RectTransform castleHealthFill;

    [Header("Panel Game Over")]
    public GameObject gameOverPanel;

    [Header("Panel Victoria")]
    public GameObject victoryPanel;

    private WaveManager waveManager;
    private int lastShownWave = -1;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        CastleHealth.HealthChanged += UpdateCastleHealth;

        // Por si el castillo ya existia antes de activarse esta UI.
        if (CastleHealth.Instance != null)
            UpdateCastleHealth(CastleHealth.Instance.currentHealth,
                               CastleHealth.Instance.maxHealth);
    }

    void OnDisable()
    {
        CastleHealth.HealthChanged -= UpdateCastleHealth;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        waveManager = FindFirstObjectByType<WaveManager>();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel  != null) victoryPanel.SetActive(false);

        ShowMessage("¡Defiende el castillo!", 3f);
    }

    void UpdateCastleHealth(float current, float max)
    {
        if (castleHealthFill == null) return;

        float fill = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        castleHealthFill.localScale = new Vector3(fill, 1f, 1f);
    }

    void Update()
    {
        if (waveManager == null || waveText == null) return;

        if (waveManager.currentWave != lastShownWave && waveManager.currentWave > 0)
        {
            lastShownWave = waveManager.currentWave;

            bool isLastWave = (waveManager.currentWave == waveManager.TotalBigWaves);

            if (isLastWave)
                ShowMessage("¡La oleada final!", 3f);
            else
                ShowMessage("Oleada " + waveManager.currentWave, 3f);
        }
    }

    void ShowMessage(string message, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        if (waveText != null)
        {
            waveText.text = message;
            waveText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(duration);

        if (waveText != null)
            waveText.gameObject.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void ShowVictory()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    public void OnRestartButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }
}
