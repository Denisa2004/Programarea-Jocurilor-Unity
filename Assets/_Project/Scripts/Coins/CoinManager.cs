using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI coinsText;

    //number of coins collected during the current run
    public int CurrentRunCoins { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //reset coin count whenever the scene loads
        ResetRunCoins();
    }

    //coin amount to the current run's total
    public void AddCoins(int amount)
    {
        CurrentRunCoins += amount;
        UpdateUI();
    }

    //rsets the coin count at the start of a new run
    public void ResetRunCoins()
    {
        CurrentRunCoins = 0;
        UpdateUI(); //refresh the text on screen
    }

    //updates the User Interface text
    private void UpdateUI()
    {
        if (coinsText != null)
        {
            coinsText.text = "Coins: " + CurrentRunCoins;
        }
    }

}
