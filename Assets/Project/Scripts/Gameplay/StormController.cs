using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

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
    public float masterVolumeMultiplier = 1.0f;

    [Title("Sister Reactions")]
    public List<DialogueNode> thunderReactionNodes;

    private float thunderTimer;
    private float initialLevelTime;

    // --- NEW FLAG ---
    private bool isStormActive = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        isStormActive = true; // Reset

        if (stormOverlayPanel != null)
            stormOverlayPanel.color = normalStormColor;

        ResetTimer();

        if (Scene1Manager.Instance != null)
            initialLevelTime = Scene1Manager.Instance.levelTimeInSeconds;
        else
            initialLevelTime = 120f;

        if (rainAudioSource != null)
        {
            rainAudioSource.volume = startRainVolume;
            if (!rainAudioSource.isPlaying) rainAudioSource.Play();
        }
    }

    private void Update()
    {
        // --- STOP CHECK ---
        if (!isStormActive || Time.timeScale == 0) return;

        float timeRatio = 1.0f;
        if (Scene1Manager.Instance != null)
        {
            timeRatio = Scene1Manager.Instance.currentTime / Scene1Manager.Instance.levelTimeInSeconds;
        }
        float panicMultiplier = Mathf.Clamp(timeRatio, 0.2f, 1.0f);

        thunderTimer -= Time.deltaTime;

        if (thunderTimer <= 0)
        {
            TriggerThunder();
            float nextDelay = Random.Range(minThunderInterval, maxThunderInterval) * panicMultiplier;
            thunderTimer = nextDelay;
        }

        HandleRainVolume();
    }

    // --- NEW METHOD TO STOP STORM ---
    public void StopStorm()
    {
        isStormActive = false;
        StopAllCoroutines();

        // Kill thunder
        if (thunderAudioSource != null) thunderAudioSource.Stop();

        // Fade out rain nicely
        if (rainAudioSource != null)
        {
            rainAudioSource.DOFade(0f, 2.0f);
        }
    }
    // --------------------------------

    private void HandleRainVolume()
    {
        if (rainAudioSource == null || Scene1Manager.Instance == null) return;

        float currentTime = Scene1Manager.Instance.currentTime;
        float progress = 1f - (currentTime / initialLevelTime);

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
        if (!isStormActive) return;

        if (UIShake.Instance != null) UIShake.Instance.ShakeBurden();

        if (thunderAudioSource != null && thunderSound != null)
            thunderAudioSource.PlayOneShot(thunderSound);

        if (GlobalSettingsManager.IsFlashingAllowed)
        {
            if (stormOverlayPanel != null)
            {
                Sequence stormSeq = DOTween.Sequence();
                stormSeq.Append(stormOverlayPanel.DOColor(thunderFlashColor, 0.1f));
                stormSeq.Append(stormOverlayPanel.DOColor(normalStormColor, 1.5f));
            }
        }

        if (DialogueManager.Instance != null && thunderReactionNodes != null && thunderReactionNodes.Count > 0)
        {
            if (Random.value > 0.5f)
            {
                DialogueNode randomReaction = thunderReactionNodes[Random.Range(0, thunderReactionNodes.Count)];
                DialogueManager.Instance.StartDialogue(randomReaction);
            }
        }
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