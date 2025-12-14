using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    public float pointsPerSecond = 5f;

    private float currentScore = 0f;
    private int highScore = 0;

    public bool isScoring = true;

    private const string HIGH_SCORE_KEY = "HighScore";

    void Start()
    {
        // Load saved high score
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

        UpdateUI();
    }

    void Update()
    {
        if (isScoring)
        {
            currentScore += pointsPerSecond * Time.deltaTime;
            UpdateUI();
        }
    }

    public void StopScoringAndSave()
    {
        isScoring = false;

        int finalScore = (int)currentScore;

        // Update high score if needed
        if (finalScore > highScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "Score: " + (int)currentScore +
                "\nHigh Score: " + highScore;
        }
    }
}
