using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvatarItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;

    public void Setup(AvatarData data, string label, bool interactable, System.Action onClick)
    {
        titleText.text = data.displayName;
        iconImage.sprite = data.icon;
        priceText.text = data.price.ToString();

        actionButtonText.text = label;
        actionButton.interactable = interactable;

        actionButton.onClick.RemoveAllListeners();
        if (onClick != null)
            actionButton.onClick.AddListener(() => onClick());
    }
    public void SetupSprite(AvatarData data, string label, bool interactable, System.Action onClick)
    {
        titleText.text = data.displayName;
        iconImage.sprite = data.icon;
        priceText.text = data.price.ToString();

        actionButtonText.text = label;
        actionButton.interactable = interactable;

        actionButton.onClick.RemoveAllListeners();
        if (onClick != null)
            actionButton.onClick.AddListener(() => onClick());
    }

}
