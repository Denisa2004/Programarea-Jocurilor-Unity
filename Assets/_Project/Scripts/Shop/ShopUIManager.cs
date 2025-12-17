using UnityEngine;
using TMPro;

public class ShopUIManager : MonoBehaviour
{
    [Header("References")]
    public GameObject shopPanel;           // ShopPanel
    public TMP_Text shopCoinsText;         // ShopCoinsText
    public AvatarShopManager avatarShopManager;  //referinta spre managerul de avataruri

    private void Start()
    {
        // ascundem panelul la inceput
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void UpdateCoinsText()
    {
        if (shopCoinsText != null && CoinManager.Instance != null)
        {
            shopCoinsText.text = "Coins: " + CoinManager.Instance.TotalCoins;
        }
    }

    //apelata de butonul "Shop"
    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            UpdateCoinsText();

            if (avatarShopManager != null)
            {
                avatarShopManager.RefreshShop();
            }
        }
    }

    //apelata de butonul "Back"
    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
}
