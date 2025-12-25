using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopManagerSprites : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private Transform content;
    [SerializeField] private AvatarItemUI itemPrefab;

    [Header("Avatars (Sprites)")]
    [SerializeField] private List<AvatarData> avatars = new();

    private const string OWNED_PREFIX = "shop_owned_";
    private const string SELECTED_KEY = "shop_selected_avatar";

    private void OnEnable()
    {
        RefreshCoins();
        Build();
    }

    private void RefreshCoins()
    {
        coinsText.text = "Coins: " + CoinManager.Instance.TotalCoins;
    }

    private void Build()
    {
        foreach (Transform child in content) Destroy(child.gameObject);

        string selectedId = PlayerPrefs.GetString(SELECTED_KEY, "cat");

        foreach (var a in avatars)
        {
            bool owned = PlayerPrefs.GetInt(OWNED_PREFIX + a.id, a.id == "cat" ? 1 : 0) == 1;
            bool selected = owned && a.id == selectedId;

            var ui = Instantiate(itemPrefab, content);

            if (selected)
                ui.SetupSprite(a, "SELECTED", false, null);
            else if (owned)
                ui.SetupSprite(a, "SELECT", true, () => Select(a.id));
            else
                ui.SetupSprite(a, "BUY", true, () => Buy(a));
        }
    }

    private void Buy(AvatarData a)
    {
        if (!CoinManager.Instance.SpendCoins(a.price)) return;

        PlayerPrefs.SetInt(OWNED_PREFIX + a.id, 1);
        PlayerPrefs.Save();

        RefreshCoins();
        Build();
    }

    private void Select(string id)
    {
        PlayerPrefs.SetString(SELECTED_KEY, id);
        PlayerPrefs.Save();

        Build();
    }
}
