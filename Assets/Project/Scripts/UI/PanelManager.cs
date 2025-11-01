using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class PanelManager : MonoBehaviour
{
    [Title("UI Panels")]
    [Required] public GameObject pauseMenu;
    [Required] public GameObject settingsPanel;
    [Required] public GameObject saveLoadPanel;
    [Required] public GameObject exitConfirmPanel;

    [Title("Root Reference")]
    [ReadOnly] public GameObject uiRoot;

    // --- NEW REFERENCES ---
    private Canvas rootCanvas;
    private GraphicRaycaster rootRaycaster;
    private Camera rootCamera; // <-- ADD THIS

    [Title("State (Read-Only)")]
    [ReadOnly] public string currentPanel = "None";

    private void Awake()
    {
        uiRoot = GameObject.FindWithTag("UIRoot");
        if (uiRoot == null)
            Debug.LogError("[PanelManager] UIRoot not found in scene!");
        else
            Debug.Log("[PanelManager] UIRoot found successfully in Awake.");
    }

    private void Start()
    {
        if (uiRoot != null)
        {
            rootCanvas = uiRoot.GetComponentInChildren<Canvas>(true);
            rootRaycaster = uiRoot.GetComponentInChildren<GraphicRaycaster>(true);
            rootCamera = uiRoot.GetComponentInChildren<Camera>(true); // <-- ADD THIS

            if (rootCanvas == null) Debug.LogError("[PanelManager] No Canvas found on UIRoot!");
            if (rootRaycaster == null) Debug.LogError("[PanelManager] No GraphicRaycaster found!");
            if (rootCamera == null) Debug.LogError("[PanelManager] No Camera found on UIRoot!");

            // Force the canvas AND camera to be disabled on start.
            if (rootCanvas != null) rootCanvas.enabled = false;
            if (rootRaycaster != null) rootRaycaster.enabled = false;
            if (rootCamera != null) rootCamera.enabled = false; // <-- ADD THIS

            Debug.Log("[PanelManager] Canvas and Camera hidden on Start().");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBack();
        }
    }

    [Button("Back (Universal)")]
    public void HandleBack()
    {
        // Re-find references if they are null
        if (rootCanvas == null || rootRaycaster == null || rootCamera == null)
        {
            if (uiRoot == null) uiRoot = GameObject.FindWithTag("UIRoot");
            if (uiRoot != null)
            {
                rootCanvas = uiRoot.GetComponentInChildren<Canvas>(true);
                rootRaycaster = uiRoot.GetComponentInChildren<GraphicRaycaster>(true);
                rootCamera = uiRoot.GetComponentInChildren<Camera>(true); // <-- ADD THIS
            }
        }

        if (rootCanvas == null || rootRaycaster == null || rootCamera == null)
        {
            Debug.LogError("[PanelManager] Cannot handle back — components missing!");
            return;
        }

        // Check panel states first
        if (exitConfirmPanel.activeSelf)
        {
            Debug.Log("[PanelManager] Closing Exit Confirm -> Returning to Pause Menu.");
            ShowPanel(pauseMenu);
        }
        else if (settingsPanel.activeSelf)
        {
            Debug.Log("[PanelManager] Closing Settings -> Returning to Pause Menu.");
            ShowPanel(pauseMenu);
        }
        else if (saveLoadPanel.activeSelf)
        {
            Debug.Log("[PanelManager] Closing Save/Load -> Returning to Pause Menu.");
            ShowPanel(pauseMenu);
        }
        else if (pauseMenu.activeSelf)
        {
            // --- RESUME GAME ---
            Debug.Log("[PanelManager] Closing Pause Menu -> Resuming Game.");
            pauseMenu.SetActive(false);

            rootCanvas.enabled = false;
            rootRaycaster.enabled = false;
            rootCamera.enabled = false; // <-- ADD THIS

            Time.timeScale = 1f;
            currentPanel = "None";
        }
        else
        {
            // --- PAUSE GAME ---
            Debug.Log("[PanelManager] Opening Pause Menu.");

            rootCanvas.enabled = true;
            rootRaycaster.enabled = true;
            rootCamera.enabled = true; // <-- ADD THIS

            ShowPanel(pauseMenu);
            Time.timeScale = 0f;
        }
    }

    private void ShowPanel(GameObject panelToShow)
    {
        pauseMenu.SetActive(panelToShow == pauseMenu);
        settingsPanel.SetActive(panelToShow == settingsPanel);
        saveLoadPanel.SetActive(panelToShow == saveLoadPanel);
        exitConfirmPanel.SetActive(panelToShow == exitConfirmPanel);

        currentPanel = panelToShow.name;
        Debug.Log($"[PanelManager] Showing panel: {currentPanel}");
    }

    // --- BUTTON FUNCTIONS ---

    [BoxGroup("Pause Menu Buttons")]
    [Button]
    public void OnResume()
    {
        Debug.Log("Resume pressed");
        HandleBack();
    }

    [BoxGroup("Pause Menu Buttons")]
    [Button]
    public void OnSettings()
    {
        Debug.Log("Settings pressed");
        ShowPanel(settingsPanel);
    }

    // --- TYPO FIX HERE ---
    [BoxGroup("Pause Menu Buttons")]
    [Button]
    public void OnSaveAndLoad()
    {
        Debug.Log("Save/Load Pressed");
        ShowPanel(saveLoadPanel);
    }

    [BoxGroup("Pause Menu Buttons")]
    [Button]
    public void OnReturnToMainMenu()
    {
        Debug.Log("Return to Main Menu pressed");
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // --- TYPO FIX HERE ---
    [BoxGroup("Exit Panel Buttons")]
    [Button]
    public void OnExitPressed()
    {
        Debug.Log("Exit button pressed");
        ShowPanel(exitConfirmPanel);
    }

    [BoxGroup("Exit Panel Buttons")]
    [Button]
    public void OnExitConfirm()
    {
        Debug.Log("Exit confirmed.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    [BoxGroup("Exit Panel Buttons")]
    [Button]
    public void OnExitCancel()
    {
        Debug.Log("Exit cancelled");
        ShowPanel(pauseMenu); // Go back to the pause menu
    }
}

