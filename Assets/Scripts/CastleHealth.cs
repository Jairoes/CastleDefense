using UnityEngine;
using UnityEngine.UI;

public class CastleHealth : MonoBehaviour
{
    [Header("Vida del Castillo")]
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log("Castillo recibio daño. Vida restante: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("¡Game Over! El castillo fue destruido.");
        // Aquí después pausaremos el juego y mostraremos la pantalla de derrota
    }
}