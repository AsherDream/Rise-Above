using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalUILoader : MonoBehaviour
{
    public static GlobalUILoader Instance;
    private const string UISceneName = "UI_Global";

    // --- NEW: STRICT LOCK ---
    private static bool isGlobalUILoaded = false;

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

        LoadGlobalUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check every scene load just in case, but rely on the strict lock
        if (!isGlobalUILoaded)
        {
            LoadGlobalUI();
        }
    }

    private void LoadGlobalUI()
    {
        // 1. If it's already locked, ABORT immediately.
        if (isGlobalUILoaded) return;

        // 2. Lock it so it can never be called again
        isGlobalUILoaded = true;

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