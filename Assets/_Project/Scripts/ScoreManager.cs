using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI scoreText;

    [Header("Scoring")]
    public float pointsPerSecond = 5f;
    public bool isScoring = true;

    [Header("Difficulty / Speed")]
    public MovementScript playerMovement;  
    public int pointsStep = 50;            
    public float speedIncrease = 0.75f;   
    public float maxForwardSpeed = 20f;  

    private float currentScore = 0f;
    private int highScore = 0;
    private int nextThreshold;

    private const string HIGH_SCORE_KEY = "HighScore";

    private void Start()
    {
        // Load saved high score (folosit doar la Game Over / Stats)
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

        nextThreshold = pointsStep;

        if (playerMovement != null && playerMovement.GetForwardSpeed() <= 0f)
        {
            playerMovement.SetForwardSpeed(2f);
        }

        UpdateUI();
    }

    private void Update()
    {
        if (!isScoring)
            return;

        currentScore += pointsPerSecond * Time.deltaTime;

        int displayScore = (int)currentScore;
        while (displayScore >= nextThreshold)
        {
            IncreasePlayerSpeed();
            nextThreshold += pointsStep;
        }

        UpdateUI();
    }

    public void StopScoringAndSave()
    {
        isScoring = false;

        int finalScore = (int)currentScore;

        if (finalScore > highScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
        }

        // La Game Over, UI-ul de in?game NU mai afi?eaz? high score,
        // game over panel-ul se ocup? de afi?are.
        UpdateUI();
    }

    private void IncreasePlayerSpeed()
    {
        if (playerMovement == null)
            return;

        float currentForward = playerMovement.GetForwardSpeed();
        float newForward = Mathf.Min(currentForward + speedIncrease, maxForwardSpeed);
        playerMovement.SetForwardSpeed(newForward);

        Debug.Log($"[ScoreManager] Speed increased: at score {(int)currentScore}");
    }

    private void UpdateUI()
    {
        if (scoreText == null)
            return;

        // DOAR scorul curent, ca în screenshot-ul dorit
        scoreText.text = "Score: " + (int)currentScore;
    }

    // Op?ional: metode publice pentru GameOver UI / StatsPanel
    public int GetCurrentScoreInt() => (int)currentScore;
    public int GetHighScore() => highScore;
}
