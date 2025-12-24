using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class GlobalSettingsManager : MonoBehaviour
{
    public static GlobalSettingsManager Instance;

    [Title("Audio References")]
    [Tooltip("Assign your main AudioMixer here.")]
    [Required] public AudioMixer mainMixer;

    [Header("Volume Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider dialogueSlider; // New! For the Sister's voice

    [Header("Display & Accessibility")]
    public TMP_Dropdown displayModeDropdown;
    public Toggle photosensitivityToggle; // New! Disables lightning flashes

    // --- Static Access for Game Logic ---
    public static bool IsFlashingAllowed = true;

    // Internal Keys for PlayerPrefs
    private const string PREF_MASTER = "MasterVol";
    private const string PREF_MUSIC = "MusicVol";
    private const string PREF_SFX = "SFXVol";
    private const string PREF_VOICE = "VoiceVol";
    private const string PREF_DISPLAY = "DisplayMode";
    private const string PREF_FLASH = "NoFlash";

    private void Awake()
    {
        // Simple Singleton to allow other scripts to check settings easily
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        InitializeUI();
        LoadSavedSettings();
    }

    private void InitializeUI()
    {
        // Add Listeners so we don't have to drag events in Inspector manually
        if (masterSlider) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicSlider) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        if (dialogueSlider) dialogueSlider.onValueChanged.AddListener(SetVoiceVolume);

        if (displayModeDropdown) displayModeDropdown.onValueChanged.AddListener(SetDisplayMode);
        if (photosensitivityToggle) photosensitivityToggle.onValueChanged.AddListener(SetPhotosensitivity);
    }

    // --- LOADING LOGIC ---
    private void LoadSavedSettings()
    {
        // 1. Load Audio (Default to 0.75 if empty)
        float mVol = PlayerPrefs.GetFloat(PREF_MASTER, 0.75f);
        float muVol = PlayerPrefs.GetFloat(PREF_MUSIC, 0.75f);
        float sVol = PlayerPrefs.GetFloat(PREF_SFX, 0.75f);
        float vVol = PlayerPrefs.GetFloat(PREF_VOICE, 1.0f);

        if (masterSlider) masterSlider.value = mVol;
        if (musicSlider) musicSlider.value = muVol;
        if (sfxSlider) sfxSlider.value = sVol;
        if (dialogueSlider) dialogueSlider.value = vVol;

        // Apply immediately to mixer
        SetMasterVolume(mVol);
        SetMusicVolume(muVol);
        SetSFXVolume(sVol);
        SetVoiceVolume(vVol);

        // 2. Load Display
        int dispMode = PlayerPrefs.GetInt(PREF_DISPLAY, 0); // 0 = Fullscreen default
        if (displayModeDropdown) displayModeDropdown.value = dispMode;
        SetDisplayMode(dispMode);

        // 3. Load Accessibility
        bool disableFlash = PlayerPrefs.GetInt(PREF_FLASH, 0) == 1;
        if (photosensitivityToggle) photosensitivityToggle.isOn = disableFlash;
        SetPhotosensitivity(disableFlash);
    }

    // --- AUDIO LOGIC ---
    // Note: Volume sliders should be 0.0001 to 1.0 in Inspector
    public void SetMasterVolume(float val) { SetMixerVolume("MasterVolume", val); PlayerPrefs.SetFloat(PREF_MASTER, val); }
    public void SetMusicVolume(float val) { SetMixerVolume("MusicVolume", val); PlayerPrefs.SetFloat(PREF_MUSIC, val); }
    public void SetSFXVolume(float val) { SetMixerVolume("SFXVolume", val); PlayerPrefs.SetFloat(PREF_SFX, val); }
    public void SetVoiceVolume(float val) { SetMixerVolume("VoiceVolume", val); PlayerPrefs.SetFloat(PREF_VOICE, val); }

    private void SetMixerVolume(string paramName, float sliderValue)
    {
        if (!mainMixer) return;
        // Convert slider (0-1) to Decibels (-80 to 0)
        float db = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;
        mainMixer.SetFloat(paramName, db);
    }

    // --- DISPLAY LOGIC ---
    public void SetDisplayMode(int index)
    {
        switch (index)
        {
            case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: Screen.fullScreenMode = FullScreenMode.Windowed; break;
            case 2: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break; // Borderless
        }
        PlayerPrefs.SetInt(PREF_DISPLAY, index);
    }

    // --- ACCESSIBILITY LOGIC ---
    public void SetPhotosensitivity(bool disableFlash)
    {
        IsFlashingAllowed = !disableFlash;
        PlayerPrefs.SetInt(PREF_FLASH, disableFlash ? 1 : 0);
    }

    public void SaveAndClose()
    {
        PlayerPrefs.Save();
        // Uses the PanelManager to go back, keeping the flow clean
        if (PanelManager.Instance != null) PanelManager.Instance.HandleBack();
    }
}