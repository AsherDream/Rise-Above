using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalUILoader : MonoBehaviour
{
    public static GlobalUILoader Instance;
    private const string UISceneName = "UI_Global";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Only load UI_Global if we are NOT in MainMenu
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            LoadGlobalUI();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Load UI_Global only if not MainMenu and it's not already loaded
        if (scene.name != "MainMenu" && !SceneManager.GetSceneByName(UISceneName).isLoaded)
        {
            LoadGlobalUI();
        }

        // Unload UI_Global when returning to Main Menu
        if (scene.name == "MainMenu" && SceneManager.GetSceneByName(UISceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(UISceneName);
        }
    }

    private void LoadGlobalUI()
    {
        SceneManager.LoadSceneAsync(UISceneName, LoadSceneMode.Additive);
    }
}
