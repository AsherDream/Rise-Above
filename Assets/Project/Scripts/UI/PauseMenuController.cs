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


    private bool isPaused = false;

    private void Update()
    {
        // use GetKeyDown inside unscaled delta time to avoid time freeze issues
        if (Input.GetKeyDown(pauseKey))
        {
            Debug.Log("Pause key pressed – toggling pause.");
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

        // Make sure timeScale resumes correctly
        Time.timeScale = isPaused ? 0f : 1f;

        Debug.Log(isPaused ? "Game Paused" : "Game Resumed");
    }

    private void OnDisable()
    {
        // Ensure time scale resets if object is disabled
        if (isPaused)
        {
            Time.timeScale = 1f;
        }
    }

    //---------------- Button Functions-------------------

    public void OnResume()
    {
        Debug.Log("Resume pressed");
        TogglePause():
    }

    public void OnSettings()
    {
        Debug.Log("Settings pressed");
        openPanel(settingsPanel);
    }

    public void OnSaveAndLoad()
    {
        Debug.Log("Save/Load Pressed");
        OpenPanel(saveloadPanel)
    }

    public void OnReturnToMainMenu()
    {
        Debug.log("Return to Main Menu prassed");
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnExitGame()
    {
        Debug.Log("Exit Game pressed");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#else
        Application.Quit();
#endif
    }
}

   
