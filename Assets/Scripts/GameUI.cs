using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [Header("Referencias UI")]
    public TextMeshProUGUI waveText;

    [Tooltip("Relleno de la barra de vida del castillo. Se escala en X. Antes lo " +
             "referenciaba CastleHealth, pero el castillo vive en la escena de " +
             "layout y Unity no serializa referencias entre escenas.")]
    public RectTransform castleHealthFill;

    [Header("Panel Game Over")]
    public GameObject gameOverPanel;

    [Header("Panel Victoria")]
    public GameObject victoryPanel;

    [Header("Mensajes breves (cristales insuficientes, recarga...)")]
    [Tooltip("Opcional. Si lo dejas vacio se crea solo al primer aviso, con la " +
             "misma fuente que el texto de oleadas.")]
    public TextMeshProUGUI toastText;

    [Tooltip("Posicion del aviso, medida desde el centro del borde inferior de " +
             "la pantalla. Por defecto queda justo encima de los botones.")]
    public Vector2 toastPosition = new Vector2(0f, 110f);

    public float toastDuration = 1.6f;

    private WaveManager waveManager;
    private int lastShownWave = -1;

    // Cada mensaje tiene su propia corrutina. Antes se usaba StopAllCoroutines,
    // y un aviso de oleada habria cortado un mensaje de cristales a medias.
    private Coroutine messageRoutine;
    private Coroutine toastRoutine;
    private CanvasGroup toastGroup;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        CastleHealth.HealthChanged += UpdateCastleHealth;

        // Por si el castillo ya existia antes de activarse esta UI.
        if (CastleHealth.Instance != null)
            UpdateCastleHealth(CastleHealth.Instance.currentHealth,
                               CastleHealth.Instance.maxHealth);
    }

    void OnDisable()
    {
        CastleHealth.HealthChanged -= UpdateCastleHealth;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        waveManager = FindFirstObjectByType<WaveManager>();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel  != null) victoryPanel.SetActive(false);

        ShowMessage("¡Defiende el castillo!", 3f);
    }

    void UpdateCastleHealth(float current, float max)
    {
        if (castleHealthFill == null) return;

        float fill = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        castleHealthFill.localScale = new Vector3(fill, 1f, 1f);
    }

    void Update()
    {
        if (waveManager == null || waveText == null) return;

        if (waveManager.currentWave != lastShownWave && waveManager.currentWave > 0)
        {
            lastShownWave = waveManager.currentWave;

            bool isLastWave = (waveManager.currentWave == waveManager.TotalBigWaves);

            if (isLastWave)
                ShowMessage("¡La oleada final!", 3f);
            else
                ShowMessage("Oleada " + waveManager.currentWave, 3f);
        }
    }

    void ShowMessage(string message, float duration)
    {
        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        if (waveText != null)
        {
            waveText.text = message;
            waveText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(duration);

        if (waveText != null)
            waveText.gameObject.SetActive(false);

        messageRoutine = null;
    }

    // ---------------------------------------------------------------------
    // Avisos breves
    // ---------------------------------------------------------------------

    /// <summary>
    /// Aviso corto encima de los botones. Si llega otro mientras se muestra uno,
    /// lo sustituye y reinicia el tiempo: pulsar varias veces un boton sin
    /// cristales no apila mensajes.
    /// </summary>
    public void ShowToast(string message)
    {
        EnsureToast();
        if (toastText == null) return;

        toastText.text = message;

        if (toastRoutine != null)
            StopCoroutine(toastRoutine);

        toastRoutine = StartCoroutine(ToastCoroutine());
    }

    IEnumerator ToastCoroutine()
    {
        GameObject root = toastGroup != null ? toastGroup.gameObject : toastText.gameObject;
        root.SetActive(true);

        if (toastGroup != null) toastGroup.alpha = 1f;

        // Tiempo sin escalar: el aviso debe verse igual aunque el juego este
        // pausado con timeScale = 0.
        yield return new WaitForSecondsRealtime(toastDuration);

        const float fade = 0.25f;
        for (float t = 0f; t < fade && toastGroup != null; t += Time.unscaledDeltaTime)
        {
            toastGroup.alpha = 1f - t / fade;
            yield return null;
        }

        root.SetActive(false);
        toastRoutine = null;
    }

    /// <summary>
    /// Crea el aviso si no se asigno uno en el Inspector: un panel oscuro con el
    /// texto encima, anclado abajo en el centro. Nada de esto recibe toques, para
    /// no bloquear los botones ni la colocacion de torres que hay debajo.
    /// </summary>
    void EnsureToast()
    {
        if (toastText != null)
        {
            // Texto asignado a mano: el CanvasGroup va en su propio objeto. No
            // se busca en los padres, porque ocultar el aviso desactivaria ese
            // padre y podria llevarse por delante medio HUD.
            if (toastGroup == null)
            {
                toastGroup = toastText.GetComponent<CanvasGroup>();
                if (toastGroup == null)
                    toastGroup = toastText.gameObject.AddComponent<CanvasGroup>();

                toastGroup.blocksRaycasts = false;
                toastGroup.interactable   = false;
            }
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        GameObject panel = new GameObject("Toast",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = (RectTransform)panel.transform;
        panelRect.anchorMin        = new Vector2(0.5f, 0f);
        panelRect.anchorMax        = new Vector2(0.5f, 0f);
        panelRect.pivot            = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = toastPosition;
        panelRect.sizeDelta        = new Vector2(620f, 46f);

        Image background = panel.GetComponent<Image>();
        background.color         = new Color(0f, 0f, 0f, 0.75f);
        background.raycastTarget = false;

        toastGroup = panel.GetComponent<CanvasGroup>();
        toastGroup.blocksRaycasts = false;
        toastGroup.interactable   = false;

        GameObject label = new GameObject("Text",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        label.transform.SetParent(panel.transform, false);

        RectTransform labelRect = (RectTransform)label.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(12f, 4f);
        labelRect.offsetMax = new Vector2(-12f, -4f);

        toastText = label.GetComponent<TextMeshProUGUI>();
        if (waveText != null) toastText.font = waveText.font;

        toastText.alignment        = TextAlignmentOptions.Center;
        toastText.color            = Color.white;
        toastText.raycastTarget    = false;
        toastText.enableAutoSizing = true;   // PressStart2P es ancha: que encoja
        toastText.fontSizeMin      = 8f;     // antes que salirse del panel
        toastText.fontSizeMax      = 16f;

        // El ultimo hijo se dibuja encima de todo el HUD.
        panel.transform.SetAsLastSibling();
        panel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void ShowVictory()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    public void OnRestartButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }
}
