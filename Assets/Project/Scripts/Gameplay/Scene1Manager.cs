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

    // --- GAME OVER & THANKS (Fixed) ---
    [Title("Game Over & Transitions")]
    [Required] public GameObject gameOverPanel;
    [Required] public TextMeshProUGUI gameOverReasonText; // <--- This was missing!
    [Required] public GameObject specialThanksPanel;
    // ----------------------------------

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
        currentTime = levelTimeInSeconds;
        currentItems = 0;
        collectedItemsList.Clear();
        isGameActive = true;
        hasBeeped = false;

        if (timerText != null) defaultTimerColor = timerText.color;

        // Hide all panels at start
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (specialThanksPanel != null) specialThanksPanel.SetActive(false);

        checkoutButton.interactable = false;
        if (checkoutButton.image != null) checkoutButton.image.color = Color.gray;

        checkoutButton.onClick.AddListener(OnCheckoutClicked);
    }

    private void Update()
    {
        if (!isGameActive) return;
        if (ItemInspector.IsInspecting) return;

        currentTime -= Time.deltaTime;
        UpdateTimerUI();

        if (currentTime <= beepStartTime && !hasBeeped && currentTime > 0)
        {
            PlayTimerBeep();
        }

        if (currentTime <= 0)
        {
            TriggerGameOver("The storm arrived before you finished.");
        }
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
        if (checkoutButton.image != null) checkoutButton.image.color = Color.red;
        if (SisterReactionController.Instance != null) SisterReactionController.Instance.TriggerAlert();
        StartCoroutine(FlashCheckoutButton());
    }

    private IEnumerator FlashCheckoutButton()
    {
        if (checkoutButton.image == null) yield break;
        Color redColor = new Color(1f, 0.2f, 0.2f);
        Color lightRedColor = new Color(1f, 0.6f, 0.6f);

        while (isGameActive)
        {
            checkoutButton.interactable = true;
            checkoutButton.image.color = redColor;
            yield return new WaitForSeconds(0.5f);
            checkoutButton.image.color = lightRedColor;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void OnCheckoutClicked()
    {
        if (!isGameActive) return;
        if (sfxSource != null && checkoutSound != null) sfxSource.PlayOneShot(checkoutSound);
        
        isGameActive = false;
        StopAllCoroutines();
        if (timerAudioSource != null && timerAudioSource.isPlaying) timerAudioSource.Stop();
        
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

    // --- TRANSITION FUNCTIONS ---

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

    public void GoToSpecialThanks()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (specialThanksPanel != null) specialThanksPanel.SetActive(true);
    }
}