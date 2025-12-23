using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AvatarPrefabData
{
    public string id;              // "cat", "sheep"
    public string displayName;     // "Cat", "Sheep"
    public Sprite icon;            // icon in shop
    public int price;              // coins
    public GameObject prefab;      // Cat Variant / Sheep Variant
}

public class AvatarApplier : MonoBehaviour
{
    [SerializeField] private Transform visualParent;
    [SerializeField] private List<AvatarPrefabData> avatars = new();

    private const string SELECTED_KEY = "shop_selected_avatar";

    void Start()
    {
        string selectedId = PlayerPrefs.GetString(SELECTED_KEY, "cat");

        // sterge ce e deja in Visual
        for (int i = visualParent.childCount - 1; i >= 0; i--)
            Destroy(visualParent.GetChild(i).gameObject);

        // pune avatarul selectat
        foreach (var a in avatars)
        {
            if (a.id == selectedId && a.prefab != null)
            {
                Instantiate(a.prefab, visualParent);
                return;
            }
        }
    }
}
