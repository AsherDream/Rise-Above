using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using DG.Tweening; // REQUIRED for Audio Ducking

public class SisterReactionController : MonoBehaviour
{
    public static SisterReactionController Instance;

    [Title("Settings")]
    public float timeToIdle = 10f;
    public float hoverReactionDelay = 0.5f;

    [Title("Idle Dialogues")]
    [InfoBox("Randomly picks one of these when idling.")]
    public List<DialogueNode> idleNodes;

    [Title("Special Reactions")]
    [InfoBox("Dialogue to play when player picks 3 burden items in a row.")]
    public DialogueNode specialAnnoyedNode;

    [Title("Audio")]
    [Required] public AudioSource reactionAudioSource;
    public AudioClip idleSound;
    public AudioClip hoverSound;
    public AudioClip alertSound;

    [Title("Reaction Sounds")]
    [InfoBox("Add multiple sounds here. One will be picked randomly.")]
    public List<AudioClip> positiveReactionSounds = new List<AudioClip>();
    public List<AudioClip> negativeReactionSounds = new List<AudioClip>();

    [Title("Audio Ducking Settings")]
    [InfoBox("Assign the Music AudioSource here. The Storm is ducked automatically via code.")]
    public List<AudioSource> otherSourcesToDuck;

    [Range(0f, 1f)] public float duckedVolumePercentage = 0.3f; // Drop to 30%
    public float duckingAttack = 0.3f;  // Time to fade out
    public float duckingRelease = 1.0f; // Time to fade back in

    // Internal State
    private float lastInputTime;
    private bool isIdle = false;
    private float hoverTimer = 0f;
    private DraggableItem currentHoverItem;
    private int burdenStreak = 0;

    // Internal tracker for original volumes of "Other Sources"
    private Dictionary<AudioSource, float> originalVolumes = new Dictionary<AudioSource, float>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        lastInputTime = Time.time;

        // Cache original volumes for safety
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
        // 1. Detect Input to Reset Idle Timer
        if (Input.anyKey || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 || Input.GetMouseButton(0))
        {
            ResetIdle();
        }

        // 2. Check for Idle
        if (!isIdle && Time.time - lastInputTime > timeToIdle)
        {
            TriggerIdle();
        }

        // 3. Hover Logic
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

    public void RegisterDrop(FloodItemType type)
    {
        if (type == FloodItemType.Burden)
        {
            burdenStreak++;
            if (burdenStreak >= 3)
            {
                // Trigger a specific "Why are you picking junk?!" dialogue
                if (specialAnnoyedNode != null)
                    DialogueManager.Instance.StartDialogue(specialAnnoyedNode);

                burdenStreak = 0; // Reset
            }
        }
        else
        {
            burdenStreak = 0; // Reset if they pick a good item
        }
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

    // --- Audio Triggers ---
    public void TriggerAlert()
    {
        PlaySound(alertSound);
    }

    public void TriggerPositiveSound()
    {
        PlayRandomSound(positiveReactionSounds);
    }

    public void TriggerNegativeSound()
    {
        PlayRandomSound(negativeReactionSounds);
    }

    private void ResetIdle()
    {
        lastInputTime = Time.time;
        if (isIdle) isIdle = false;
    }

    private void TriggerIdle()
    {
        isIdle = true;
        Debug.Log("[SisterReaction] Player is Idle.");

        PlaySound(idleSound);

        if (idleNodes != null && idleNodes.Count > 0)
        {
            DialogueNode randomNode = idleNodes[Random.Range(0, idleNodes.Count)];
            DialogueManager.Instance.StartDialogue(randomNode);
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
            // JUICE: Trigger the Ducking whenever she speaks!
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
                // JUICE: Trigger the Ducking
                DuckAudio(clip.length);
            }
        }
    }

    // --- NEW DUCKING LOGIC ---

    private void DuckAudio(float clipDuration)
    {
        // 1. Duck the Storm (Accessing the variable via Singleton)
        if (StormController.Instance != null)
        {
            // Kill any running tweens on this value so they don't fight
            DOTween.Kill("StormDucking");

            // Tween the multiplier: 1.0 -> 0.3 -> 1.0
            Sequence stormSeq = DOTween.Sequence().SetId("StormDucking");

            // Fade Down
            stormSeq.Append(DOTween.To(() => StormController.Instance.masterVolumeMultiplier,
                x => StormController.Instance.masterVolumeMultiplier = x,
                duckedVolumePercentage, duckingAttack));

            // Wait for voice to finish
            stormSeq.AppendInterval(clipDuration);

            // Fade Up
            stormSeq.Append(DOTween.To(() => StormController.Instance.masterVolumeMultiplier,
                x => StormController.Instance.masterVolumeMultiplier = x,
                1.0f, duckingRelease));
        }

        // 2. Duck "Other" Sources (Music, Buzz, etc.)
        if (otherSourcesToDuck != null)
        {
            foreach (var source in otherSourcesToDuck)
            {
                if (source == null) continue;

                // If we haven't stored the volume yet, do it now
                if (!originalVolumes.ContainsKey(source)) originalVolumes[source] = source.volume;

                float startVol = originalVolumes[source];
                float targetVol = startVol * duckedVolumePercentage;

                // Kill old tweens on this specific source
                source.DOKill();

                Sequence musicSeq = DOTween.Sequence();
                musicSeq.Append(source.DOFade(targetVol, duckingAttack));
                musicSeq.AppendInterval(clipDuration);
                musicSeq.Append(source.DOFade(startVol, duckingRelease));
            }
        }
    }
}