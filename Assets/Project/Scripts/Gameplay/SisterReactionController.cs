using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using DG.Tweening;

public class SisterReactionController : MonoBehaviour
{
    public static SisterReactionController Instance;

    [Title("Settings")]
    public float timeToIdle = 10f;
    public float hoverReactionDelay = 0.5f;

    [Title("Idle Dialogues")]
    public List<DialogueNode> idleNodes;

    [Title("Special Reactions")]
    [InfoBox("Dialogue when picking 3 burden items in a row.")]
    public DialogueNode specialAnnoyedNode;

    [InfoBox("Dialogue when trying to add the same item twice.")]
    public DialogueNode duplicateAttemptNode;

    [Title("Audio")]
    [Required] public AudioSource reactionAudioSource;
    public AudioClip idleSound;
    public AudioClip hoverSound;
    public AudioClip alertSound;

    [Title("Reaction Sounds")]
    public List<AudioClip> positiveReactionSounds = new List<AudioClip>();
    public List<AudioClip> negativeReactionSounds = new List<AudioClip>();

    [Title("Audio Ducking Settings")]
    public List<AudioSource> otherSourcesToDuck;
    [Range(0f, 1f)] public float duckedVolumePercentage = 0.3f;
    public float duckingAttack = 0.3f;
    public float duckingRelease = 1.0f;

    // Internal State
    private float lastInputTime;
    private bool isIdle = false;
    private float hoverTimer = 0f;
    private DraggableItem currentHoverItem;
    private int burdenStreak = 0;
    private Dictionary<AudioSource, float> originalVolumes = new Dictionary<AudioSource, float>();

    // --- NEW FLAG ---
    private bool areReactionsActive = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        lastInputTime = Time.time;
        areReactionsActive = true; // Reset on load

        if (otherSourcesToDuck != null)
        {
            foreach (var source in otherSourcesToDuck)
            {
                if (source != null) originalVolumes[source] = source.volume;
            }
        }
    }

    private void Update()
    {
        // --- STOP CHECK ---
        if (!areReactionsActive) return;

        if (Input.anyKey || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 || Input.GetMouseButton(0))
        {
            ResetIdle();
        }

        if (!isIdle && Time.time - lastInputTime > timeToIdle)
        {
            TriggerIdle();
        }

        if (currentHoverItem != null)
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer > hoverReactionDelay)
            {
                hoverTimer = -999f;
                TriggerHoverReaction();
            }
        }
    }

    // --- NEW METHOD TO STOP SISTER ---
    public void StopBehaviors()
    {
        areReactionsActive = false; // Stops Update loop
        StopAllCoroutines();
        if (reactionAudioSource != null) reactionAudioSource.Stop();
    }
    // ---------------------------------

    public void RegisterDrop(FloodItemType type)
    {
        if (!areReactionsActive) return; // Block reactions if game over

        if (type == FloodItemType.Burden)
        {
            burdenStreak++;
            if (burdenStreak >= 3)
            {
                if (specialAnnoyedNode != null)
                    DialogueManager.Instance.StartDialogue(specialAnnoyedNode);
                burdenStreak = 0;
            }
        }
        else
        {
            burdenStreak = 0;
        }
    }

    public void TriggerDuplicateReaction()
    {
        if (!areReactionsActive) return;
        PlayRandomSound(negativeReactionSounds);
        if (duplicateAttemptNode != null)
        {
            DialogueManager.Instance.StartDialogue(duplicateAttemptNode);
        }
        ResetIdle();
    }

    public void RegisterHoverStart(DraggableItem item)
    {
        currentHoverItem = item;
        hoverTimer = 0f;
        ResetIdle();
    }

    public void RegisterHoverEnd(DraggableItem item)
    {
        if (currentHoverItem == item)
        {
            currentHoverItem = null;
            hoverTimer = 0f;
        }
    }

    public void TriggerAlert() { if (areReactionsActive) PlaySound(alertSound); }

    public void TriggerPositiveSound() { if (areReactionsActive) PlayRandomSound(positiveReactionSounds); }

    public void TriggerNegativeSound() { if (areReactionsActive) PlayRandomSound(negativeReactionSounds); }

    private void ResetIdle()
    {
        lastInputTime = Time.time;
        if (isIdle) isIdle = false;
    }

    private void TriggerIdle()
    {
        isIdle = true;
        PlaySound(idleSound);
        if (idleNodes != null && idleNodes.Count > 0)
        {
            DialogueManager.Instance.StartDialogue(idleNodes[Random.Range(0, idleNodes.Count)]);
        }
    }

    private void TriggerHoverReaction()
    {
        if (currentHoverItem != null && currentHoverItem.itemData != null)
        {
            PlaySound(hoverSound, 0.5f);
        }
    }

    private void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (reactionAudioSource != null && clip != null)
        {
            reactionAudioSource.PlayOneShot(clip, volume);
            DuckAudio(clip.length);
        }
    }

    private void PlayRandomSound(List<AudioClip> clips, float volume = 1f)
    {
        if (reactionAudioSource != null && clips != null && clips.Count > 0)
        {
            AudioClip clip = clips[Random.Range(0, clips.Count)];
            if (clip != null)
            {
                reactionAudioSource.PlayOneShot(clip, volume);
                DuckAudio(clip.length);
            }
        }
    }

    private void DuckAudio(float clipDuration)
    {
        if (StormController.Instance != null)
        {
            DOTween.Kill("StormDucking");
            Sequence stormSeq = DOTween.Sequence().SetId("StormDucking");
            stormSeq.Append(DOTween.To(() => StormController.Instance.masterVolumeMultiplier,
                x => StormController.Instance.masterVolumeMultiplier = x,
                duckedVolumePercentage, duckingAttack));
            stormSeq.AppendInterval(clipDuration);
            stormSeq.Append(DOTween.To(() => StormController.Instance.masterVolumeMultiplier,
                x => StormController.Instance.masterVolumeMultiplier = x,
                1.0f, duckingRelease));
        }

        if (otherSourcesToDuck != null)
        {
            foreach (var source in otherSourcesToDuck)
            {
                if (source == null) continue;
                if (!originalVolumes.ContainsKey(source)) originalVolumes[source] = source.volume;

                float startVol = originalVolumes[source];
                float targetVol = startVol * duckedVolumePercentage;

                source.DOKill();
                Sequence musicSeq = DOTween.Sequence();
                musicSeq.Append(source.DOFade(targetVol, duckingAttack));
                musicSeq.AppendInterval(clipDuration);
                musicSeq.Append(source.DOFade(startVol, duckingRelease));
            }
        }
    }
}