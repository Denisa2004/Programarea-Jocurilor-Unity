using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    public float pointsPerSecond = 5f;

    private float currentScore = 0f;

    public bool isScoring = true;

    void Update()
    {
        if (isScoring)
        {
            currentScore += pointsPerSecond * Time.deltaTime;

            scoreText.text = "Score: " + (int)currentScore;
        }
    }
}