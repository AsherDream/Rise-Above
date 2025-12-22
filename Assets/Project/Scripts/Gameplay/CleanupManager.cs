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

    // REMOVED: public Texture2D milkTexture;
    // REMOVED: public Texture2D eggTexture;

    [Tooltip("The shape of the eraser. White pixels = erase.")]
    [Required] public Texture2D dirtBrush;

    [Title("Eraser Settings")]
    public int brushSize = 50;
    public float brushScale = 1.0f;

    [Title("Audio")]
    public AudioSource audioSource;
    public AudioClip scrubSound;

    // REMOVED: public AudioClip milkSplatSound;
    // REMOVED: public AudioClip eggSplatSound;

    [Title("Cursor Settings")]
    public RectTransform cleaningToolCursor;

    private Texture2D dirtMaskTexture;
    private Color32[] texturePixels;
    private byte[] brushAlphaPixels;
    private int cachedBrushWidth;
    private int cachedBrushHeight;

    private bool isCleaning = false;
    private float dirtAmountTotal;
    private float dirtAmount;
    private Camera uiCamera;

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

        CacheBrushPixels();

        if (splatterCanvasGroup != null)
        {
            splatterCanvasGroup.alpha = 0f;
            splatterCanvasGroup.blocksRaycasts = false;
            splatterCanvasGroup.gameObject.SetActive(false);
        }

        if (cleaningToolCursor != null) cleaningToolCursor.gameObject.SetActive(false);

        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = rootCanvas.worldCamera;
        }
    }

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

        // --- 1. HANDLE CURSOR MOVEMENT ---
        Vector2 localPoint;
        bool isMouseInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(splatterRect, Input.mousePosition, uiCamera, out localPoint);

        if (cleaningToolCursor != null && isMouseInside)
        {
            cleaningToolCursor.anchoredPosition = localPoint;
        }

        // --- 2. HANDLE SCRUBBING ---
        if (Input.GetMouseButton(0) && isMouseInside)
        {
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
                dirtMaskTexture.SetPixels32(texturePixels);
                dirtMaskTexture.Apply(false);

                if (audioSource != null && scrubSound != null && !audioSource.isPlaying)
                    audioSource.PlayOneShot(scrubSound);

                CheckCompletion();
            }

            lastPixelPos = new Vector2Int(pixelX, pixelY);
            hasLastPos = true;
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

        int endX = startX + brushW;
        int endY = startY + brushH;

        int texWidth = dirtMaskTexture.width;
        int texHeight = dirtMaskTexture.height;

        int loopStartX = Mathf.Max(0, startX);
        int loopStartY = Mathf.Max(0, startY);
        int loopEndX = Mathf.Min(texWidth, endX);
        int loopEndY = Mathf.Min(texHeight, endY);

        bool didClean = false;

        for (int y = loopStartY; y < loopEndY; y++)
        {
            int targetRowIndex = y * texWidth;
            int brushY = (int)(((y - startY) / (float)brushH) * cachedBrushHeight);
            int brushRowIndex = brushY * cachedBrushWidth;

            for (int x = loopStartX; x < loopEndX; x++)
            {
                int brushX = (int)(((x - startX) / (float)brushW) * cachedBrushWidth);
                byte alpha = brushAlphaPixels[brushRowIndex + brushX];

                if (alpha > 25)
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

    // --- UPDATED TRIGGER METHOD ---
    [Button("Test Splatter")]
    public void TriggerMess(InspectableItemData itemData)
    {
        if (splatterCanvasGroup == null || splatterRawImage == null) return;
        if (itemData == null) return;

        // 1. Check if this item actually has a mess texture
        if (itemData.messTexture == null)
        {
            // This item is "Wasteful" but doesn't cause a mess (like Earphones or Candy)
            return;
        }

        // 2. Use the data directly!
        Texture2D sourceTex = itemData.messTexture;
        AudioClip soundToPlay = itemData.messSound;

        if (dirtMaskTexture != null) Destroy(dirtMaskTexture);

        dirtMaskTexture = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.RGBA32, false);

        texturePixels = sourceTex.GetPixels32();
        dirtMaskTexture.SetPixels32(texturePixels);
        dirtMaskTexture.Apply(false);

        splatterRawImage.texture = dirtMaskTexture;

        dirtAmount = 0;
        for (int i = 0; i < texturePixels.Length; i++)
        {
            if (texturePixels[i].a > 10) dirtAmount++;
        }
        dirtAmountTotal = dirtAmount;

        splatterCanvasGroup.gameObject.SetActive(true);
        splatterCanvasGroup.alpha = 1f;
        splatterCanvasGroup.blocksRaycasts = true;

        isCleaning = true;
        hasLastPos = false;
        Cursor.visible = false;
        if (cleaningToolCursor != null) cleaningToolCursor.gameObject.SetActive(true);

        if (audioSource != null && soundToPlay != null) audioSource.PlayOneShot(soundToPlay);
        if (DialogueManager.Instance != null) DialogueManager.Instance.ForcePortrait(SisterMood.Complain);
    }

    private void CheckCompletion()
    {
        if (dirtAmountTotal <= 0) return;

        float percentageRemaining = dirtAmount / dirtAmountTotal;

        if (percentageRemaining < 0.05f)
        {
            isCleaning = false;
            Cursor.visible = true;
            if (cleaningToolCursor != null) cleaningToolCursor.gameObject.SetActive(false);

            splatterCanvasGroup.alpha = 0f;
            splatterCanvasGroup.blocksRaycasts = false;
            splatterCanvasGroup.gameObject.SetActive(false);

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