using UnityEngine;
using Sirenix.OdinInspector;

public class BackgroundScroller : MonoBehaviour
{
    [Title("Scrolling Settings")]
    [SerializeField] private float scrollSpeed = 500f; // Speed in pixels per second
    [SerializeField] private float minXPosition = -1000f; // Left boundary
    [SerializeField] private float maxXPosition = 1000f;  // Right boundary

    [Title("Key Bindings")]
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;

    [Title("Audio")]
    [SerializeField] private AudioSource cartAudioSource; // Reference to the Audio Source

 
    [SerializeField] private float smoothness = 10f; // Higher = Snappier, Lower = Floaty
    private float currentVelocity = 0f;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (cartAudioSource == null) cartAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (ItemInspector.IsInspecting) return; // Stop logic if inspecting

        float targetVelocity = 0f;

        // Determine target direction
        if (Input.GetKey(leftKey)) targetVelocity = scrollSpeed;
        if (Input.GetKey(rightKey)) targetVelocity = -scrollSpeed;

        // Apply "Inertia" using Lerp
        currentVelocity = Mathf.Lerp(currentVelocity, targetVelocity, Time.deltaTime * smoothness);

        // Move
        if (Mathf.Abs(currentVelocity) > 0.1f)
        {
            Vector2 newPos = rect.anchoredPosition;
            newPos.x += currentVelocity * Time.deltaTime;
            newPos.x = Mathf.Clamp(newPos.x, minXPosition, maxXPosition);
            rect.anchoredPosition = newPos;

            // Smart Audio: Pitch changes with speed?
            if (cartAudioSource != null)
            {
                if (!cartAudioSource.isPlaying) cartAudioSource.Play();
                cartAudioSource.pitch = Mathf.Lerp(0.8f, 1.2f, Mathf.Abs(currentVelocity) / scrollSpeed);
            }
        }
        else
        {
            if (cartAudioSource != null && cartAudioSource.isPlaying) cartAudioSource.Stop();
        }
    }
}