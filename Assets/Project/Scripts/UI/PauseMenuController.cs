using UnityEngine;
using Sirenix.OdinInspector;

public class PauseController : MonoBehaviour
{
    [Title("Pause Menu Settings")]
    [SerializeField, Required] private GameObject pauseMenu;

    [Title("Key Bind")]
    [InfoBox("Press this key to toggle pause/unpause the game.")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

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
}
