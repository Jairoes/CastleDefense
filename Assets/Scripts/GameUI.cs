using UnityEngine;
using TMPro;
using System.Collections;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [Header("Referencias UI")]
    public TextMeshProUGUI waveText;

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

    void Start()
    {
        waveManager = FindFirstObjectByType<WaveManager>();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel  != null) victoryPanel.SetActive(false);

        // Mensaje de inicio
        ShowMessage("¡Defiende el castillo!", 3f);
    }

    void Update()
    {
        if (waveManager == null || waveText == null) return;

        // Detectar cuando cambia la oleada para mostrar mensaje
        if (waveManager.currentWave != lastShownWave && waveManager.currentWave > 0)
        {
            lastShownWave = waveManager.currentWave;

            bool isLastWave = (waveManager.currentWave == waveManager.bigWaves.Count);

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
        GameManager.Instance.RestartGame();
    }
}