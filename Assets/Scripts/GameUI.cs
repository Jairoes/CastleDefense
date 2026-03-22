using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI waveText;

    private WaveManager waveManager;

    void Start()
    {
        waveManager = FindFirstObjectByType<WaveManager>();
    }

    void Update()
    {
        if (waveManager != null)
        {
            waveText.text = "Oleada " + waveManager.currentWave;
        }
    }
}