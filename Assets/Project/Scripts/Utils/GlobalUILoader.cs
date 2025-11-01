using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalUILoader : MonoBehaviour
{
    // ... (no changes to Awake, OnSceneLoaded) ...
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
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            LoadGlobalUI();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainMenu" && !SceneManager.GetSceneByName(UISceneName).isLoaded)
        {
            LoadGlobalUI();
        }

        if (scene.name == "MainMenu" && SceneManager.GetSceneByName(UISceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(UISceneName);
        }
    }


    private void LoadGlobalUI()
    {
        SceneManager.LoadSceneAsync(UISceneName, LoadSceneMode.Additive)
            .completed += (AsyncOperation op) =>
            {
                Scene uiScene = SceneManager.GetSceneByName(UISceneName);
                if (uiScene.isLoaded)
                {
                    foreach (GameObject obj in uiScene.GetRootGameObjects())
                    {
                        if (obj.CompareTag("UIRoot"))
                        {
                            Canvas canvas = obj.GetComponentInChildren<Canvas>(true);
                            GraphicRaycaster raycaster = obj.GetComponentInChildren<GraphicRaycaster>(true);
                            Camera uiCamera = obj.GetComponentInChildren<Camera>(true); // <-- ADD THIS

                            if (canvas != null) canvas.enabled = false;
                            if (raycaster != null) raycaster.enabled = false;
                            if (uiCamera != null) uiCamera.enabled = false; // <-- ADD THIS

                            Debug.Log("[GlobalUILoader] UI_Global hidden on load (Canvas + Camera method).");
                        }
                    }
                }
            };
    }
}

