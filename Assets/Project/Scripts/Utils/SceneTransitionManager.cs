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

    [Tooltip("How long the circle takes to open/close")]
    public float transitionDuration = 1.0f;

    [Tooltip("Buffer time to keep screen black before loading")]
    public float loadBufferTime = 0.5f;

    // Target Size (Width/Height) when fully open. 
    // 3000 is usually big enough to cover a 1920x1080 screen corner-to-corner.
    private Vector2 maxBufferSize = new Vector2(3000f, 3000f);
    private Vector2 minBufferSize = Vector2.zero;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (holeRectTransform != null) holeRectTransform.DOKill();

        // Start with hole OPEN (Game Visible)
        if (holeRectTransform != null)
        {
            holeRectTransform.sizeDelta = maxBufferSize;
            holeRectTransform.localScale = Vector3.one; // Ensure scale is always 1
            if (holeCanvasGroup != null) holeCanvasGroup.blocksRaycasts = false;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(string sceneName)
    {
        if (holeRectTransform == null) return;

        holeRectTransform.DOKill();
        if (holeCanvasGroup != null) holeCanvasGroup.blocksRaycasts = true;

        // 1. IRIS CLOSE (Resize to 0,0)
        // When Size is 0, the Mask is 0, so the 'Inverse Mask' (Curtain) draws everywhere -> Black Screen.
        holeRectTransform.DOSizeDelta(minBufferSize, transitionDuration)
            .SetEase(Ease.InOutExpo)
            .OnComplete(() =>
            {
                StartCoroutine(LoadSceneAsyncRoutine(sceneName));
            });
    }

    private IEnumerator LoadSceneAsyncRoutine(string sceneName)
    {
        yield return new WaitForSeconds(0.1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(loadBufferTime);
        asyncLoad.allowSceneActivation = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (holeRectTransform == null) return;

        holeRectTransform.DOKill();

        // 1. Ensure Hole is closed
        holeRectTransform.sizeDelta = minBufferSize;
        holeRectTransform.localScale = Vector3.one; // Reset scale just in case
        if (holeCanvasGroup != null) holeCanvasGroup.blocksRaycasts = true;

        // 2. IRIS OPEN (Resize to 3000,3000)
        holeRectTransform.DOSizeDelta(maxBufferSize, transitionDuration)
            .SetEase(Ease.InOutExpo)
            .OnComplete(() =>
            {
                if (holeCanvasGroup != null) holeCanvasGroup.blocksRaycasts = false;
            });
    }
}