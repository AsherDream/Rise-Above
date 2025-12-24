using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalUILoader : MonoBehaviour
{
    public static GlobalUILoader Instance;
    private const string UISceneName = "UI_Global"; // Make sure your scene is named exactly this!

    private void Awake()
    {
        // Singleton Pattern
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Load immediately on start
        LoadGlobalUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Whenever a scene loads, ensure UI_Global is there.
        // We removed the code that unloaded it for the Main Menu.
        if (!SceneManager.GetSceneByName(UISceneName).isLoaded)
        {
            LoadGlobalUI();
        }
    }

    private void LoadGlobalUI()
    {
        if (SceneManager.GetSceneByName(UISceneName).isLoaded) return;

        SceneManager.LoadSceneAsync(UISceneName, LoadSceneMode.Additive)
            .completed += (AsyncOperation op) =>
            {
                Scene uiScene = SceneManager.GetSceneByName(UISceneName);
                if (uiScene.isLoaded)
                {
                    foreach (GameObject obj in uiScene.GetRootGameObjects())
                    {
                        // Finds the root object (Make sure your Global UI root has the tag "UIRoot")
                        if (obj.CompareTag("UIRoot"))
                        {
                            // Force components off so the UI starts Invisible
                            Canvas canvas = obj.GetComponentInChildren<Canvas>(true);
                            GraphicRaycaster raycaster = obj.GetComponentInChildren<GraphicRaycaster>(true);
                            Camera uiCamera = obj.GetComponentInChildren<Camera>(true);

                            if (canvas != null) canvas.enabled = false;
                            if (raycaster != null) raycaster.enabled = false;
                            if (uiCamera != null) uiCamera.enabled = false;

                            Debug.Log("[GlobalUILoader] UI_Global loaded & hidden successfully.");
                        }
                    }
                }
            };
    }
}