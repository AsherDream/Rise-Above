using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class CleanupManager : MonoBehaviour
{
    public static CleanupManager Instance;

    [Title("Visuals")]
    [Required] public RawImage splatterRawImage; // Changed to RawImage
    [Required] public CanvasGroup splatterCanvasGroup;
    [Required] public RectTransform splatterRect; // For coordinate math

    [Title("Splatter Textures (Must be Read/Write Enabled!)")]
    public Texture2D milkTexture;
    public Texture2D eggTexture;

    [Title("Eraser Settings")]
    public int brushSize = 50; // Size of the eraser hole
    public float scrubDifficulty = 3000f; // Total pixel distance to scrub
    public float scrubSensitivity = 1f;

    [Title("Audio")]
    public AudioSource audioSource;
    public AudioClip splatSound;
    public AudioClip scrubSound;

    private bool isCleaning = false;
    private float currentCleanProgress = 0f;
    private Vector3 lastMousePos;
    private Texture2D activeTexture; // The texture we are modifying

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (splatterCanvasGroup != null)
        {
            splatterCanvasGroup.alpha = 0f;
            splatterCanvasGroup.blocksRaycasts = false;
            splatterCanvasGroup.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isCleaning) return;

        if (Input.GetMouseButton(0))
        {
            Vector3 currentMousePos = Input.mousePosition;
            float dist = Vector3.Distance(currentMousePos, lastMousePos);

            // Only erase if we moved enough
            if (dist > 1f)
            {
                // 1. Update Win Progress
                currentCleanProgress += dist * scrubSensitivity;

                // 2. Perform Visual Erase
                EraseAt(currentMousePos);

                // 3. Audio
                if (audioSource != null && scrubSound != null && !audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(scrubSound);
                }
            }
            lastMousePos = currentMousePos;
        }
        else
        {
            lastMousePos = Input.mousePosition;
        }

        CheckCompletion();
    }

    [Button("Test Splatter")]
    public void TriggerMess(string messType = "milk")
    {
        if (splatterCanvasGroup == null || splatterRawImage == null) return;

        // 1. Pick Texture
        Texture2D sourceTex = milkTexture; // Default
        if (messType.ToLower().Contains("egg")) sourceTex = eggTexture;

        if (sourceTex == null)
        {
            Debug.LogError("[CleanupManager] Texture is missing! Assign Milk/Egg textures.");
            return;
        }

        // 2. Create Writable Copy (So we don't ruin the original asset)
        // We create a new texture with RGBA32 format to support transparency
        activeTexture = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.RGBA32, false);
        activeTexture.SetPixels32(sourceTex.GetPixels32());
        activeTexture.Apply();

        splatterRawImage.texture = activeTexture;

        // 3. Show UI
        splatterCanvasGroup.gameObject.SetActive(true);
        splatterCanvasGroup.alpha = 1f;
        splatterCanvasGroup.blocksRaycasts = true;

        // 4. Reset State
        isCleaning = true;
        currentCleanProgress = 0f;
        lastMousePos = Input.mousePosition;

        if (audioSource != null && splatSound != null)
            audioSource.PlayOneShot(splatSound);

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ForcePortrait(SisterMood.Complain);
    }

    private void EraseAt(Vector2 screenPos)
    {
        if (activeTexture == null) return;

        // Convert screen point to local point in the UI rect
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(splatterRect, screenPos, null, out localPoint))
        {
            // Normalize coordinates (0 to 1)
            float normX = Mathf.InverseLerp(splatterRect.rect.xMin, splatterRect.rect.xMax, localPoint.x);
            float normY = Mathf.InverseLerp(splatterRect.rect.yMin, splatterRect.rect.yMax, localPoint.y);

            // Convert to pixel coordinates
            int px = Mathf.FloorToInt(normX * activeTexture.width);
            int py = Mathf.FloorToInt(normY * activeTexture.height);

            // Apply Eraser (Circle Shape)
            Color32[] pixels = activeTexture.GetPixels32();
            int width = activeTexture.width;
            int height = activeTexture.height;
            bool dirty = false;

            for (int y = -brushSize; y <= brushSize; y++)
            {
                for (int x = -brushSize; x <= brushSize; x++)
                {
                    if (x * x + y * y <= brushSize * brushSize) // Circle check
                    {
                        int targetX = px + x;
                        int targetY = py + y;
                        if (targetX >= 0 && targetX < width && targetY >= 0 && targetY < height)
                        {
                            // Set pixel to Transparent (Eraser)
                            pixels[targetY * width + targetX] = new Color32(0, 0, 0, 0);
                            dirty = true;
                        }
                    }
                }
            }

            if (dirty)
            {
                activeTexture.SetPixels32(pixels);
                activeTexture.Apply(); // Upload changes to GPU
            }
        }
    }

    private void CheckCompletion()
    {
        if (currentCleanProgress >= scrubDifficulty)
        {
            isCleaning = false;

            splatterCanvasGroup.alpha = 0f;
            splatterCanvasGroup.blocksRaycasts = false;
            splatterCanvasGroup.gameObject.SetActive(false);

            if (DialogueManager.Instance != null)
                DialogueManager.Instance.ForcePortrait(SisterMood.Normal);

            Debug.Log("Cleanup Complete!");
        }
    }
}