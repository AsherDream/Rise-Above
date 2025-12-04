using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // Required for VideoPlayer
using TMPro;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class SurvivalMeter : MonoBehaviour
{
    public static SurvivalMeter Instance;

    [Title("UI Components")]
    [Required]
    public RawImage meterRawImage; // Changed from Image to RawImage for RenderTextures

    [Required]
    public TMP_Text hpText;

    [Required]
    public VideoPlayer videoPlayer; // Reference to the Video Player component

    [Title("HP Settings")]
    public int maxHP = 100;
    public int minHP = 0;
    public int goodItemHeal = 10;
    public int badItemDamage = 15;

    [Title("Visual Stages")]
    [InfoBox("Define the Video Clip to play when HP is AT or ABOVE these percentages.")]
    public List<MeterStage> visualStages = new List<MeterStage>();

    [Title("Static Images (Optional)")]
    [InfoBox("Optional: Static sprites for 0% and 100% if you don't want video.")]
    public Sprite emptySprite; // 0%
    public Sprite fullSprite;  // 100%

    [ReadOnly]
    public int currentHP;

    [System.Serializable]
    public struct MeterStage
    {
        [Range(0, 100)]
        public int minPercentage;
        public VideoClip stageClip; // Changed from Sprite to VideoClip
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentHP = maxHP;
        UpdateUI();
    }

    public void OnGoodItemClick()
    {
        currentHP += goodItemHeal;
        if (currentHP > maxHP) currentHP = maxHP;
        UpdateUI();
    }

    public void OnBadItemClick()
    {
        currentHP -= badItemDamage;
        if (currentHP < minHP) currentHP = minHP;
        UpdateUI();
    }

    void UpdateUI()
    {
        // Update Text
        if (hpText != null)
        {
            float percentage = (float)currentHP / maxHP * 100f;
            hpText.text = $"{Mathf.RoundToInt(percentage)}%";
        }

        if (videoPlayer == null || meterRawImage == null) return;

        // --- 1. Handle 100% Full State (Static Image) ---
        if (currentHP >= maxHP && fullSprite != null)
        {
            // Stop video to save performance
            videoPlayer.Stop();
            // Swap the texture to the static sprite
            meterRawImage.texture = fullSprite.texture;
            return; // Exit early, don't play video
        }

        // --- 2. Handle 0% Empty State (Static Image) ---
        if (currentHP <= minHP && emptySprite != null)
        {
            videoPlayer.Stop();
            meterRawImage.texture = emptySprite.texture;
            return; // Exit early
        }

        // --- 3. Handle Video States (1% - 99%) ---

        // Ensure the RawImage is showing the Video Render Texture (in case we switched to static before)
        if (videoPlayer.targetTexture != null)
        {
            meterRawImage.texture = videoPlayer.targetTexture;
        }

        if (visualStages.Count > 0)
        {
            float percentage = (float)currentHP / maxHP * 100f;
            VideoClip clipToPlay = null;
            int highestThreshold = -1;

            // Find the correct video clip based on threshold
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

            // Swap the clip only if it's different
            if (clipToPlay != null && videoPlayer.clip != clipToPlay)
            {
                videoPlayer.clip = clipToPlay;
                videoPlayer.Play();
            }
            // If the video was stopped (because we came from 100% or 0%), make sure it plays
            else if (!videoPlayer.isPlaying && clipToPlay != null)
            {
                videoPlayer.Play();
            }
        }
    }
}