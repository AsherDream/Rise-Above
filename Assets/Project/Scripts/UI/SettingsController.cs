using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;  // Add this at the top


public class SettingsController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer; // assign your main AudioMixer in Inspector
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Display Settings")]
    [SerializeField] private TMP_Dropdown displayModeDropdown;

    

    private void Start()
    {
        // Load saved settings if exist
        LoadSettings();
        UpdateDisplayValues();


        // Dropdown setup
        if (displayModeDropdown != null)
        {
            displayModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);
        }

        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void UpdateDisplayValues()
    {
        float masterVol, musicVol, sfxVol;

        if (audioMixer.GetFloat("MasterVolume", out masterVol))
            masterSlider.value = Mathf.Pow(10, masterVol / 20f);  // convert back from dB
        if (audioMixer.GetFloat("MusicVolume", out musicVol))
            musicSlider.value = Mathf.Pow(10, musicVol / 20f);
        if (audioMixer.GetFloat("SFXVolume", out sfxVol))
            sfxSlider.value = Mathf.Pow(10, sfxVol / 20f);
    }


    #region Audio
    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
    }
    #endregion

    #region Display
    public void OnDisplayModeChanged(int index)
    {
        switch (index)
        {
            case 0: // Fullscreen
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1: // Windowed
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 2: // Borderless Window
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
        }
    }
    #endregion

    #region UI Buttons
    public void OnSaveButton()
    {
        if (audioMixer == null)
        {
            Debug.LogError("AudioMixer is not assigned!");
            return;
        }
        PlayerPrefs.SetFloat("MasterVolume", masterSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.SetInt("DisplayMode", displayModeDropdown.value);
        PlayerPrefs.Save();

        Debug.Log("Settings Saved!");
    }

   

    #endregion

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            masterSlider.value = PlayerPrefs.GetFloat("MasterVolume");
            SetMasterVolume(masterSlider.value);
        }
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
            SetMusicVolume(musicSlider.value);
        }
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
            SetSFXVolume(sfxSlider.value);
        }
        if (PlayerPrefs.HasKey("DisplayMode"))
        {
            int mode = PlayerPrefs.GetInt("DisplayMode");
            displayModeDropdown.value = mode;
            OnDisplayModeChanged(mode);
        }
    }
}
