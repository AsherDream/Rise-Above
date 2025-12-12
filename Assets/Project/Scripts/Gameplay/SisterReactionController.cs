using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class SisterReactionController : MonoBehaviour
{
    public static SisterReactionController Instance;

    [Title("Settings")]
    public float timeToIdle = 10f;
    public float hoverReactionDelay = 0.5f;

    [Title("Idle Dialogues")]
    [InfoBox("Randomly picks one of these when idling.")]
    public List<DialogueNode> idleNodes;

    [Title("Audio")]
    [Required] public AudioSource reactionAudioSource;
    public AudioClip idleSound;
    public AudioClip hoverSound;
    public AudioClip alertSound;

    [Title("Reaction Sounds")]
    [InfoBox("Add multiple sounds here. One will be picked randomly.")]
    public List<AudioClip> positiveReactionSounds = new List<AudioClip>();
    public List<AudioClip> negativeReactionSounds = new List<AudioClip>();

    // Internal State
    private float lastInputTime;
    private bool isIdle = false;
    private float hoverTimer = 0f;
    private DraggableItem currentHoverItem;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        lastInputTime = Time.time;
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
            }
        }
    }
}