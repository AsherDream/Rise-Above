using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;
using System.Collections;

public class LightFlickerController : MonoBehaviour
{
    public static LightFlickerController Instance;

    [Title("Visuals")]
    [Required] public Image tintPanel;

    [Tooltip("The normal darkness color (e.g. Alpha 0.4)")]
    public Color normalColor = new Color(0, 0, 0, 0.4f);

    [Tooltip("The 'Power Out' darkness color (e.g. Alpha 0.95)")]
    public Color blackoutColor = new Color(0, 0, 0, 0.95f);

    [Title("Flicker Settings")]
    [Range(0f, 1f)] public float currentStability = 1.0f; // 1 = Stable, 0 = Broken
    public float minFlickerInterval = 0.5f;
    public float maxFlickerInterval = 5f;

    [Title("Audio")]
    public AudioSource electricBuzzSource;
    public AudioClip buzzSound;

    private float flickerTimer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (tintPanel != null) tintPanel.color = normalColor;
        ResetTimer();
    }

    private void Update()
    {
        if (currentStability >= 1f) return; // Perfect stability = no flickering

        flickerTimer -= Time.deltaTime;
        if (flickerTimer <= 0)
        {
            StartCoroutine(DoFlicker());
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        // Lower stability = faster flickering
        float modifier = Mathf.Lerp(0.2f, 1f, currentStability);
        flickerTimer = Random.Range(minFlickerInterval * modifier, maxFlickerInterval * modifier);
    }

    // Call this when a BAD item is dropped
    [Button("Trigger Damage Flicker")]
    public void OnGridDamaged(float damageAmount)
    {
        // Reduce stability permanently or temporarily
        currentStability -= damageAmount;
        if (currentStability < 0) currentStability = 0;

        // Immediate violent flicker
        StartCoroutine(DoViolentFlicker());
    }

    private IEnumerator DoFlicker()
    {
        // Simple flicker: Dark -> Normal
        if (tintPanel == null) yield break;

        // Random chance to play buzz sound
        if (electricBuzzSource != null && buzzSound != null && Random.value > 0.7f)
            electricBuzzSource.PlayOneShot(buzzSound, 0.5f);

        int flashes = Random.Range(1, 4);
        for (int i = 0; i < flashes; i++)
        {
            tintPanel.color = blackoutColor;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            tintPanel.color = normalColor;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
        }
    }

    private IEnumerator DoViolentFlicker()
    {
        if (tintPanel == null) yield break;

        if (electricBuzzSource != null && buzzSound != null)
            electricBuzzSource.PlayOneShot(buzzSound, 1f);

        // Long blackout sequence
        tintPanel.color = blackoutColor;
        yield return new WaitForSeconds(0.2f);
        tintPanel.color = normalColor;
        yield return new WaitForSeconds(0.1f);
        tintPanel.color = blackoutColor;
        yield return new WaitForSeconds(0.5f); // Long pause in darkness
        tintPanel.color = normalColor;
    }
}