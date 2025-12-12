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
    public float levelTimeInSeconds = 120f;
    public int targetItemCount = 9;

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

        gameOverPanel.SetActive(false);
        levelCompletePanel.SetActive(false);

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

        if (currentTime <= 0)
        {
            TriggerGameOver("The storm arrived before you finished.");
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            float minutes = Mathf.FloorToInt(currentTime / 60);
            float seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (currentTime <= 30f) timerText.color = Color.red;
            else timerText.color = Color.white;
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

    // --- FIX: ADDED MISSING METHOD ---
    public bool IsCartFull()
    {
        return currentItems >= targetItemCount;
    }

    private void EnableCheckout()
    {
        Debug.Log("[Scene1Manager] Cart Full! Checkout enabled.");
        checkoutButton.interactable = true;

        // Immediate visual update
        if (checkoutButton.image != null) checkoutButton.image.color = Color.red;

        StartCoroutine(FlashCheckoutButton());
    }

    private IEnumerator FlashCheckoutButton()
    {
        if (checkoutButton.image == null) yield break;

        Color redColor = new Color(1f, 0.2f, 0.2f);
        Color lightRedColor = new Color(1f, 0.6f, 0.6f);

        while (isGameActive)
        {
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

        // Trigger Shake via UIShake
        if (UIShake.Instance != null)
        {
            UIShake.Instance.ShakeGameOver();
        }

        StartCoroutine(GameOverSequence(reason));
    }

    private IEnumerator GameOverSequence(string reason)
    {
        // Wait for shake
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