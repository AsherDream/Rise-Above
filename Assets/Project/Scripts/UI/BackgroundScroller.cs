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

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Get the current local position
        Vector2 newPosition = rect.anchoredPosition;

        // Check for 'left' key
        if (Input.GetKey(leftKey))
        {
            // Move right (to show what's on the left)
            newPosition.x += scrollSpeed * Time.deltaTime;
        }

        // Check for 'right' key
        if (Input.GetKey(rightKey))
        {
            // Move left (to show what's on the right)
            newPosition.x -= scrollSpeed * Time.deltaTime;
        }

        // Clamp the position to stay within your boundaries
        newPosition.x = Mathf.Clamp(newPosition.x, minXPosition, maxXPosition);

        // Apply the new position
        rect.anchoredPosition = newPosition;
    }
}