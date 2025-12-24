using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    [Title("UI Panels")]
    [Required] public GameObject pauseMenu;
    [Required] public GameObject settingsPanel;
    [Required] public GameObject saveLoadPanel;
    [Required] public GameObject exitConfirmPanel;
    [Required] public GameObject creditsPanel; // --- NEW: Added Credits Panel Reference ---

    private Canvas rootCanvas;
    private GraphicRaycaster rootRaycaster;
    private Camera rootCamera;

    [Title("Effects")]
    public Volume pauseVolume;

    [Title("State (Read-Only)")]
    [ReadOnly] public string currentPanel = "None";

    private void Awake()
    {
        // --- SINGLETON PATTERN ---
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        RefreshReferences();
        HideAll();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Safety checks
            if (CleanupManager.IsMessActive || ItemInspector.IsInspecting) return;

            // Don't open Pause Menu if we are sitting in the Main Menu and nothing is open
            if (IsMainMenu() && currentPanel == "None")
            {
                return;
            }

            HandleBack();
        }
    }

    // --- REFERENCE FINDING ---
    private void RefreshReferences()
    {
        if (rootCanvas == null) rootCanvas = GetComponentInChildren<Canvas>(true);
        if (rootRaycaster == null) rootRaycaster = GetComponentInChildren<GraphicRaycaster>(true);
        if (rootCamera == null) rootCamera = GetComponentInChildren<Camera>(true);

        if (rootCanvas == null) Debug.LogError("[PanelManager] Could not find Canvas! Ensure PanelManager is on the UI Root.");
    }

    public void HideAll()
    {
        if (rootCanvas != null) rootCanvas.enabled = false;
        if (rootRaycaster != null) rootRaycaster.enabled = false;
        if (rootCamera != null) rootCamera.enabled = false;
        if (pauseVolume != null) pauseVolume.weight = 0;

        if (pauseMenu) pauseMenu.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (saveLoadPanel) saveLoadPanel.SetActive(false);
        if (exitConfirmPanel) exitConfirmPanel.SetActive(false);
        if (creditsPanel) creditsPanel.SetActive(false); // --- NEW: Ensure Credits close too ---

        currentPanel = "None";
    }

    // --- OPEN PANELS ---

    // --- NEW: Function to open credits ---
    public void OpenCredits()
    {
        RefreshReferences();
        EnableCanvas();
        ShowPanel(creditsPanel);
    }

    public void OpenSettings()
    {
        RefreshReferences();
        EnableCanvas();
        ShowPanel(settingsPanel);
    }

    public void OpenExitConfirm()
    {
        RefreshReferences();
        EnableCanvas();
        ShowPanel(exitConfirmPanel);
    }

    private void EnableCanvas()
    {
        if (rootCanvas != null) rootCanvas.enabled = true;
        if (rootRaycaster != null) rootRaycaster.enabled = true;
        if (rootCamera != null) rootCamera.enabled = true;
    }

    // --- NAVIGATION LOGIC ---

    [Button("Back (Universal)")]
    public void HandleBack()
    {
        RefreshReferences();

        // 1. If in Main Menu, Back/Escape just hides whatever panel is open
        if (IsMainMenu())
        {
            HideAll();
            return;
        }

        // 2. In-Game Logic
        // Check if ANY sub-panel is open (Settings, Save, Exit, OR CREDITS)
        if (exitConfirmPanel.activeSelf || settingsPanel.activeSelf || saveLoadPanel.activeSelf || (creditsPanel && creditsPanel.activeSelf))
        {
            ShowPanel(pauseMenu); // Go back to Pause Menu
        }
        else if (pauseMenu.activeSelf)
        {
            HideAll(); // Resume Game
            Time.timeScale = 1f;
        }
        else
        {
            EnableCanvas(); // Force the canvas to appear
            ShowPanel(pauseMenu);
            if (pauseVolume != null) pauseVolume.weight = 1;
            Time.timeScale = 0f; // Stop time
        }
    }

    private void ShowPanel(GameObject panelToShow)
    {
        if (pauseMenu) pauseMenu.SetActive(panelToShow == pauseMenu);
        if (settingsPanel) settingsPanel.SetActive(panelToShow == settingsPanel);
        if (saveLoadPanel) saveLoadPanel.SetActive(panelToShow == saveLoadPanel);
        if (exitConfirmPanel) exitConfirmPanel.SetActive(panelToShow == exitConfirmPanel);
        if (creditsPanel) creditsPanel.SetActive(panelToShow == creditsPanel); // --- NEW: Manage Credits ---

        currentPanel = panelToShow != null ? panelToShow.name : "None";
    }

    // --- BUTTON FUNCTIONS ---

    public void OnResume() { HandleBack(); }
    public void OnSettings() { ShowPanel(settingsPanel); }
    public void OnSaveAndLoad() { ShowPanel(saveLoadPanel); }

    public void OnReturnToMainMenu()
    {
        Time.timeScale = 1f;
        HideAll();
        SceneManager.LoadScene("MainMenu");
    }

    public void OnExitPressed() { ShowPanel(exitConfirmPanel); }
    public void OnExitCancel() { HandleBack(); }

    public void OnExitConfirm()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private bool IsMainMenu()
    {
        return SceneManager.GetActiveScene().name == "MainMenu";
    }
}