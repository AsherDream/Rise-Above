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

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (cartAudioSource == null) cartAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // If the inspector is open, stop moving and stop sound
        if (ItemInspector.IsInspecting)
        {
            if (cartAudioSource != null && cartAudioSource.isPlaying) cartAudioSource.Stop();
            return;
        }

        Vector2 newPosition = rect.anchoredPosition;
        bool isMoving = false; // Track if we are moving this frame

        // Check for 'left' key
        if (Input.GetKey(leftKey))
        {
            newPosition.x += scrollSpeed * Time.deltaTime;
            isMoving = true;
        }

        // Check for 'right' key
        if (Input.GetKey(rightKey))
        {
            newPosition.x -= scrollSpeed * Time.deltaTime;
            isMoving = true;
        }

        // Apply movement logic
        if (isMoving)
        {
            // Clamp position
            newPosition.x = Mathf.Clamp(newPosition.x, minXPosition, maxXPosition);
            rect.anchoredPosition = newPosition;

            // Handle Audio
            if (cartAudioSource != null && !cartAudioSource.isPlaying)
            {
                cartAudioSource.Play();
            }
        }
        else
        {
            // Stop Audio if not moving
            if (cartAudioSource != null && cartAudioSource.isPlaying)
            {
                cartAudioSource.Stop();
            }
        }
    }
}