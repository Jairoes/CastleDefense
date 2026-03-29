using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Cristales")]
    public int crystals = 100;
    public TextMeshProUGUI crystalsText;

    [Header("Estado del juego")]
    public bool gameOver = false;
    public bool gameWon  = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateCrystalsUI();
    }

    public bool SpendCrystals(int amount)
    {
        if (gameOver || gameWon) return false;

        if (crystals >= amount)
        {
            crystals -= amount;
            UpdateCrystalsUI();
            return true;
        }
        Debug.Log("No hay suficientes cristales!");
        return false;
    }

    public void AddCrystals(int amount)
    {
        crystals += amount;
        UpdateCrystalsUI();
        Debug.Log("+" + amount + " cristales. Total: " + crystals);
    }

    public void TriggerGameOver()
    {
        if (gameOver || gameWon) return;
        gameOver = true;
        Time.timeScale = 0f; // pausa el juego
        GameUI.Instance.ShowGameOver();
    }

    public void TriggerVictory()
    {
        if (gameOver || gameWon) return;
        gameWon = true;
        Time.timeScale = 0f; // pausa el juego
        GameUI.Instance.ShowVictory();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void UpdateCrystalsUI()
    {
        if (crystalsText != null)
            crystalsText.text = "💎 " + crystals;
    }
}