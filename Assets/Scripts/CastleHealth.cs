using UnityEngine;

public class CastleHealth : MonoBehaviour
{
    [Header("Vida del Castillo")]
    public float maxHealth    = 100f;
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
        if (GameManager.Instance.gameOver) return;

        currentHealth -= damage;
        currentHealth  = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        if (currentHealth <= 0)
            GameManager.Instance.TriggerGameOver();
    }

    void UpdateUI()
    {
        if (healthBarFill != null)
        {
            float fillAmount = currentHealth / maxHealth;
            healthBarFill.localScale = new Vector3(fillAmount, 1, 1);
        }
    }
}