using UnityEngine;
using TMPro;

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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        waveManager = FindFirstObjectByType<WaveManager>();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel  != null) victoryPanel.SetActive(false);
    }

    void Update()
    {
        if (waveManager != null)
            waveText.text = "Oleada " + waveManager.currentWave;
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