using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;

public class Scene1Manager : MonoBehaviour
{
    public static Scene1Manager Instance;

    [Title("Level Settings")]
    public float levelTimeInSeconds = 120f;
    public int targetItemCount = 9;

    [Title("Timer Beep Settings")]
    public float beepStartTime = 10f;
    [Required] public AudioSource timerAudioSource;
    public AudioClip beepSound;

    [Title("Checkout Audio")]
    public AudioSource sfxSource;
    public AudioClip checkoutSound;

    [Title("UI References")]
    [Required] public TextMeshProUGUI timerText;
    [Required] public Button checkoutButton;
    [Required] public GameObject levelCompletePanel;

    [Title("Game Over & Transitions")]
    [Required] public GameObject gameOverPanel;
    [Required] public TextMeshProUGUI gameOverReasonText;

    // --- REMOVED: specialThanksPanel (It lives in Global UI now) --- 

    [ReadOnly]
    public List<InspectableItemData> collectedItemsList = new List<InspectableItemData>();

    [Title("Debug Info")]
    [ReadOnly] public float currentTime;
    [ReadOnly] public int currentItems;
    private bool isGameActive = true;
    private Color defaultTimerColor;
    private bool hasBeeped = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Time.timeScale = 1f;

        currentTime = levelTimeInSeconds;
        currentItems = 0;
        collectedItemsList.Clear();
        isGameActive = true;
        hasBeeped = false;

        if (timerText != null) defaultTimerColor = timerText.color;

        // Hide panels
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        // SpecialThanks is handled by PanelManager now, so we don't hide it here.

        // Disable checkout initially
        checkoutButton.interactable = false;
        if (checkoutButton.image != null)
        {
            checkoutButton.image.color = Color.gray;
        }
        checkoutButton.onClick.AddListener(OnCheckoutClicked);
    }

    private void Update()
    {
        if (!isGameActive) return;
        if (ItemInspector.IsInspecting) return;

        currentTime -= Time.deltaTime;
        UpdateTimerUI();

        if (currentTime <= beepStartTime && !hasBeeped && currentTime > 0) PlayTimerBeep();

        if (currentTime <= 0) TriggerGameOver("The storm arrived before you finished.");
    }

    private void PlayTimerBeep()
    {
        hasBeeped = true;
        if (timerAudioSource != null && beepSound != null)
            timerAudioSource.PlayOneShot(beepSound);
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            float minutes = Mathf.FloorToInt(currentTime / 60);
            float seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            if (currentTime <= 30f) timerText.color = Color.red;
            else timerText.color = defaultTimerColor;
        }
    }

    public bool IsItemAlreadyInCart(InspectableItemData itemToCheck)
    {
        if (itemToCheck == null) return false;
        foreach (var collectedItem in collectedItemsList)
        {
            if (collectedItem.itemName == itemToCheck.itemName) return true;
        }
        return false;
    }

    public void OnItemCollected(InspectableItemData itemData)
    {
        if (itemData != null) collectedItemsList.Add(itemData);
        currentItems++;
        if (currentItems >= targetItemCount) EnableCheckout();
    }

    public bool IsCartFull() { return currentItems >= targetItemCount; }

    private void EnableCheckout()
    {
        checkoutButton.interactable = true;
        if (checkoutButton.image != null) checkoutButton.image.color = Color.white;
        if (SisterReactionController.Instance != null) SisterReactionController.Instance.TriggerAlert();
    }

    public void OnCheckoutClicked()
    {
        if (!isGameActive) return;
        if (sfxSource != null && checkoutSound != null) sfxSource.PlayOneShot(checkoutSound);
        isGameActive = false;
        StopAllCoroutines();

        if (timerAudioSource != null && timerAudioSource.isPlaying) timerAudioSource.Stop();

        if (SisterReactionController.Instance != null)
            SisterReactionController.Instance.StopBehaviors();

        if (StormController.Instance != null)
            StormController.Instance.StopStorm();

        levelCompletePanel.SetActive(true);
    }

    public void TriggerGameOver(string reason)
    {
        if (!isGameActive) return;
        isGameActive = false;
        if (timerAudioSource != null && timerAudioSource.isPlaying) timerAudioSource.Stop();
        if (UIShake.Instance != null) UIShake.Instance.ShakeGameOver();
        StartCoroutine(GameOverSequence(reason));
    }

    private IEnumerator GameOverSequence(string reason)
    {
        yield return new WaitForSeconds(1.0f);
        gameOverPanel.SetActive(true);
        if (gameOverReasonText != null) gameOverReasonText.text = reason;
        Time.timeScale = 0f;
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // --- UPDATED LOGIC ---
    public void GoToSpecialThanks()
    {
        // 1. Hide the local panels in this scene
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // 2. Ask the Global PanelManager to show the credits
        // Since PanelManager is in UI_Global (a different scene), we access it via Singleton
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.OpenCredits();
        }
        else
        {
            Debug.LogError("PanelManager not found! Is UI_Global loaded?");
        }
    }
}