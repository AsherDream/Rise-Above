using UnityEngine;
using Sirenix.OdinInspector;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 originalPos;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.7f;
    private float dampingSpeed = 1.0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        originalPos = transform.localPosition;
    }

    private void Update()
    {
        if (shakeDuration > 0)
        {
            // USE VECTOR2 (insideUnitCircle) to prevent Z-axis movement!
            Vector2 randomShake = Random.insideUnitCircle * shakeMagnitude;

            transform.localPosition = originalPos + new Vector3(randomShake.x, randomShake.y, 0);

            // USE UNSCALED TIME so it works during Game Over/Pause
            shakeDuration -= Time.unscaledDeltaTime * dampingSpeed;
        }
        else
        {
            shakeDuration = 0f;
            transform.localPosition = originalPos;
        }
    }

    [Button("Test Shake")]
    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        dampingSpeed = 1.0f;
    }

    public void ShakeWasteful() { TriggerShake(0.2f, 0.2f); }
    public void ShakeBurden() { TriggerShake(0.4f, 0.5f); }
    public void ShakeGameOver() { TriggerShake(1.5f, 1.0f); }
}