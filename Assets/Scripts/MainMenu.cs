using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Escena de juego a cargar desde el menu.")]
    public string gameSceneName = "Forest_Level_01";

    public void PlayGame()
    {
        // Si se llega aqui tras un Game Over, timeScale sigue a 0.
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
