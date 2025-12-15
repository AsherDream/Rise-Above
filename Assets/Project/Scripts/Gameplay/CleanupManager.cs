using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class CleanupManager : MonoBehaviour
{
    public static CleanupManager Instance;

    [Title("Visuals")]
    [Required] public RawImage splatterRawImage;
    [Required] public CanvasGroup splatterCanvasGroup;
    [Required] public RectTransform splatterRect;

    [Title("Textures (Read/Write Enabled!)")]
    public Texture2D milkTexture;
    public Texture2D eggTexture;

    [Tooltip("The shape of the eraser. White pixels = erase.")]
    [Required] public Texture2D dirtBrush;

    [Title("Eraser Settings")]
    public int brushSize = 50;
    [Tooltip("Scale of the brush applied to texture.")]
    public float brushScale = 1.0f;

    // REMOVED: scrubDifficulty and scrubSensitivity as we are using pixel counting now

    [Title("Audio")]
    public AudioSource audioSource;
    public AudioClip splatSound;
    public AudioClip scrubSound;

    [Title("Debug")]
    public RectTransform debugCursor;

    private Texture2D dirtMaskTexture;
    private Color32[] texturePixels; // Cached pixel array for speed
    private bool isCleaning = false;
    private float dirtAmountTotal;
    private float dirtAmount;
    private Camera uiCamera;

    // Track previous positions
    private Vector2Int lastPixelPos;
    private bool hasLastPos = false;

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

        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = rootCanvas.worldCamera;
        }
    }

    private void Update()
    {
        if (!isCleaning) return;

        if (Input.GetMouseButton(0))
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(splatterRect, Input.mousePosition, uiCamera, out localPoint))
            {
                if (debugCursor != null) debugCursor.anchoredPosition = localPoint;

                float normalizedX = (localPoint.x / splatterRect.rect.width) + 0.5f;
                float normalizedY = (localPoint.y / splatterRect.rect.height) + 0.5f;

                int pixelX = (int)(normalizedX * dirtMaskTexture.width);
                int pixelY = (int)(normalizedY * dirtMaskTexture.height);

                // --- OPTIMIZED SMOOTH ERASING ---
                bool pixelsChanged = false;

                if (hasLastPos)
                {
                    // Draw line on the cached array
                    if (PaintLine(lastPixelPos.x, lastPixelPos.y, pixelX, pixelY))
                        pixelsChanged = true;
                }
                else
                {
                    // Draw dot on the cached array
                    if (Paint(pixelX, pixelY))
                        pixelsChanged = true;
                }

                // ONLY Apply to GPU once per frame!
                if (pixelsChanged)
                {
                    dirtMaskTexture.SetPixels32(texturePixels);
                    dirtMaskTexture.Apply();

                    if (audioSource != null && scrubSound != null && !audioSource.isPlaying)
                        audioSource.PlayOneShot(scrubSound);
                }

                // Update Positions
                lastPixelPos = new Vector2Int(pixelX, pixelY);
                hasLastPos = true;
            }
        }
        else
        {
            hasLastPos = false;
        }

        CheckCompletion();
    }

    private bool PaintLine(int x0, int y0, int x1, int y1)
    {
        bool changed = false;
        float distance = Vector2.Distance(new Vector2(x0, y0), new Vector2(x1, y1));
        float stepSize = Mathf.Max(1f, brushSize * brushScale * 0.25f);
        int steps = Mathf.CeilToInt(distance / stepSize);

        for (int i = 0; i <= steps; i++)
        {
            float t = (steps == 0) ? 0 : (float)i / steps;
            int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, t));

            if (Paint(x, y)) changed = true;
        }
        return changed;
    }

    private bool Paint(int pixelX, int pixelY)
    {
        if (pixelX < 0 || pixelX >= dirtMaskTexture.width || pixelY < 0 || pixelY >= dirtMaskTexture.height) return false;

        int brushW = Mathf.RoundToInt(dirtBrush.width * brushScale);
        int brushH = Mathf.RoundToInt(dirtBrush.height * brushScale);
        int startX = pixelX - (brushW / 2);
        int startY = pixelY - (brushH / 2);

        bool didClean = false;
        int texWidth = dirtMaskTexture.width;
        int texHeight = dirtMaskTexture.height;

        for (int x = 0; x < brushW; x++)
        {
            for (int y = 0; y < brushH; y++)
            {
                int targetX = startX + x;
                int targetY = startY + y;

                if (targetX >= 0 && targetX < texWidth && targetY >= 0 && targetY < texHeight)
                {
                    // Map brush alpha
                    int brushUX = (int)((float)x / brushW * dirtBrush.width);
                    int brushUY = (int)((float)y / brushH * dirtBrush.height);

                    // Simple optimization: check brush alpha first
                    if (dirtBrush.GetPixel(brushUX, brushUY).a > 0.1f)
                    {
                        int index = targetY * texWidth + targetX;
                        // Modify the CACHED array directly (Very Fast)
                        if (texturePixels[index].a > 0)
                        {
                            texturePixels[index].a = 0; // Set alpha to 0
                            dirtAmount--;
                            didClean = true;
                        }
                    }
                }
            }
        }
        return didClean;
    }

    [Button("Test Splatter")]
    public void TriggerMess(string messType = "milk")
    {
        if (splatterCanvasGroup == null || splatterRawImage == null) return;

        Texture2D sourceTex = milkTexture;
        if (messType.ToLower().Contains("egg") && eggTexture != null) sourceTex = eggTexture;

        if (sourceTex == null) return;

        // Clone Texture
        dirtMaskTexture = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.RGBA32, false);
        // Cache the pixels immediately!
        texturePixels = sourceTex.GetPixels32();

        dirtMaskTexture.SetPixels32(texturePixels);
        dirtMaskTexture.Apply();

        splatterRawImage.texture = dirtMaskTexture;

        // Calculate dirt
        dirtAmount = 0;
        for (int i = 0; i < texturePixels.Length; i++) { if (texturePixels[i].a > 10) dirtAmount++; }
        dirtAmountTotal = dirtAmount;

        // Show UI
        splatterCanvasGroup.gameObject.SetActive(true);
        splatterCanvasGroup.alpha = 1f;
        splatterCanvasGroup.blocksRaycasts = true;
        isCleaning = true;
        hasLastPos = false;

        if (audioSource != null && splatSound != null) audioSource.PlayOneShot(splatSound);
        if (DialogueManager.Instance != null) DialogueManager.Instance.ForcePortrait(SisterMood.Complain);
    }

    private void CheckCompletion()
    {
        if (dirtAmountTotal <= 0) return;

        // Win by pixel count (Precision)
        float percentageRemaining = dirtAmount / dirtAmountTotal;

        // Debug log to see progress
        // Debug.Log($"Dirt Remaining: {percentageRemaining:P}");

        // Only finish if < 5% remains (meaning 95% is cleaned)
        if (percentageRemaining < 0.05f)
        {
            isCleaning = false;

            splatterCanvasGroup.alpha = 0f;
            splatterCanvasGroup.blocksRaycasts = false;
            splatterCanvasGroup.gameObject.SetActive(false);
            if (DialogueManager.Instance != null) DialogueManager.Instance.ForcePortrait(SisterMood.Normal);
            Debug.Log("Cleanup Complete!");
        }
    }
}