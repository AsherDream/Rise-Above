using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class SurvivalMeter : MonoBehaviour
{
    public static SurvivalMeter Instance;

    [Title("UI Components")]
    [Required] public RawImage meterRawImage;
    [Required] public TMP_Text hpText;
    [Required] public VideoPlayer videoPlayer;

    [Title("HP Settings")]
    public int startingMaxHP = 100;
    public int minHP = 0;

    [Title("Visual Stages")]
    public List<MeterStage> visualStages = new List<MeterStage>();
    public Sprite emptySprite;
    public Sprite fullSprite;

    // --- State Variables ---
    [ReadOnly] public int currentHP;
    [ReadOnly] public int currentMaxHP;

    [System.Serializable]
    public struct MeterStage
    {
        [Range(0, 100)] public int minPercentage;
        public VideoClip stageClip;
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

    // --- NEW LOGIC: CENTRALIZED EFFECTS ---

    public void HandleEssentialItem(int value)
    {
        currentHP += value;
        if (currentHP > currentMaxHP) currentHP = currentMaxHP;

        UpdateUI();
        Debug.Log($"<color=green>Essential Item! +{value} HP.</color>");

        // Positive Feedback
        if (SisterReactionController.Instance != null)
            SisterReactionController.Instance.TriggerPositiveSound();
    }

    public void HandleBurdenItem(int weightCost)
    {
        currentMaxHP -= weightCost;
        if (currentMaxHP < 0) currentMaxHP = 0;
        currentHP -= weightCost; // Immediate penalty

        if (currentHP > currentMaxHP) currentHP = currentMaxHP;
        if (currentHP < minHP) currentHP = minHP;

        UpdateUI();
        Debug.Log($"<color=yellow>Burden Item! Max Reduced by {weightCost}.</color>");

        // Burden Feedback
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

        // Wasteful Feedback
        if (SisterReactionController.Instance != null)
            SisterReactionController.Instance.TriggerNegativeSound();

        if (UIShake.Instance != null)
            UIShake.Instance.ShakeWasteful();

        // --- TRIGGER LIGHT FLICKER ---
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
            percentage = (float)currentHP / startingMaxHP * 100f; // Use Starting Max for visual consistency

        if (hpText != null)
        {
            hpText.text = $"{Mathf.RoundToInt(percentage)}%";
        }

        if (videoPlayer == null || meterRawImage == null) return;

        if (currentHP >= startingMaxHP && fullSprite != null)
        {
            videoPlayer.Stop();
            meterRawImage.texture = fullSprite.texture;
            return;
        }

        if (currentHP <= minHP && emptySprite != null)
        {
            videoPlayer.Stop();
            meterRawImage.texture = emptySprite.texture;
            return;
        }

        if (videoPlayer.targetTexture != null) meterRawImage.texture = videoPlayer.targetTexture;

        if (visualStages.Count > 0)
        {
            VideoClip clipToPlay = null;
            int highestThreshold = -1;

            foreach (var stage in visualStages)
            {
                if (percentage >= stage.minPercentage)
                {
                    if (stage.minPercentage > highestThreshold)
                    {
                        highestThreshold = stage.minPercentage;
                        clipToPlay = stage.stageClip;
                    }
                }
            }

            if (clipToPlay != null && videoPlayer.clip != clipToPlay)
            {
                videoPlayer.clip = clipToPlay;
                videoPlayer.Play();
            }
            else if (!videoPlayer.isPlaying && clipToPlay != null)
            {
                videoPlayer.Play();
            }
        }
    }
}