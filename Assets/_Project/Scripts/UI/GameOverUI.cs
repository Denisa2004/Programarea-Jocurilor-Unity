using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI totalCoinsText;

    private void OnEnable()
    {
        UpdateTexts();
    }

    private void UpdateTexts()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        var scoreMgr = FindFirstObjectByType<ScoreManager>();
        if (scoreMgr != null)
        {
            highScore = scoreMgr.GetHighScore();
        }

        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;

        int totalCoins = 0;
        if (CoinManager.Instance != null)
            totalCoins = CoinManager.Instance.TotalCoins;
        else
            totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (totalCoinsText != null)
            totalCoinsText.text = "Total Coins: " + totalCoins;
    }
}