using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections;

public class Scene1Manager : MonoBehaviour
{
    public static Scene1Manager Instance;

    [Title("Level Settings")]
    public float levelTimeInSeconds = 120f; // 2 Minutes
    public int targetItemCount = 9;

    [Title("Timer Beep Settings")]
    [Tooltip("Time in seconds when the timer starts beeping.")]
    public float beepStartTime = 10f;
    [Required] public AudioSource timerAudioSource;
    public AudioClip beepSound;

    [Title("UI References")]
    [Required] public TextMeshProUGUI timerText;
    [Required] public Button checkoutButton;
    [Required] public GameObject gameOverPanel;
    [Required] public TextMeshProUGUI gameOverReasonText;
    [Required] public GameObject levelCompletePanel;
    [Required] public TextMeshProUGUI summaryText;

    // Internal State
    [Title("Debug Info")]
    [ReadOnly] public float currentTime;
    [ReadOnly] public int currentItems;
    private bool isGameActive = true;

    // Store the original color you set in the Inspector
    private Color defaultTimerColor;
    private bool hasBeeped = false; // New flag to play sound only once

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentTime = levelTimeInSeconds;
        currentItems = 0;
        isGameActive = true;
        hasBeeped = false; // Reset flag

        // Save the color you picked in the editor
        if (timerText != null)
        {
            defaultTimerColor = timerText.color;
        }

        gameOverPanel.SetActive(false);
        levelCompletePanel.SetActive(false);

        // Initial Button State
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

        // Check for Beep (Only play once)
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
        hasBeeped = true; // Ensure it only plays once
        if (timerAudioSource != null && beepSound != null)
        {
            timerAudioSource.PlayOneShot(beepSound);
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            float minutes = Mathf.FloorToInt(currentTime / 60);
            float seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Turn red only when time is low, otherwise use your custom color
            if (currentTime <= 30f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = defaultTimerColor;
            }
        }
    }

    public void OnItemCollected()
    {
        currentItems++;
        Debug.Log($"[Scene1Manager] Items: {currentItems}/{targetItemCount}");

        if (currentItems >= targetItemCount)
        {
            EnableCheckout();
        }
    }

    public bool IsCartFull()
    {
        return currentItems >= targetItemCount;
    }

    private void EnableCheckout()
    {
        Debug.Log("[Scene1Manager] Cart Full! Checkout enabled.");
        checkoutButton.interactable = true;

        if (checkoutButton.image != null) checkoutButton.image.color = Color.red;

        // Play Alert Sound
        if (SisterReactionController.Instance != null)
        {
            SisterReactionController.Instance.TriggerAlert();
        }

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
        isGameActive = false;

        StopAllCoroutines();
        // Also stop the beeping sound if checkout happens
        if (timerAudioSource != null && timerAudioSource.isPlaying) timerAudioSource.Stop();

        levelCompletePanel.SetActive(true);

        if (SurvivalMeter.Instance != null)
        {
            int hp = SurvivalMeter.Instance.currentHP;
            int daysSurvived = (hp / 20) + 1;
            summaryText.text = $"You secured {currentItems} items.\nMorale: {hp}%\n\nBased on your supplies,\nyou can survive for {daysSurvived} days.";
        }
        else
        {
            summaryText.text = $"You secured {currentItems} items.\n\nGood luck surviving the storm.";
        }
    }

    public void TriggerGameOver(string reason)
    {
        if (!isGameActive) return;
        isGameActive = false;

        // Stop beep sound on game over
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
}