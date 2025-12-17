using UnityEngine;

[System.Serializable]
public class AvatarOption
{
    public string id;               // ex: "default", "ninja", "robot"
    public string displayName;      // numele 
    public int price;               // costul avatarului
    public Sprite icon;             // imaginea din shop
    public GameObject avatarPrefab; // prefab-ul care va fi folosit in joc
}
