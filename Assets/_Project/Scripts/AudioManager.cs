using UnityEngine;
using UnityEngine.Audio;

/// Global audio manager for mute/unmute functionality during gameplay
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio - Setat automat din MenuManager")]
    [Tooltip("Se va seta automat cand MenuManager exista in scena")]
    public AudioMixer audioMixer;

    private const string MIXER_PARAM = "MasterVolume";
    private const string SAVE_KEY_VOL = "MusicVolume";
    private const string SAVE_KEY_MUTE = "IsMuted";

    private bool isMuted = false;
    private float volumeBeforeMute = 1f;

    private void Awake()
    {
        // Singleton cu DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Incarca volumul si starea de mute salvate
        volumeBeforeMute = PlayerPrefs.GetFloat(SAVE_KEY_VOL, 1f);
        isMuted = PlayerPrefs.GetInt(SAVE_KEY_MUTE, 0) == 1;

        // Aplica starea initiala
        ApplyMuteState();
    }

    private void Update()
    {
        // Asculta tasta M pentru toggle mute/unmute
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMute();
        }
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            // Salveaza volumul curent inainte de mute
            float currentVol = PlayerPrefs.GetFloat(SAVE_KEY_VOL, 1f);
            if (currentVol > 0.0001f)
                volumeBeforeMute = currentVol;

            SetVolume(0f);
            Debug.Log("🔇 Muzica MUTED (apasa M din nou pentru unmute)");
        }
        else
        {
            // Restaureaza volumul anterior
            SetVolume(volumeBeforeMute);
            Debug.Log("🔊 Muzica UNMUTED");
        }

        // Salveaza starea de mute
        PlayerPrefs.SetInt(SAVE_KEY_MUTE, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetVolume(float volume)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("AudioManager: AudioMixer nu este setat! Seteaza-l manual sau lasa MenuManager sa il seteze.");
            return;
        }

        // Converteste volumul linear in decibeli
        float volumeInDecibels = (volume > 0.0001f) ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat(MIXER_PARAM, volumeInDecibels);

        // Salveaza volumul doar daca nu suntem in mute
        if (!isMuted && volume > 0.0001f)
        {
            PlayerPrefs.SetFloat(SAVE_KEY_VOL, volume);
            PlayerPrefs.Save();
        }
    }

    private void ApplyMuteState()
    {
        if (isMuted)
        {
            SetVolume(0f);
        }
        else
        {
            SetVolume(volumeBeforeMute);
        }
    }

    public bool IsMuted()
    {
        return isMuted;
    }

    public float GetVolume()
    {
        return isMuted ? 0f : volumeBeforeMute;
    }

    // Seteaza AudioMixer-ul (apelat de MenuManager)
    public void SetAudioMixer(AudioMixer mixer)
    {
        audioMixer = mixer;
        ApplyMuteState();
    }
}