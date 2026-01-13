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
    [SerializeField] private List<AvatarAudioSet> avatarAudioSets;
    private AvatarAudioRuntime audioRuntime;


    private const string SELECTED_KEY = "shop_selected_avatar";

    void Start()
    {
        string selectedId = PlayerPrefs.GetString(SELECTED_KEY, "cat");
        audioRuntime = GetComponent<AvatarAudioRuntime>();

        // sterge ce e deja in Visual
        for (int i = visualParent.childCount - 1; i >= 0; i--)
            Destroy(visualParent.GetChild(i).gameObject);

        // pune avatarul selectat
        for (int i = 0; i < avatars.Count; i++)
        {
            var a = avatars[i];

            if (a.id == selectedId && a.prefab != null)
            {
                Instantiate(a.prefab, visualParent);

                // seteaza audio set-ul corespunzator (daca exista)
                if (audioRuntime != null && avatarAudioSets != null && i < avatarAudioSets.Count)
                {
                    audioRuntime.Current = avatarAudioSets[i];
                    // Debug.Log("Audio set changed to: " + audioRuntime.Current.name);
                }

                return;
            }
        }
    }
}
