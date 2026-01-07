using UnityEngine;
using TMPro;

/// <summary>
/// Script that automatically adds new shortcuts to Control Panel
/// Attach this script to the Control Panel GameObject
/// </summary>
public class ControlPanelShortcutsUpdater : MonoBehaviour
{
    [Header("Shortcut Texts")]
    [TextArea(3, 10)]
    public string additionalShortcuts = "M - Mute/Unmute\nR - Restart";

    [Header("Settings")]
    [Tooltip("Relative position to Control Panel (adjusted in Inspector)")]
    public Vector2 textPosition = new Vector2(0, -520); // Lower on screen
    public float fontSize = 36f;
    public Color textColor = Color.black;
    public FontStyles fontStyle = FontStyles.Bold;

    [Header("Optional: Custom Font")]
    public TMP_FontAsset customFont;

    private GameObject shortcutTextObj;

    private void Start()
    {
        AddShortcutsText();
    }

    private void AddShortcutsText()
    {
        // Check if it already exists
        if (shortcutTextObj != null)
        {
            Debug.Log("Shortcuts have already been added!");
            return;
        }

        // Find the parent Canvas
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("ControlPanelShortcutsUpdater: Control Panel is not a child of a Canvas!");
            return;
        }

        // Create a new GameObject for shortcuts
        shortcutTextObj = new GameObject("ShortcutsText_Dynamic");
        shortcutTextObj.transform.SetParent(transform, false);

        // Add TextMeshProUGUI component
        TextMeshProUGUI shortcutText = shortcutTextObj.AddComponent<TextMeshProUGUI>();

        // Set the font
        if (customFont != null)
        {
            shortcutText.font = customFont;
        }
        else
        {
            // Try to find a default TMP font
            TextMeshProUGUI referenceText = GetComponentInChildren<TextMeshProUGUI>();
            if (referenceText != null && referenceText.font != null)
            {
                shortcutText.font = referenceText.font;
            }
            else
            {
                // Use the default TMP font
                shortcutText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }
        }
        
        // Set text properties
        shortcutText.fontSize = fontSize;
        shortcutText.color = textColor;
        shortcutText.fontStyle = fontStyle;
        shortcutText.alignment = TextAlignmentOptions.Center;
        shortcutText.text = additionalShortcuts;

        // Configure RectTransform for correct positioning
        RectTransform rectTransform = shortcutTextObj.GetComponent<RectTransform>();
        
        // Anchor in center
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        // Set position and size
        rectTransform.anchoredPosition = textPosition;
        rectTransform.sizeDelta = new Vector2(600, 150);

        Debug.Log("Shortcuts added to Control Panel at position: " + textPosition);
    }

    // Utility to adjust position in real time from Inspector
    private void OnValidate()
    {
        if (Application.isPlaying && shortcutTextObj != null)
        {
            RectTransform rectTransform = shortcutTextObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = textPosition;
            }
            
            TextMeshProUGUI text = shortcutTextObj.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.fontSize = fontSize;
                text.color = textColor;
                text.fontStyle = fontStyle;
                text.text = additionalShortcuts;
            }
        }
    }

    // Clean up on destroy
    private void OnDestroy()
    {
        if (shortcutTextObj != null && Application.isPlaying)
        {
            Destroy(shortcutTextObj);
        }
    }
}
