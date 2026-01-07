using UnityEngine;
using UnityEngine.Audio;

/// Global audio manager for mute/unmute functionality during gameplay
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio - Set automatically from MenuManager")]
    [Tooltip("Will be set automatically when MenuManager exists in the scene")]
    public AudioMixer audioMixer;

    private const string MIXER_PARAM = "MasterVolume";
    private const string SAVE_KEY_VOL = "MusicVolume";
    private const string SAVE_KEY_MUTE = "IsMuted";

    private bool isMuted = false;
    private float volumeBeforeMute = 1f;

    private void Awake()
    {
        // Singleton with DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load the saved volume and mute state
        volumeBeforeMute = PlayerPrefs.GetFloat(SAVE_KEY_VOL, 1f);
        isMuted = PlayerPrefs.GetInt(SAVE_KEY_MUTE, 0) == 1;

        // Apply initial state
        ApplyMuteState();
    }

    private void Update()
    {
        // Listen for M key to toggle mute/unmute
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
            // Save current volume before mute
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

        // Save mute state
        PlayerPrefs.SetInt(SAVE_KEY_MUTE, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetVolume(float volume)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("AudioManager: AudioMixer is not set! Set it manually or let MenuManager set it.");
            return;
        }

        // Convert linear volume to decibels
        float volumeInDecibels = (volume > 0.0001f) ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat(MIXER_PARAM, volumeInDecibels);

        // Save volume only if we're not muted
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

    // Set the AudioMixer (called by MenuManager)
    public void SetAudioMixer(AudioMixer mixer)
    {
        audioMixer = mixer;
        ApplyMuteState();
    }
}