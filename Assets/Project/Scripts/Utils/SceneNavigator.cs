using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class SceneNavigator : MonoBehaviour
{
    [Title("Scene Management")]
    public string gameSceneName = "SuperMarket_Scene"; // Make sure this matches your scene name!

    [Title("Optional: Main Menu Buttons")]
    [Tooltip("Drag the parent object of your Main Menu buttons here if you want to hide them when Options are open.")]
    public GameObject mainMenuButtonsContainer;

    // --- 1. SPECIFIC FUNCTION (Linked to Start Button) ---
    public void StartGame()
    {
        LoadScene(gameSceneName);
    }

    // --- 2. GENERIC FUNCTION (The Bridge) ---
    public void LoadScene(string sceneName)
    {
        Debug.Log($"[SceneNavigator] Attempting to load: {sceneName}");

        // 1. Reset Global UI so menus don't get stuck
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.HideAll();
        }

        // 2. FORCE TIME TO RESUME
        // Critical: If time is 0, the fade animation might not play!
        Time.timeScale = 1f;

        // 3. Check if scene is valid
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            // --- THE CHANGE IS HERE ---
            // Try to use the smooth transition first
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(sceneName);
            }
            else
            {
                // Fallback: If no transition manager exists, load instantly
                Debug.LogWarning("SceneTransitionManager not found. Loading instantly.");
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
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
    }

    public void OpenGlobalExit()
    {
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.OpenExitConfirm();
        }
    }
}