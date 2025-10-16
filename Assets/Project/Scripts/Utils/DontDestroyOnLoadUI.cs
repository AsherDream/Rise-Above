using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroyOnLoadUI : MonoBehaviour
{
    private static DontDestroyOnLoadUI instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // persist across scenes
        }
        else
        {
            Destroy(gameObject); // avoid duplicates
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu") // destroy UI in main menu
        {
            Destroy(gameObject);
        }
    }
}
