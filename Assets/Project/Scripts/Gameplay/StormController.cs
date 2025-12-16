using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening; // Keep DOTween

public class StormController : MonoBehaviour
{
    public static StormController Instance;

    [Title("Visuals")]
    [Required] public Image stormOverlayPanel;
    public Color normalStormColor = new Color(0.1f, 0.2f, 0.3f, 0.4f);
    public Color thunderFlashColor = new Color(0.8f, 0.9f, 1f, 0.6f);

    [Title("Timing")]
    public float minThunderInterval = 10f;
    public float maxThunderInterval = 20f;

    [Title("Audio")]
    [Required] public AudioSource thunderAudioSource;
    public AudioClip thunderSound;

    [Tooltip("The AudioSource playing the looping rain sound.")]
    public AudioSource rainAudioSource;
    [Range(0, 1)] public float startRainVolume = 0.2f;
    [Range(0, 1)] public float endRainVolume = 0.8f;

    [Title("Audio Ducking")]
    [ReadOnly]
    public float masterVolumeMultiplier = 1.0f; // New Control Variable for Ducking

    [Title("Sister Reactions")]
    [InfoBox("Dialogue nodes to play when thunder strikes.")]
    public List<DialogueNode> thunderReactionNodes;

    private float thunderTimer;
    private float initialLevelTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (stormOverlayPanel != null)
            stormOverlayPanel.color = normalStormColor;

        ResetTimer();

        if (Scene1Manager.Instance != null)
        {
            initialLevelTime = Scene1Manager.Instance.levelTimeInSeconds;
        }
        else
        {
            initialLevelTime = 120f;
        }

        if (rainAudioSource != null)
        {
            rainAudioSource.volume = startRainVolume;
            if (!rainAudioSource.isPlaying) rainAudioSource.Play();
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;

        // --- DYNAMIC DIFFICULTY (PANIC PHASE) ---
        // As time drops from 120s to 0s, multiplier goes from 1.0 to 0.2
        float timeRatio = 1.0f;
        if (Scene1Manager.Instance != null)
        {
            timeRatio = Scene1Manager.Instance.currentTime / Scene1Manager.Instance.levelTimeInSeconds;
        }
        float panicMultiplier = Mathf.Clamp(timeRatio, 0.2f, 1.0f);

        // Countdown
        thunderTimer -= Time.deltaTime;

        // Thunder logic
        if (thunderTimer <= 0)
        {
            TriggerThunder();
            // Reset timer using the panic multiplier (faster thunder at end of level)
            float nextDelay = Random.Range(minThunderInterval, maxThunderInterval) * panicMultiplier;
            thunderTimer = nextDelay;
        }

        // Always update rain volume
        HandleRainVolume();
    }

    private void HandleRainVolume()
    {
        if (rainAudioSource == null || Scene1Manager.Instance == null) return;

        float currentTime = Scene1Manager.Instance.currentTime;
        float progress = 1f - (currentTime / initialLevelTime);

        // MODIFIED: We multiply the calculation by our new masterVolumeMultiplier
        // This allows the SisterReactionController to "duck" the volume without breaking the rain progression
        float baseVolume = Mathf.Lerp(startRainVolume, endRainVolume, progress);
        rainAudioSource.volume = baseVolume * masterVolumeMultiplier;
    }

    private void ResetTimer()
    {
        thunderTimer = Random.Range(minThunderInterval, maxThunderInterval);
    }

    [Button("Test Thunder")]
    public void TriggerThunder()
    {
        // 1. Shake
        if (UIShake.Instance != null)
        {
            UIShake.Instance.ShakeBurden();
        }

        // 2. Play Sound
        if (thunderAudioSource != null && thunderSound != null)
        {
            thunderAudioSource.PlayOneShot(thunderSound);
        }

        // 3. Visual Flash
        if (stormOverlayPanel != null)
        {
            Sequence stormSeq = DOTween.Sequence();
            stormSeq.Append(stormOverlayPanel.DOColor(thunderFlashColor, 0.1f));
            stormSeq.Append(stormOverlayPanel.DOColor(normalStormColor, 1.5f));
        }

        // 4. Sister Reaction (Dialogue & Face)
        // If we have dialogue nodes, pick one randomly and play it
        if (DialogueManager.Instance != null && thunderReactionNodes != null && thunderReactionNodes.Count > 0)
        {
            // Only trigger occasionally (e.g., 50% chance) so she doesn't talk every single thunder strike
            if (Random.value > 0.5f)
            {
                DialogueNode randomReaction = thunderReactionNodes[Random.Range(0, thunderReactionNodes.Count)];
                DialogueManager.Instance.StartDialogue(randomReaction);
            }
        }
        // Fallback: visual face change only if no dialogue or chance failed
        else if (DialogueManager.Instance != null && Random.value > 0.5f)
        {
            DialogueManager.Instance.ForcePortrait(SisterMood.Frown);
            StartCoroutine(ResetSisterFace());
        }
    }

    private IEnumerator ResetSisterFace()
    {
        yield return new WaitForSeconds(2f);
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ForcePortrait(SisterMood.Normal);
    }
}