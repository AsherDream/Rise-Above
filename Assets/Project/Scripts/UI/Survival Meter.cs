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
    [ReadOnly] public int currentMaxHP; // The cap that can shrink

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
        // Initialize HP
        currentMaxHP = startingMaxHP;
        currentHP = startingMaxHP;

        UpdateUI();
    }

    // --- NEW LOGIC FOR FLOOD TYPES ---

    public void HandleEssentialItem(int value)
    {
        currentHP += value;

        // FIX: Clamp to currentMaxHP, NOT startingMaxHP
        if (currentHP > currentMaxHP) currentHP = currentMaxHP;

        UpdateUI();
        Debug.Log($"<color=green>Essential Item! +{value} HP. Current: {currentHP}/{currentMaxHP}</color>");
    }

    public void HandleBurdenItem(int weightCost)
    {
        // 1. Reduce the Ceiling (Permanent)
        currentMaxHP -= weightCost;
        if (currentMaxHP < 0) currentMaxHP = 0;

        // 2. Reduce Current HP (Immediate Penalty)
        // If we don't do this, adding a burden at 50/100 -> 50/85 feels like "free" healing percentage-wise.
        // We subtract the weight so the player feels the "heavy" impact immediately.
        currentHP -= weightCost;

        // 3. Safety Clamps
        if (currentHP > currentMaxHP) currentHP = currentMaxHP;
        if (currentHP < minHP) currentHP = minHP;

        UpdateUI();
        Debug.Log($"<color=yellow>Burden Item! Max Capacity reduced by {weightCost}. New Cap: {currentMaxHP}. Current: {currentHP}</color>");

        CheckGameOver();
    }

    public void HandleWastefulItem(int damage)
    {
        currentHP -= damage;
        if (currentHP < minHP) currentHP = minHP;

        UpdateUI();
        Debug.Log($"<color=red>Wasteful Item! -{damage} HP. Current: {currentHP}/{currentMaxHP}</color>");

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
        // VISUAL FIX: Calculate percentage based on STARTING Max HP (100).
        // This ensures that if Max Capacity drops to 85, the visual meter NEVER goes to 100% full.
        // It will look like the container has shrunk or can't be filled.
        float percentage = (float)currentHP / startingMaxHP * 100f;

        if (hpText != null)
        {
            hpText.text = $"{Mathf.RoundToInt(percentage)}%";
        }

        if (videoPlayer == null || meterRawImage == null) return;

        // 1. Handle 100% Full State (Only show if we are actually at STARTING MAX)
        // If cap is 85, we should never show the "Perfectly Full" sprite.
        if (currentHP >= startingMaxHP && fullSprite != null)
        {
            videoPlayer.Stop();
            meterRawImage.texture = fullSprite.texture;
            return;
        }

        // 2. Handle 0% Empty State
        if (currentHP <= minHP && emptySprite != null)
        {
            videoPlayer.Stop();
            meterRawImage.texture = emptySprite.texture;
            return;
        }

        // 3. Handle Video States
        if (videoPlayer.targetTexture != null)
        {
            meterRawImage.texture = videoPlayer.targetTexture;
        }

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