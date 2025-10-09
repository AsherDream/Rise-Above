using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Title("Pause Menu Settings")]
    [SerializeField, Required] private GameObject pauseMenu;

    [Title("Key Bind")]
    [InfoBox("Press this key to toggle pause/unpause the game.")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Title("References")]
    [SerializeField, Required] private GameObject settingsPanel;
    [SerializeField] private GameObject saveLoadPanel;

    [Title("ExitPanel")]
    [SerializeField, Required] private GameObject exitConfirmPanel;


    private bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            Debug.Log("Pause key pressed - toggling pause.");
            TogglePause();
        }
    }

    [Button("Toggle Pause (Manual Test)")]
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(isPaused);
        }

        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log(isPaused ? "Game Paused" : "Game Resumed");
    }

    private void OnDisable()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
        }
    }

    //---------------- Button Functions -------------------

    public void OnResume()
    {
        Debug.Log("Resume pressed");
        TogglePause();
    }

    public void OnSettings()
    {
        Debug.Log("Settings pressed");
        OpenPanel(settingsPanel);
    }

    public void OnSaveAndLoad()
    {
        Debug.Log("Save/Load Pressed");
        OpenPanel(saveLoadPanel);
    }

    public void OnReturnToMainMenu()
    {
        Debug.Log("Return to Main Menu pressed");
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // Called by Exit button
    public void OnExitPressed()
    {
        if (exitConfirmPanel != null)
            exitConfirmPanel.SetActive(true); // Show confirmation panel
    }

    // Called by "Yes" button on confirmation panel
    public void OnExitConfirm()
    {
        Debug.Log("Exit confirmed.");
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Called by "No" button on confirmation panel
    public void OnExitCancel()
    {
        if (exitConfirmPanel != null)
            exitConfirmPanel.SetActive(false); // Hide confirmation panel
    }


    private void OpenPanel(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogWarning("Panel reference is missing!");
            return;
        }

        panel.SetActive(true);
    }
}
