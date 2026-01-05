using UnityEngine;
using UnityEngine.UI;

public class ShopScrollReset : MonoBehaviour
{
    public ScrollRect scrollRect;

    void OnEnable()
    {
        //remake the layout
        Canvas.ForceUpdateCanvases();

        //scroll to the left
        scrollRect.horizontalNormalizedPosition = 0f;

    }
}
