using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Cristales")]
    public int crystals = 100;
    public TextMeshProUGUI crystalsText;

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

    void UpdateCrystalsUI()
    {
        if (crystalsText != null)
            crystalsText.text = "💎 " + crystals;
    }
}