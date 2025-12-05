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

    [Title("UI References")]
    [Required] public TextMeshProUGUI timerText;
    [Required] public Button checkoutButton;
    [Required] public GameObject gameOverPanel;
    [Required] public TextMeshProUGUI gameOverReasonText;
    [Required] public GameObject levelCompletePanel;
    [Required] public TextMeshProUGUI summaryText;

    // Internal State
    private float currentTime;
    private int currentItems;
    private bool isGameActive = true;

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

        // Setup UI State
        gameOverPanel.SetActive(false);
        levelCompletePanel.SetActive(false);

        // Setup Checkout Button (Disabled/Gray initially)
        checkoutButton.interactable = false;

        // Ensure the button has an image component before trying to set its color
        if (checkoutButton.image != null)
        {
            checkoutButton.image.color = Color.gray;
        }

        checkoutButton.onClick.AddListener(OnCheckoutClicked);
    }

    private void Update()
    {
        if (!isGameActive) return;

        // Pause timer while inspecting items
        if (ItemInspector.IsInspecting) return;

        // Timer Logic
        currentTime -= Time.deltaTime;
        UpdateTimerUI();

        if (currentTime <= 0)
        {
            TriggerGameOver("The storm arrived before you finished.");
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // Format time as minutes:seconds
            float minutes = Mathf.FloorToInt(currentTime / 60);
            float seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Optional: Turn red when time is low (e.g., last 30 seconds)
            if (currentTime <= 30f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    // --- Public Logic called by DropZone ---
    public void OnItemCollected()
    {
        currentItems++;
        Debug.Log($"[Scene1Manager] Items: {currentItems}/{targetItemCount}");

        if (currentItems >= targetItemCount)
        {
            EnableCheckout();
        }
    }

    // --- NEW: This is the missing method! ---
    public bool IsCartFull()
    {
        return currentItems >= targetItemCount;
    }

    // --- Button Logic ---
    private void EnableCheckout()
    {
        Debug.Log("[Scene1Manager] Cart Full! Checkout enabled.");
        checkoutButton.interactable = true;

        // Start "Flashing Red" effect
        StartCoroutine(FlashCheckoutButton());
    }

    private IEnumerator FlashCheckoutButton()
    {
        // Only flash if we have an image component to tint
        if (checkoutButton.image == null) yield break;

        while (isGameActive)
        {
            checkoutButton.image.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            checkoutButton.image.color = new Color(1f, 0.5f, 0.5f); // Lighter red
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void OnCheckoutClicked()
    {
        if (!isGameActive) return;
        isGameActive = false; // Stop timer, stop input

        // Show Success Screen
        levelCompletePanel.SetActive(true);

        // Calculate Logic for Summary (Simple version)
        if (SurvivalMeter.Instance != null)
        {
            int hp = SurvivalMeter.Instance.currentHP;

            // Calculate days survived (simple formula: 20 HP = 1 Day)
            int daysSurvived = (hp / 20) + 1;

            summaryText.text = $"You secured {currentItems} items.\nMorale: {hp}%\n\nBased on your supplies,\nyou can survive for {daysSurvived} days.";
        }
        else
        {
            summaryText.text = $"You secured {currentItems} items.\n\nGood luck surviving the storm.";
        }
    }

    // --- Game Over Logic ---
    public void TriggerGameOver(string reason)
    {
        if (!isGameActive) return;
        isGameActive = false;

        gameOverPanel.SetActive(true);
        if (gameOverReasonText != null) gameOverReasonText.text = reason;

        // Optional: Stop time so physics/drag stops
        Time.timeScale = 0f;
    }

    // --- Scene Navigation (Hook these to buttons in Inspector) ---
    public void RetryLevel()
    {
        Time.timeScale = 1f; // Important: Unpause before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f; // Important: Unpause before loading new scene
        SceneManager.LoadScene("MainMenu");
    }
}