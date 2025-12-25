using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Gestioneaza afisarea/ascunderea unui panel cu statistici detaliate
/// Apasa TAB pentru a toggle stats panel-ul
/// </summary>
public class StatsPanel : MonoBehaviour
{
    public static StatsPanel Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TextMeshProUGUI statsText;

    [Header("Settings")]
    [Tooltip("Tasta pentru toggle stats")]
    public KeyCode toggleKey = KeyCode.Tab;
    
    [Tooltip("Rata de actualizare a stats-urilor (secunde)")]
    public float updateRate = 0.5f;

    [Header("Optional: Show Individual Stats")]
    public bool showScore = true;
    public bool showCoins = true;
    public bool showHealth = true;
    public bool showSpeed = true;
    public bool showFPS = true;
    public bool showPlayTime = true;

    private bool isVisible = false;
    private float nextUpdateTime = 0f;
    private float startTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        startTime = Time.time;
    }

    private void Start()
    {
        // Ascunde panel-ul la inceput
        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
            
            // Pozitioneaza panel-ul in coltul dreapta-jos
            RectTransform panelRect = statsPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                // Ancora in coltul dreapta-jos
                panelRect.anchorMin = new Vector2(1, 0);
                panelRect.anchorMax = new Vector2(1, 0);
                panelRect.pivot = new Vector2(1, 0);
                
                // Seteaza o dimensiune potrivita pentru panel (mai inalt pentru tot continutul)
                panelRect.sizeDelta = new Vector2(300, 480);
                
                // Offset pentru margini
                panelRect.anchoredPosition = new Vector2(-20, 20);
            }
            
            // Centreaza textul in mijlocul panelului
            if (statsText != null)
            {
                statsText.alignment = TextAlignmentOptions.Top;
                // Adauga padding uniform
                statsText.margin = new Vector4(10, 10, 10, 10);
            }
        }
    }

    private void Update()
    {
        // Toggle stats panel cu tasta Tab
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleStatsPanel();
        }

        // Actualizeaza stats-urile doar daca panel-ul e vizibil
        if (isVisible && Time.time >= nextUpdateTime)
        {
            UpdateStatsDisplay();
            nextUpdateTime = Time.time + updateRate;
        }
    }

    public void ToggleStatsPanel()
    {
        isVisible = !isVisible;
        
        if (statsPanel != null)
        {
            statsPanel.SetActive(isVisible);
        }

        if (isVisible)
        {
            UpdateStatsDisplay();
            Debug.Log("?? Stats Panel SHOWN (press Tab to hide)");
        }
        else
        {
            Debug.Log("?? Stats Panel HIDDEN");
        }
    }

    private void UpdateStatsDisplay()
    {
        if (statsText == null) return;

        string stats = BuildStatsString();
        statsText.text = stats;
    }

    private string BuildStatsString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<color=yellow>GAME STATS</color>\n");

        // Score Stats
        if (showScore)
        {
            var scoreMgr = FindObjectOfType<ScoreManager>();
            if (scoreMgr != null)
            {
                // Folosim reflection sau o proprietate publica pentru a accesa currentScore
                int score = GetCurrentScore();
                int highScore = PlayerPrefs.GetInt("HighScore", 0);
                
                sb.AppendLine($"<color=#FFD700>SCORE</color>");
                sb.AppendLine($"  Current: <b>{score}</b>");
                sb.AppendLine($"  High Score: <b>{highScore}</b>");
                sb.AppendLine($"  Rate: +{scoreMgr.pointsPerSecond:F1}/sec\n");
            }
        }

        // Coins Stats
        if (showCoins && CoinManager.Instance != null)
        {
            sb.AppendLine($"<color=#FFD700>COINS</color>");
            sb.AppendLine($"  This Run: <b>{CoinManager.Instance.CurrentRunCoins}</b>");
            sb.AppendLine($"  Total Saved: <b>{CoinManager.Instance.TotalCoins}</b>\n");
        }

        // Health Stats
        if (showHealth && PlayerHealth.Instance != null)
        {
            float healthPercent = PlayerHealth.Instance.health * 100f;
            string healthBar = CreateHealthBar(PlayerHealth.Instance.health);
            
            sb.AppendLine($"<color=#FF6B6B>HEALTH</color>");
            sb.AppendLine($"  {healthBar} <b>{healthPercent:F0}%</b>\n");
        }

        // Speed Stats
        if (showSpeed)
        {
            var movement = FindObjectOfType<MovementScript>();
            if (movement != null)
            {
                float currentSpeed = movement.GetForwardSpeed();
                var scoreMgr = FindObjectOfType<ScoreManager>();
                float maxSpeed = scoreMgr != null ? scoreMgr.maxForwardSpeed : 20f;
                
                sb.AppendLine($"<color=#00D9FF>SPEED</color>");
                sb.AppendLine($"  Current: <b>{currentSpeed:F1}</b> m/s");
                sb.AppendLine($"  Max: <b>{maxSpeed:F1}</b> m/s\n");
            }
        }

        // FPS Stats
        if (showFPS)
        {
            float fps = 1f / Time.unscaledDeltaTime;
            string fpsColor = fps >= 50 ? "green" : fps >= 30 ? "yellow" : "red";
            
            sb.AppendLine($"<color=#9D4EDD>PERFORMANCE</color>");
            sb.AppendLine($"  FPS: <color={fpsColor}><b>{fps:F0}</b></color>");
        }

        // Play Time
        if (showPlayTime)
        {
            float playTime = Time.time - startTime;
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);
            
            sb.AppendLine($"  Time: <b>{minutes:00}:{seconds:00}</b>");
        }

        return sb.ToString();
    }

    private int GetCurrentScore()
    {
        var scoreMgr = FindObjectOfType<ScoreManager>();
        if (scoreMgr == null) return 0;

        // Cautam field-ul privat 'currentScore' prin reflection
        var field = typeof(ScoreManager).GetField("currentScore", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            float score = (float)field.GetValue(scoreMgr);
            return (int)score;
        }
        
        return 0;
    }

    private string CreateHealthBar(float healthPercent)
    {
        int barLength = 10;
        int filledBars = Mathf.RoundToInt(healthPercent * barLength);
        
        string bar = "";
        for (int i = 0; i < barLength; i++)
        {
            bar += i < filledBars ? "?" : "?";
        }
        return bar;
    }

    // Metode publice pentru toggle individual
    public void Show()
    {
        isVisible = true;
        if (statsPanel != null) statsPanel.SetActive(true);
        UpdateStatsDisplay();
    }

    public void Hide()
    {
        isVisible = false;
        if (statsPanel != null) statsPanel.SetActive(false);
    }
}
