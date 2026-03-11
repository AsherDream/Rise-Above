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

    [Title("UI References")]
    [Required] public CanvasGroup continuePromptGroup;
    [Required] public Button fullScreenButton;

    private bool videoFinished = false;
    private bool isLoading = false; // The safety lock

    private void Start()
    {
        // 1. Force TimeScale to 1 
        Time.timeScale = 1f;

        // 2. Hide Global UI
        if (PanelManager.Instance != null) PanelManager.Instance.HideAll();

        // 3. Setup Initial State - COMPLETELY hide the button and text
        continuePromptGroup.alpha = 0f;
        continuePromptGroup.interactable = false;
        continuePromptGroup.blocksRaycasts = false;

        // Turning the game object off guarantees it cannot be clicked early
        fullScreenButton.gameObject.SetActive(false);

        // Clear inspector events and add code listener
        fullScreenButton.onClick.RemoveAllListeners();
        fullScreenButton.onClick.AddListener(OnContinueClicked);

        // 4. Start Video
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (videoFinished) return;
        videoFinished = true;
        Debug.Log("[IntroManager] Video Finished. Enabling Button.");

        // 5. Show Prompt
        continuePromptGroup.DOFade(1f, 1.0f).SetEase(Ease.InOutQuad);
        continuePromptGroup.transform.DOScale(1.1f, 1.0f).SetLoops(-1, LoopType.Yoyo);

        // 6. Enable Button ONLY after video ends
        fullScreenButton.gameObject.SetActive(true);
        fullScreenButton.interactable = true;
    }

    // --- BUTTON CLICK EVENT ---
    private void OnContinueClicked()
    {
        Debug.Log("[IntroManager] Button Clicked!");
        LoadSceneByName(nextSceneName);
    }

    // --- REUSABLE PUBLIC METHOD ---
    public void LoadSceneByName(string sceneToLoad)
    {
        if (isLoading) return; // Stop double-clicks!
        isLoading = true;

        Debug.Log($"[IntroManager] Attempting to load: {sceneToLoad}");

        // --- FIX 1: Use a scalpel instead of a sledgehammer ---
        // Only kill the animations on the text prompt, leave the Iris alone!
        if (continuePromptGroup != null)
        {
            continuePromptGroup.DOKill();
            continuePromptGroup.transform.DOKill();
        }

        // --- FIX 2: Stop the video to free up memory ---
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