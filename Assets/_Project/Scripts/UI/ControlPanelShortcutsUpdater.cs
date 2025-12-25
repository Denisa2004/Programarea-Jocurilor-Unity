using UnityEngine;
using TMPro;

/// <summary>
/// Script care adauga automat shortcut-urile noi in Control Panel
/// Ataseaza acest script la GameObject-ul Control Panel
/// </summary>
public class ControlPanelShortcutsUpdater : MonoBehaviour
{
    [Header("Shortcut Texts")]
    [TextArea(3, 10)]
    public string additionalShortcuts = "M - Mute/Unmute\nR - Restart";

    [Header("Settings")]
    [Tooltip("Pozitia relativa fata de Control Panel (ajusteaza in Inspector!)")]
    public Vector2 textPosition = new Vector2(0, -520); // Mai jos pe ecran
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
        // Verifica daca exista deja
        if (shortcutTextObj != null)
        {
            Debug.Log("Shortcut-urile au fost deja adaugate!");
            return;
        }

        // Cauta Canvas-ul parinte
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("ControlPanelShortcutsUpdater: Control Panel nu este copil al unui Canvas!");
            return;
        }

        // Creaza un nou GameObject pentru shortcut-uri
        shortcutTextObj = new GameObject("ShortcutsText_Dynamic");
        shortcutTextObj.transform.SetParent(transform, false);

        // Adauga componenta TextMeshProUGUI
        TextMeshProUGUI shortcutText = shortcutTextObj.AddComponent<TextMeshProUGUI>();
        
        // Seteaza fontul
        if (customFont != null)
        {
            shortcutText.font = customFont;
        }
        else
        {
            // Incearca sa gaseasca un font TMP implicit
            TextMeshProUGUI referenceText = GetComponentInChildren<TextMeshProUGUI>();
            if (referenceText != null && referenceText.font != null)
            {
                shortcutText.font = referenceText.font;
            }
            else
            {
                // Foloseste fontul implicit TMP
                shortcutText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }
        }
        
        // Seteaza proprietatile textului
        shortcutText.fontSize = fontSize;
        shortcutText.color = textColor;
        shortcutText.fontStyle = fontStyle;
        shortcutText.alignment = TextAlignmentOptions.Center;
        shortcutText.text = additionalShortcuts;

        // Configurare RectTransform pentru pozitionare corecta
        RectTransform rectTransform = shortcutTextObj.GetComponent<RectTransform>();
        
        // Ancoreaza in centru
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        // Seteaza pozitia si dimensiunea
        rectTransform.anchoredPosition = textPosition;
        rectTransform.sizeDelta = new Vector2(600, 150);

        Debug.Log("? Shortcut-uri adaugate in Control Panel la pozitia: " + textPosition);
    }

    // Utility pentru a ajusta pozitia in timp real din Inspector
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

    // Curata la destroy
    private void OnDestroy()
    {
        if (shortcutTextObj != null && Application.isPlaying)
        {
            Destroy(shortcutTextObj);
        }
    }
}
