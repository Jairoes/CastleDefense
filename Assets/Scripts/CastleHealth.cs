using UnityEngine;

public class CastleHealth : MonoBehaviour
{
    [Header("Vida del Castillo")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public RectTransform healthBarFill;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        if (currentHealth <= 0)
            GameOver();
    }

    void UpdateUI()
    {
        if (healthBarFill != null)
        {
            float fillAmount = currentHealth / maxHealth;
            healthBarFill.localScale = new Vector3(fillAmount, 1, 1);
        }
    }

    void GameOver()
    {
        Debug.Log("Game Over! El castillo fue destruido.");
    }
}