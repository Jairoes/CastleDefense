using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 50f;
    public float currentHealth;

    [Header("Recompensa")]
    public int crystalReward = 8;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddCrystals(crystalReward);

        Debug.Log("Enemigo eliminado! +" + crystalReward + " cristales");
        Destroy(gameObject);
    }
}