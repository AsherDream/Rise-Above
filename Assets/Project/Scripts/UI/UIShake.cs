using UnityEngine;
using Sirenix.OdinInspector;

public class UIShake : MonoBehaviour
{
    public static UIShake Instance;

    [Required]
    public RectTransform targetToShake; // The "ShakeContainer"

    private Vector2 originalPos;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 10f; // Pixels to shake
    private float dampingSpeed = 1.0f;
    private bool isShaking = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (targetToShake != null)
            originalPos = targetToShake.anchoredPosition;
    }

    private void Update()
    {
        if (shakeDuration > 0)
        {
            if (targetToShake != null)
            {
                // Shake around the original position
                Vector2 randomOffset = Random.insideUnitCircle * shakeMagnitude;
                targetToShake.anchoredPosition = originalPos + randomOffset;
            }

            shakeDuration -= Time.unscaledDeltaTime * dampingSpeed;
            isShaking = true;
        }
        else
        {
            if (isShaking)
            {
                isShaking = false;
                if (targetToShake != null) targetToShake.anchoredPosition = originalPos;
            }
        }
    }

    [Button("Test Wasteful Shake")]
    public void ShakeWasteful()
    {
        TriggerShake(0.2f, 5f); // Shake 5 pixels
    }

    [Button("Test Burden Shake")]
    public void ShakeBurden()
    {
        TriggerShake(0.4f, 15f); // Shake 15 pixels
    }

    [Button("Test Game Over Shake")]
    public void ShakeGameOver()
    {
        TriggerShake(1.5f, 40f); // Shake 40 pixels!
    }

    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        dampingSpeed = 1.0f;
    }
}