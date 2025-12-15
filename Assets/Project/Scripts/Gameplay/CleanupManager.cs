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

    [Title("Audio")]
    public AudioSource audioSource;
    public AudioClip splatSound;
    public AudioClip scrubSound;

    [Title("Debug")]
    public RectTransform debugCursor;

    private Texture2D dirtMaskTexture;
    private Color32[] texturePixels; // The target texture pixels
    private byte[] brushAlphaPixels; // OPTIMIZATION: Cache brush alpha as bytes
    private int cachedBrushWidth;
    private int cachedBrushHeight;

    private bool isCleaning = false;
    private float dirtAmountTotal;
    private float dirtAmount;
    private Camera uiCamera;

    // Track previous positions
    private Vector2Int lastPixelPos;
    private bool hasLastPos = false;

    public static bool IsMessActive
    {
        get { return Instance != null && Instance.isCleaning; }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Pre-cache the brush texture to avoid GetPixel in Update loop
        CacheBrushPixels();

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

    // Optimization: Cache brush pixels into a simple byte array (0-255)
    private void CacheBrushPixels()
    {
        if (dirtBrush == null) return;

        cachedBrushWidth = dirtBrush.width;
        cachedBrushHeight = dirtBrush.height;
        Color32[] rawBrushPixels = dirtBrush.GetPixels32();
        brushAlphaPixels = new byte[rawBrushPixels.Length];

        for (int i = 0; i < rawBrushPixels.Length; i++)
        {
            brushAlphaPixels[i] = rawBrushPixels[i].a;
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

                bool pixelsChanged = false;

                if (hasLastPos)
                {
                    if (PaintLine(lastPixelPos.x, lastPixelPos.y, pixelX, pixelY))
                        pixelsChanged = true;
                }
                else
                {
                    if (Paint(pixelX, pixelY))
                        pixelsChanged = true;
                }

                if (pixelsChanged)
                {
                    // Update the texture
                    dirtMaskTexture.SetPixels32(texturePixels);
                    // OPTIMIZATION: Apply(false) prevents mipmap generation (slow and unneeded for UI)
                    dirtMaskTexture.Apply(false);

                    if (audioSource != null && scrubSound != null && !audioSource.isPlaying)
                        audioSource.PlayOneShot(scrubSound);

                    CheckCompletion();
                }

                lastPixelPos = new Vector2Int(pixelX, pixelY);
                hasLastPos = true;
            }
        }
        else
        {
            hasLastPos = false;
        }
    }

    private bool PaintLine(int x0, int y0, int x1, int y1)
    {
        bool changed = false;
        float distance = Vector2.Distance(new Vector2(x0, y0), new Vector2(x1, y1));
        // Optimization: Don't step perfectly every pixel, step by 25% of brush size is enough coverage
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
        int brushW = Mathf.RoundToInt(cachedBrushWidth * brushScale);
        int brushH = Mathf.RoundToInt(cachedBrushHeight * brushScale);

        int startX = pixelX - (brushW / 2);
        int startY = pixelY - (brushH / 2);

        // OPTIMIZATION: Calculate loop bounds beforehand to avoid "if" checks inside the loop
        int endX = startX + brushW;
        int endY = startY + brushH;

        int texWidth = dirtMaskTexture.width;
        int texHeight = dirtMaskTexture.height;

        // Clamp to texture boundaries
        int loopStartX = Mathf.Max(0, startX);
        int loopStartY = Mathf.Max(0, startY);
        int loopEndX = Mathf.Min(texWidth, endX);
        int loopEndY = Mathf.Min(texHeight, endY);

        bool didClean = false;

        for (int y = loopStartY; y < loopEndY; y++)
        {
            // Pre-calculate row index for the target texture
            int targetRowIndex = y * texWidth;

            // Map 'y' back to brush coordinate space
            int brushY = (int)(((y - startY) / (float)brushH) * cachedBrushHeight);
            int brushRowIndex = brushY * cachedBrushWidth;

            for (int x = loopStartX; x < loopEndX; x++)
            {
                // Map 'x' back to brush coordinate space
                int brushX = (int)(((x - startX) / (float)brushW) * cachedBrushWidth);

                // Get cached alpha (No GetPixel calls!)
                byte alpha = brushAlphaPixels[brushRowIndex + brushX];

                if (alpha > 25) // Threshold
                {
                    int index = targetRowIndex + x;
                    if (texturePixels[index].a != 0)
                    {
                        texturePixels[index].a = 0;
                        dirtAmount--;
                        didClean = true;
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

        // OPTIMIZATION: Prevent Memory Leak
        // If we generated a texture before, destroy it to free VRAM
        if (dirtMaskTexture != null) Destroy(dirtMaskTexture);

        // Create new texture
        dirtMaskTexture = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.RGBA32, false);

        // Copy pixels
        texturePixels = sourceTex.GetPixels32();
        dirtMaskTexture.SetPixels32(texturePixels);
        dirtMaskTexture.Apply(false); // No mipmaps

        splatterRawImage.texture = dirtMaskTexture;

        // Calculate dirt amount
        dirtAmount = 0;
        // Fast iteration
        for (int i = 0; i < texturePixels.Length; i++)
        {
            if (texturePixels[i].a > 10) dirtAmount++;
        }
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

        float percentageRemaining = dirtAmount / dirtAmountTotal;

        if (percentageRemaining < 0.05f)
        {
            isCleaning = false;
            splatterCanvasGroup.alpha = 0f;
            splatterCanvasGroup.blocksRaycasts = false;
            splatterCanvasGroup.gameObject.SetActive(false);

            // Cleanup texture memory when finished
            if (dirtMaskTexture != null)
            {
                Destroy(dirtMaskTexture);
                dirtMaskTexture = null;
            }

            if (DialogueManager.Instance != null) DialogueManager.Instance.ForcePortrait(SisterMood.Normal);
            Debug.Log("Cleanup Complete!");
        }
    }
}