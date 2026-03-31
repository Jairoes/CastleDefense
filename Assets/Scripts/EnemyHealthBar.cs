using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    private Canvas canvas;
    private Slider slider;
    private float offsetY = 1.5f;

    void Start()
    {
        // Crear Canvas en World Space
        GameObject canvasGO = new GameObject("HealthCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = new Vector3(0, offsetY, 0);

        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1.5f, 0.4f);
        canvasGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // Crear Slider
        GameObject sliderGO = new GameObject("HealthSlider");
        sliderGO.transform.SetParent(canvasGO.transform, false);

        slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value    = 1f;

        RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.sizeDelta        = new Vector2(150f, 40f);
        sliderRect.anchoredPosition = Vector2.zero;

        // Fondo rojo
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderGO.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.6f, 0f, 0f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        // Fill verde
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillArea.transform, false);
        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = new Color(0f, 0.8f, 0f);
        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        // Asignar fill al slider
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;

        // Desactivar interacción
        slider.interactable = false;
    }

    void LateUpdate()
    {
        // Siempre mirar hacia la cámara (horizontal)
        if (canvas != null)
            canvas.transform.rotation = Camera.main.transform.rotation;
    }

    public void UpdateBar(float currentHealth, float maxHealth)
    {
        if (slider == null) return;
        slider.value = Mathf.Clamp01(currentHealth / maxHealth);
    }
}