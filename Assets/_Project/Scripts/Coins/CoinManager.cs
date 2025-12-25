using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI coinsText;

    // coins collected during the current run
    public int CurrentRunCoins { get; private set; }

    // total coins saved between sessions


    public int TotalCoins { get; private set; }
    
    private int coinMultiplier = 1;

    private const string TOTAL_COINS_KEY = "TotalCoins";

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 1) Load total coins saved from previous sessions
        TotalCoins = PlayerPrefs.GetInt(TOTAL_COINS_KEY, 0);

        // 2) Start new run from 0
        CurrentRunCoins = 0;

        UpdateUI();
    }

    // called when picking up a coin
    public void AddCoins(int amount)
    {
        CurrentRunCoins += amount * coinMultiplier;
        UpdateUI();
    }

    public void SetMultiplier(int multiplier)
    {
        coinMultiplier = multiplier;
    }

    // called when the run ends (GameOver)
    public void SaveRunCoinsToTotal()
    {
        // Add this run's coins to total saved coins
        TotalCoins += CurrentRunCoins;

        // Save permanently
        PlayerPrefs.SetInt(TOTAL_COINS_KEY, TotalCoins);
        PlayerPrefs.Save();

        // Reset run coins for next run
        CurrentRunCoins = 0;

        UpdateUI();
    }
    public bool SpendCoins(int amount)
    {
        if (TotalCoins < amount)
        {
            return false;
        }

        TotalCoins -= amount;
        // Save permanently
        PlayerPrefs.SetInt(TOTAL_COINS_KEY, TotalCoins);
        PlayerPrefs.Save();
        UpdateUI();

        return true;
    }

    private void UpdateUI()
    {
        if (coinsText != null)
        {
            coinsText.text =
                "Run Coins: " + CurrentRunCoins +
                "\nTotal Coins: " + TotalCoins;
        }
    }
}
