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

        // El Game Over y la victoria dejan timeScale a 0. Si se vuelve a esta
        // escena por cualquier via que no sea RestartGame (por ejemplo desde el
        // menu principal), la partida arrancaria congelada.
        Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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

        return false;
    }

    public void AddCrystals(int amount)
    {
        crystals += amount;
        UpdateCrystalsUI();
    }

    public void TriggerGameOver()
    {
        if (gameOver || gameWon) return;

        gameOver = true;

        if (GameUI.Instance != null)
            GameUI.Instance.ShowGameOver();

        Invoke(nameof(PauseGame), 0.1f);
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void TriggerVictory()
    {
        if (gameOver || gameWon) return;

        gameWon = true;
        Time.timeScale = 0f;

        if (GameUI.Instance != null)
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
            crystalsText.text = crystals.ToString();
    }
}
