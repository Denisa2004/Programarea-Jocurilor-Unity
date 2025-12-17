using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class AvatarShopManager : MonoBehaviour
{
    [Header("UI")]
    public Transform itemsContainer;
    public GameObject avatarItemPrefab;
    public TMP_Text coinsText;

    [Header("Avatar List")]
    public List<AvatarOption> avatars = new List<AvatarOption>();

    private HashSet<string> owned = new HashSet<string>();
    private string selectedAvatar = "";

    private const string OwnedKey = "OwnedAvatars";
    private const string SelectedKey = "SelectedAvatar";

    private Dictionary<string, AvatarItemUI> uiItems = new Dictionary<string, AvatarItemUI>();

    //private void OnEnable()
    //{
    //    Load();
    //    UpdateUI();
    //    GenerateItems();
    //}
    public void RefreshShop()
    {
        Debug.Log("RefreshShop() called");

        Load();
        GenerateItems();
        UpdateUI();
    }
    public void OnAvatarClicked(AvatarOption avatar)
    {
        if (!owned.Contains(avatar.id))
        {
            if (CoinManager.Instance.SpendCoins(avatar.price))
            {
                owned.Add(avatar.id);
                Save();
            }
            else
            {
                Debug.Log("Not enough coins!");
                return;
            }
        }

        selectedAvatar = avatar.id;
        Save();
        UpdateUI();
    }

    private void GenerateItems()
    {
        foreach (Transform child in itemsContainer)
            Destroy(child.gameObject);

        uiItems.Clear();

        foreach (var a in avatars)
        {
            var go = Instantiate(avatarItemPrefab, itemsContainer);
            var ui = go.GetComponent<AvatarItemUI>();

            bool isOwned = owned.Contains(a.id);
            bool isSelected = (a.id == selectedAvatar);

            ui.Setup(a, this, isOwned, isSelected);
            uiItems[a.id] = ui;
        }
    }

    private void UpdateUI()
    {
        if (coinsText != null && CoinManager.Instance != null)
        {
            coinsText.text = "Coins: " + CoinManager.Instance.TotalCoins;
        }


        foreach (var a in avatars)
        {
            if (uiItems.ContainsKey(a.id))
            {
                uiItems[a.id].UpdateState(
                    owned.Contains(a.id),
                    selectedAvatar == a.id
                );
            }
        }
    }

    private void Load()
    {
        owned.Clear();
        string data = PlayerPrefs.GetString(OwnedKey, "");
        foreach (var id in data.Split('|'))
            if (id != "") owned.Add(id);

        selectedAvatar = PlayerPrefs.GetString(SelectedKey, "");

        if (selectedAvatar == "" && avatars.Count > 0)
            selectedAvatar = avatars[0].id;
    }

    private void Save()
    {
        PlayerPrefs.SetString(OwnedKey, string.Join("|", owned));
        PlayerPrefs.SetString(SelectedKey, selectedAvatar);
        PlayerPrefs.Save();
    }
}
