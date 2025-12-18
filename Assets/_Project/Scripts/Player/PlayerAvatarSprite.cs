using System.Collections.Generic;
using UnityEngine;

public class PlayerAvatarSprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<AvatarData> avatars = new();

    private const string SELECTED_KEY = "shop_selected_avatar";

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        string selectedId = PlayerPrefs.GetString(SELECTED_KEY, "cat");

        foreach (var a in avatars)
        {
            if (a.id == selectedId && a.playerSprite != null)
            {
                spriteRenderer.sprite = a.playerSprite;
                break;
            }
        }
    }
}
