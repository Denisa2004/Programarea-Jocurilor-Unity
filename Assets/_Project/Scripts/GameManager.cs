using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject gameOverPanel;

    bool isGameOver = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Shortcut for Quick Restart - R key
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Save collected coins into total coins
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SaveRunCoinsToTotal();
        }

        // Stop scoring and save high score
        ScoreManager scoreMgr = FindObjectOfType<ScoreManager>();
        if (scoreMgr != null)
        {
            scoreMgr.StopScoringAndSave();
        }

        // Pause the game
        Time.timeScale = 0f;

        // Trigger shadow enemy screen takeover, then show game over UI
        if (ShadowEnemy.Instance != null)
        {
            ShadowEnemy.Instance.TriggerScreenTakeover(() =>
            {
                if (gameOverPanel != null)
                    gameOverPanel.SetActive(true);
            });
        }
        else
        {
            // No shadow enemy, show game over directly
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
