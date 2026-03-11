using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Title("Transition Settings")]
    [Required] public RectTransform holeRectTransform;
    [Required] public CanvasGroup holeCanvasGroup;

    public float transitionDuration = 1.0f;
    public float loadBufferTime = 0.5f;

    private Vector2 maxBufferSize = new Vector2(3000f, 3000f);
    private Vector2 minBufferSize = Vector2.zero;

    private Canvas parentCanvas;

    // --- NEW: Target Memory ---
    private string expectedTargetScene = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (holeRectTransform != null)
        {
            parentCanvas = holeRectTransform.GetComponentInParent<Canvas>();
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        ForceCanvasVisible();
        if (holeRectTransform != null)
        {
            holeRectTransform.DOKill();
            holeRectTransform.sizeDelta = maxBufferSize;
            if (holeCanvasGroup != null) holeCanvasGroup.blocksRaycasts = false;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (holeRectTransform != null) holeRectTransform.DOKill();
    }

    public void LoadScene(string sceneName)
    {
        if (holeRectTransform == null) return;

        expectedTargetScene = sceneName;

        Debug.Log($"[SceneTransitionManager] 1. Starting transition to scene: {sceneName}");

        ForceCanvasVisible();
        holeRectTransform.DOKill();
        if (holeCanvasGroup != null) holeCanvasGroup.blocksRaycasts = true;

        // --- ADDED .SetUpdate(true) HERE ---
        holeRectTransform.DOSizeDelta(minBufferSize, transitionDuration)
            .SetEase(Ease.InOutExpo)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                Debug.Log("[SceneTransitionManager] 2. Iris closed. Starting background load.");
                StartCoroutine(LoadSceneAsyncRoutine(sceneName));
            });
    }

    private IEnumerator LoadSceneAsyncRoutine(string sceneName)
    {
        yield return new WaitForSecondsRealtime(0.1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        Debug.Log("[SceneTransitionManager] 3. Background load complete.");
        yield return new WaitForSecondsRealtime(loadBufferTime);
        asyncLoad.allowSceneActivation = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (holeRectTransform == null) return;

        if (!string.IsNullOrEmpty(expectedTargetScene) && scene.name != expectedTargetScene)
        {
            return;
        }

        Debug.Log($"[SceneTransitionManager] 4. Scene '{scene.name}' active. Opening Iris...");

        ForceCanvasVisible();
        holeRectTransform.DOKill();

        holeRectTransform.sizeDelta = minBufferSize;
        holeRectTransform.localScale = Vector3.one;
        if (holeCanvasGroup != null) holeCanvasGroup.blocksRaycasts = true;

        // --- ADDED .SetUpdate(true) HERE ---
        holeRectTransform.DOSizeDelta(maxBufferSize, transitionDuration)
            .SetEase(Ease.InOutExpo)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                Debug.Log("[SceneTransitionManager] 5. Transition fully complete.");
                if (holeCanvasGroup != null) holeCanvasGroup.blocksRaycasts = false;

                expectedTargetScene = "";
            });
    }

    private void ForceCanvasVisible()
    {
        if (parentCanvas != null)
        {
            parentCanvas.enabled = true;
            parentCanvas.sortingOrder = 32000;
        }
    }
}