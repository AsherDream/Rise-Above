using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;

public class SurvivalMeter : MonoBehaviour
{
    public static SurvivalMeter Instance;

    [Title("UI Components")]
    [Required] public Image meterImage; // CHANGED: From RawImage to Image
    [Required] public TMP_Text hpText;

    // REMOVED: VideoPlayer reference

    [Title("HP Settings")]
    public int startingMaxHP = 100;
    public int minHP = 0;

    [Title("Animation Settings")]
    [InfoBox("How fast the water ripples (seconds per frame). Lower = Faster.")]
    public float frameRate = 0.15f;

    [Title("Visual Stages")]
    public List<MeterStage> visualStages = new List<MeterStage>();

    // --- State Variables ---
    [ReadOnly] public int currentHP;
    [ReadOnly] public int currentMaxHP;

    private Coroutine animationRoutine;
    private List<Sprite> currentActiveFrames;

    [System.Serializable]
    public struct MeterStage
    {
        [Range(0, 100)] public int minPercentage;
        [Tooltip("Drag the 3 images (e.g. 20_1, 20_2, 20_3) here.")]
        public List<Sprite> animationFrames;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentMaxHP = startingMaxHP;
        currentHP = startingMaxHP;
        UpdateUI();
    }

    public void HandleEssentialItem(int value)
    {
        currentHP += value;
        if (currentHP > currentMaxHP) currentHP = currentMaxHP;

        UpdateUI();
        Debug.Log($"<color=green>Essential Item! +{value} HP.</color>");

        if (SisterReactionController.Instance != null)
            SisterReactionController.Instance.TriggerPositiveSound();
    }

    public void HandleBurdenItem(int weightCost)
    {
        currentMaxHP -= weightCost;
        if (currentMaxHP < 0) currentMaxHP = 0;
        currentHP -= weightCost;

        if (currentHP > currentMaxHP) currentHP = currentMaxHP;
        if (currentHP < minHP) currentHP = minHP;

        UpdateUI();
        Debug.Log($"<color=yellow>Burden Item! Max Reduced by {weightCost}.</color>");

        if (SisterReactionController.Instance != null)
            SisterReactionController.Instance.TriggerNegativeSound();

        if (UIShake.Instance != null)
            UIShake.Instance.ShakeBurden();

        CheckGameOver();
    }

    public void HandleWastefulItem(int damage)
    {
        currentHP -= damage;
        if (currentHP < minHP) currentHP = minHP;

        UpdateUI();
        Debug.Log($"<color=red>Wasteful Item! -{damage} HP.</color>");

        if (SisterReactionController.Instance != null)
            SisterReactionController.Instance.TriggerNegativeSound();

        if (UIShake.Instance != null)
            UIShake.Instance.ShakeWasteful();

        if (LightFlickerController.Instance != null)
            LightFlickerController.Instance.OnGridDamaged(0.1f);

        CheckGameOver();
    }

    private void CheckGameOver()
    {
        if (currentHP <= 0 && Scene1Manager.Instance != null)
        {
            Scene1Manager.Instance.TriggerGameOver("Morale reached zero.\nThe sister had a panic attack.");
        }
    }

    void UpdateUI()
    {
        float percentage = 0;
        if (currentMaxHP > 0)
            percentage = (float)currentHP / startingMaxHP * 100f;

        if (hpText != null)
        {
            hpText.text = $"{Mathf.RoundToInt(percentage)}%";
        }

        if (meterImage == null) return;

        // --- Determine which Stage to play ---
        List<Sprite> bestMatchFrames = null;
        int highestThreshold = -1;

        if (visualStages != null)
        {
            foreach (var stage in visualStages)
            {
                if (percentage >= stage.minPercentage)
                {
                    if (stage.minPercentage > highestThreshold)
                    {
                        highestThreshold = stage.minPercentage;
                        bestMatchFrames = stage.animationFrames;
                    }
                }
            }
        }

        // If the frames changed, start a new animation loop
        if (bestMatchFrames != null && bestMatchFrames != currentActiveFrames)
        {
            currentActiveFrames = bestMatchFrames;

            if (animationRoutine != null) StopCoroutine(animationRoutine);
            animationRoutine = StartCoroutine(PlayWaterAnimation());
        }
    }

    private IEnumerator PlayWaterAnimation()
    {
        if (currentActiveFrames == null || currentActiveFrames.Count == 0) yield break;

        int index = 0;
        while (true)
        {
            if (currentActiveFrames.Count > 0)
            {
                // Wrap index: 0, 1, 2, 0, 1, 2...
                index = index % currentActiveFrames.Count;

                if (meterImage != null)
                    meterImage.sprite = currentActiveFrames[index];

                index++;
            }
            yield return new WaitForSeconds(frameRate);
        }
    }
}