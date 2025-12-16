using UnityEngine;
using UnityEngine.Audio; // pentru gestionarea sunetului
using UnityEngine.SceneManagement;// pentru schimbatul scenelor
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

    Resolution[] resolutions;

    [System.Serializable] // Asta face ca lista sa apara in Inspector
    public class RezolutiePersonalizata
    {
        public string numeAfisat; 
        public int width;         
        public int height;       
    }

    public List<RezolutiePersonalizata> listaRezolutii;
    void Start()
    {

        InitResolutions();
        // Incarcam volumul salvat. 
        float savedVolume = PlayerPrefs.GetFloat(SAVE_KEY_VOL, 1f);
        // Actualizam pozitia slider-ului
        musicSlider.value = savedVolume;
        // Aplicam volumul salvat in AudioMixer
        SetMusicVolume(savedVolume);
    }

    public void SetMusicVolume(float volume)
    {
        // valoarea Slider-ului trebuie convertita logaritmic
        float volumeInDecibels = (volume > 0.0001f) ? Mathf.Log10(volume) * 20 : -80f;
        // Setam volumul in Mixer
        Volume.SetFloat(MIXER_PARAM, volumeInDecibels);
        // Salvam volumul in PlayerPrefs
        PlayerPrefs.SetFloat(SAVE_KEY_VOL, volume);
        PlayerPrefs.Save();
    }

    void InitResolutions()
    {
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        // Trecem prin lista si vedem daca rezolutia curenta se potriveste cu una din lista
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
        SceneManager.LoadScene("testscene");
    }

    public void SetResolution(int resolutionIndex)
    {
        // Luam rezolutia din lista
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
    // activeaza SettingsPanel care e ascuns initial in editor
    public void OpenSettings() { SettingsPanel.SetActive(true); }
    // dezactiveaza SettingsPanel
    public void CloseSettings() { SettingsPanel.SetActive(false); }
}