using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    // This version doesn’t store a scene name — you pass it in OnClick()
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneNavigator: No scene name specified!");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            Debug.Log(sceneName + " has been loaded");
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' is not in Build Settings!");
        }

        SceneManager.UnloadSceneAsync("UI_Global");

    }
}
