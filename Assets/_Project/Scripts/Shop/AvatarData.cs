using UnityEngine;

[System.Serializable]
public class AvatarData
{
    public string id;            // ex: "cat", "sheep"
    public string displayName;   // ex: "Cat", "Sheep"
    public Sprite icon;          // image from shop UI
    public Sprite playerSprite;  // the sprite that is put on player
    public int price;
}
