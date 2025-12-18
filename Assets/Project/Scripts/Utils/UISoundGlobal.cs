using UnityEngine;
using Sirenix.OdinInspector;

public class UISoundGlobal : MonoBehaviour
{
    public static UISoundGlobal Instance;

    [Title("Audio Settings")]
    [Required] public AudioSource uiAudioSource;
    [Required] public AudioClip genericClickSound;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayClick()
    {
        if (uiAudioSource != null && genericClickSound != null)
        {
            // Randomize pitch slightly for realism
            uiAudioSource.pitch = Random.Range(0.95f, 1.05f);
            uiAudioSource.PlayOneShot(genericClickSound);
        }
    }
}