using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida del Enemigo")]
    public float maxHealth = 50f;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemigo eliminado!");
        Destroy(gameObject);
    }
}