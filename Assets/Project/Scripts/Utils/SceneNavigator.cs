using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        Debug.Log($"[SceneNavigator] Loading scene: {sceneName}");

        // Check if scene exists in build
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' is not in Build Settings!");
            return;
        }

        // Load new scene
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
