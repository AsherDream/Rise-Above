using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

public class IntroManager : MonoBehaviour
{
    [Title("Assets")]
    [Required] public VideoPlayer videoPlayer;
    [Required] public string nextSceneName = "SuperMarket_Scene";

    // --- THIS CREATES THE BOX IN THE INSPECTOR ---
    [Tooltip("Type the EXACT name of your video file inside the StreamingAssets folder")]
    public string videoFileName = "Comic V2.mp4";

    [Title("UI References")]
    [Required] public CanvasGroup continuePromptGroup;
    [Required] public Button fullScreenButton;

    private bool videoFinished = false;
    private bool isLoading = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (PanelManager.Instance != null) PanelManager.Instance.HideAll();

        continuePromptGroup.alpha = 0f;
        continuePromptGroup.interactable = false;
        continuePromptGroup.blocksRaycasts = false;

        fullScreenButton.gameObject.SetActive(false);
        fullScreenButton.onClick.RemoveAllListeners();
        fullScreenButton.onClick.AddListener(OnContinueClicked);

        // --- NEW: WEBGL SAFE VIDEO LOADING ---
        // This automatically builds the correct URL for the browser
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.url = videoPath;

        videoPlayer.loopPointReached += OnVideoFinished;

        // Prepare and play to avoid WebGL freezing
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += (source) => { videoPlayer.Play(); };
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (videoFinished) return;
        videoFinished = true;
        Debug.Log("[IntroManager] Video Finished. Enabling Button.");

        continuePromptGroup.DOFade(1f, 1.0f).SetEase(Ease.InOutQuad);
        continuePromptGroup.transform.DOScale(1.1f, 1.0f).SetLoops(-1, LoopType.Yoyo);

        fullScreenButton.gameObject.SetActive(true);
        fullScreenButton.interactable = true;
    }

    private void OnContinueClicked()
    {
        Debug.Log("[IntroManager] Button Clicked!");
        LoadSceneByName(nextSceneName);
    }

    public void LoadSceneByName(string sceneToLoad)
    {
        if (isLoading) return;
        isLoading = true;

        Debug.Log($"[IntroManager] Attempting to load: {sceneToLoad}");

        if (continuePromptGroup != null)
        {
            continuePromptGroup.DOKill();
            continuePromptGroup.transform.DOKill();
        }

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        Time.timeScale = 1f;

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("TransitionManager missing, loading standard.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
    }
}