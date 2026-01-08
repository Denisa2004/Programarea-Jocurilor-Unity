using UnityEngine;
using UnityEngine.Audio; // for sound management
using UnityEngine.SceneManagement; // for changing scenes
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("Referinte UI")]
    public GameObject SettingsPanel,Fullscreen,ExitFullscreen;
    public Slider musicSlider;
    public TMP_Dropdown resolutionDropdown;

    [Header("Audio")]
    public AudioMixer Volume;
    private const string MIXER_PARAM = "MasterVolume";
    private const string SAVE_KEY_VOL = "MusicVolume";

    [Header("Shop")]
    public GameObject ShopPanel;
    [Header("Control Panel")]
    public GameObject controlPanel;
    public GameObject controlPanelButton;
    

    Resolution[] resolutions;

    [System.Serializable] // This makes the list appear in Inspector
    public class RezolutiePersonalizata
    {
        public string numeAfisat; 
        public int width;         
        public int height;       
    }

    public List<RezolutiePersonalizata> listaRezolutii;
    
    void Start()
    {
        // Create or find AudioManager and set the AudioMixer
        if (AudioManager.Instance == null)
        {
            GameObject audioMgrObj = new GameObject("AudioManager");
            audioMgrObj.AddComponent<AudioManager>();
        }
        
        // Set the AudioMixer for AudioManager
        if (AudioManager.Instance != null && Volume != null)
        {
            AudioManager.Instance.SetAudioMixer(Volume);
        }

        InitResolutions();
        // Load the saved volume
        float savedVolume = PlayerPrefs.GetFloat(SAVE_KEY_VOL, 1f);
        // Update the slider position
        musicSlider.value = savedVolume;
        // Apply the saved volume in AudioMixer
        SetMusicVolume(savedVolume);
    }

    public void SetMusicVolume(float volume)
    {
        // Use AudioManager if it exists
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(volume);
        }
        else
        {
            // Fallback to direct mixer control
            float volumeInDecibels = (volume > 0.0001f) ? Mathf.Log10(volume) * 20 : -80f;
            Volume.SetFloat(MIXER_PARAM, volumeInDecibels);
            PlayerPrefs.SetFloat(SAVE_KEY_VOL, volume);
            PlayerPrefs.Save();
        }
    }

    void InitResolutions()
    {
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        // Go through the list and check if current resolution matches one from the list
        for (int i = 0; i < listaRezolutii.Count; i++)
        {
            options.Add(listaRezolutii[i].numeAfisat);
            if (listaRezolutii[i].width == Screen.width &&
                listaRezolutii[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("mainscene");
    }

    public void SetResolution(int resolutionIndex)
    {
        // Get the resolution from the list
        RezolutiePersonalizata rezolutieAleasa = listaRezolutii[resolutionIndex];

        Screen.SetResolution(rezolutieAleasa.width, rezolutieAleasa.height, Screen.fullScreen);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        UpdateFullscreenButtons(isFullscreen);
    }
    private void UpdateFullscreenButtons(bool isFullscreen)
    {
        if (Fullscreen != null && ExitFullscreen != null)
        {
            Fullscreen.SetActive(!isFullscreen);
            ExitFullscreen.SetActive(isFullscreen);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Jocul a fost inchis");
    }
    // activates SettingsPanel which is initially hidden in editor
    public void OpenSettings()
    {
        SettingsPanel.SetActive(true);
        SettingsPanel.transform.SetAsLastSibling(); // ensures settings panel renders above other UI elements
    }
    // deactivates SettingsPanel
    public void CloseSettings() { SettingsPanel.SetActive(false); }

    //activates shopPanel which is hidden initially in the editor
    public void OpenShop()
    {
        if (ShopPanel != null) ShopPanel.SetActive(true);
        if (controlPanelButton != null) controlPanelButton.SetActive(false);
    }

    //deactivates shopPanel
    public void CloseShop()
    {
        if (ShopPanel != null) ShopPanel.SetActive(false);
        if (controlPanelButton != null) controlPanelButton.SetActive(true);
    }

    public void ShowControlPanel()
    {
        controlPanel.SetActive(true);
    }

    public void HideControlPanel()
    {
        controlPanel.SetActive(false);
    }

}
