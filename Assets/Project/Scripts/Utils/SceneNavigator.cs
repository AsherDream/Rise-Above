using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class SceneNavigator : MonoBehaviour
{
    [Title("Scene Management")]
    public string gameSceneName = "Scene1";

    [Title("Optional: Main Menu Buttons")]
    [Tooltip("Drag the parent object of your Main Menu buttons here if you want to hide them when Options are open.")]
    public GameObject mainMenuButtonsContainer;

    // --- 1. SPECIFIC FUNCTION (For Main Menu "New Game") ---
    public void StartGame()
    {
        // Wrapper that just calls the generic one with the default name
        LoadScene(gameSceneName);
    }

    // --- 2. GENERIC FUNCTION ---
    public void LoadScene(string sceneName)
    {
        Debug.Log($"[SceneNavigator] Attempting to load: {sceneName}");

        // 1. Reset Global UI if it exists (so menus don't get stuck open)
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.HideAll();
        }

        // 2. FORCE TIME TO RESUME (The Fix!)
        // Unity remembers TimeScale between scenes. If you were paused, 
        // the next scene would be frozen without this line.
        Time.timeScale = 1f;

        // 3. Check if scene is valid before loading
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' could not be found! Check Build Settings.");
        }
    }

    // --- 3. MENU FUNCTIONS ---
    public void OpenGlobalOptions()
    {
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.OpenSettings();
        }
        else
        {
            Debug.LogError("PanelManager not found! Is Global UI loaded?");
        }
    }

    public void OpenGlobalExit()
    {
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.OpenExitConfirm();
        }
        else
        {
            Debug.LogError("PanelManager not found! Is Global UI loaded?");
        }
    }
}