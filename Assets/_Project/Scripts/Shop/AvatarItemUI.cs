using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AvatarItemUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Button actionButton;
    public TMP_Text actionButtonText;

    private AvatarOption avatarData;
    private AvatarShopManager shopManager;

    public void Setup(AvatarOption data, AvatarShopManager manager, bool isOwned, bool isSelected)
    {
        avatarData = data;
        shopManager = manager;

        if (iconImage != null) iconImage.sprite = data.icon;
        if (nameText != null) nameText.text = data.displayName;
        if (priceText != null) priceText.text = data.price + " coins";

        UpdateState(isOwned, isSelected);

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnClick);
    }

    public void UpdateState(bool owned, bool selected)
    {
        if (selected)
        {
            actionButtonText.text = "Selected";
            actionButton.interactable = false;
        }
        else if (owned)
        {
            actionButtonText.text = "Select";
            actionButton.interactable = true;
        }
        else
        {
            actionButtonText.text = "Buy";
            actionButton.interactable = true;
        }
    }

    private void OnClick()
    {
        if (shopManager != null && avatarData != null)
        {
            shopManager.OnAvatarClicked(avatarData);
        }
    }
}
