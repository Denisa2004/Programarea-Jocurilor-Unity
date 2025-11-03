using UnityEngine;
using UnityEngine.SceneManagement; // pentru schimbatul scenelor
using UnityEngine.Audio; // pentru gestionarea sunetului

public class MenuManager : MonoBehaviour
{
    public GameObject SettingsPanel,Fullscreen,ExitFullscreen;
    public AudioMixer Volume;

    public void StartGame()
    {
        SceneManager.LoadScene("testscene");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Jocul a fost inchis");
    }

    public void OpenSettings()
    {
        // activeaza SettingsPanel care e ascuns initial in editor
        SettingsPanel.SetActive(true);
    }
    public void SetMusicVolume(float volume)
    {
        // valoarea Slider-ului trebuie convertita logaritmic
        Volume.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        if (isFullscreen)
        {
            Fullscreen.SetActive(false);
            ExitFullscreen.SetActive(true);
        }
        else
        {
            Fullscreen.SetActive(true);
            ExitFullscreen.SetActive(false);
        }
    }

    public void CloseSettings()
    {
        // dezactiveaza SettingsPanel
        SettingsPanel.SetActive(false);
    }
}