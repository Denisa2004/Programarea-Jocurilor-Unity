using UnityEngine;

[System.Serializable]
public class AvatarData
{
    public string id;            // ex: "cat", "sheep"
    public string displayName;   // ex: "Cat", "Sheep"
    public Sprite icon;          // poza din shop UI
    public Sprite playerSprite;  // sprite-ul care se pune pe player
    public int price;
}
