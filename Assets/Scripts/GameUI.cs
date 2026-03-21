using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Slider castleHealthBar;
    public TextMeshProUGUI waveText;

    private CastleHealth castleHealth;
    private WaveManager waveManager;

    void Start()
    {
        castleHealth = FindFirstObjectByType<CastleHealth>();
        waveManager = FindFirstObjectByType<WaveManager>();

        if (castleHealth != null)
        {
            castleHealthBar.maxValue = castleHealth.maxHealth;
            castleHealthBar.value = castleHealth.currentHealth;
        }
    }

    void Update()
    {
        if (castleHealth != null)
        {
            castleHealthBar.value = castleHealth.currentHealth;
        }

        if (waveManager != null)
        {
            waveText.text = "Oleada " + waveManager.currentWave;
        }
    }
}