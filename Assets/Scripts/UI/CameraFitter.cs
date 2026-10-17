using UnityEngine;

/// <summary>
/// Fits the whole board on screen, on any screen size and shape.
/// Put this on the Main Camera (it must be Orthographic).
///
/// How it works:
///  1) Measures the board from its sprite.
///  2) Reserves a strip at the top and bottom of the screen for the HUD.
///  3) Chooses the camera zoom so the board fits BOTH the width and the height.
///  4) Moves the camera so the board sits in the middle of the free area.
///  5) Runs again whenever the screen size changes (rotation, window resize).
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFitter : MonoBehaviour
{
    [Header("Board")]
    [Tooltip("Drag UI_Board_0 (the board sprite) here.")]
    [SerializeField] private SpriteRenderer boardSprite;

    [Tooltip("Used only if Board Sprite is empty. World size of the board (width = height).")]
    [SerializeField] private float fallbackBoardSize = 10f;

    [Header("Space kept free for the HUD (fraction of screen height)")]
    [Range(0f, 0.4f)] [SerializeField] private float topUiFraction = 0.12f;
    [Range(0f, 0.4f)] [SerializeField] private float bottomUiFraction = 0.10f;

    [Header("Extra breathing room around the board")]
    [Range(0f, 0.2f)] [SerializeField] private float padding = 0.04f;

    [Header("Notches")]
    [Tooltip("Also keep the board out of notches and rounded corners.")]
    [SerializeField] private bool useSafeArea = true;

    private Camera cam;
    private int lastWidth;
    private int lastHeight;
    private Rect lastSafeArea;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    private void OnEnable()
    {
        Fit();
    }

    private void LateUpdate()
    {
        // Only re-fit when the screen really changed.
        if (Screen.width != lastWidth || Screen.height != lastHeight || Screen.safeArea != lastSafeArea)
            Fit();
    }

    private void Fit()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        lastSafeArea = Screen.safeArea;
        if (lastWidth <= 0 || lastHeight <= 0) return;

        // --- 1) Board size and center in world units ---
        Vector3 center;
        float halfW, halfH;
        if (boardSprite != null)
        {
            Bounds b = boardSprite.bounds;
            center = b.center;
            halfW = b.extents.x;
            halfH = b.extents.y;
        }
        else
        {
            center = Vector3.zero;
            halfW = halfH = fallbackBoardSize * 0.5f;
        }
        halfW *= 1f + padding;
        halfH *= 1f + padding;

        // --- 2) Free area of the screen (fractions, 0 = bottom, 1 = top) ---
        float top = topUiFraction;
        float bottom = bottomUiFraction;
        float left = 0f;
        float right = 0f;

        if (useSafeArea)
        {
            Rect safe = Screen.safeArea;
            top += (Screen.height - safe.yMax) / Screen.height;
            bottom += safe.yMin / Screen.height;
            left += safe.xMin / Screen.width;
            right += (Screen.width - safe.xMax) / Screen.width;
        }

        float freeHeight = Mathf.Max(0.2f, 1f - top - bottom);
        float freeWidth = Mathf.Max(0.2f, 1f - left - right);
        float aspect = (float)lastWidth / lastHeight;

        // --- 3) Zoom so the board fits the free height AND the free width ---
        float sizeForHeight = halfH / freeHeight;
        float sizeForWidth = halfW / (freeWidth * aspect);
        float size = Mathf.Max(sizeForHeight, sizeForWidth);
        cam.orthographicSize = size;

        // --- 4) Move the camera so the board is centered in the free area ---
        // Viewport 0..1 maps to world: worldY = camY + (v - 0.5) * 2 * size
        float freeCenterY = bottom + freeHeight * 0.5f;
        float freeCenterX = left + freeWidth * 0.5f;

        float camY = center.y - (freeCenterY - 0.5f) * 2f * size;
        float camX = center.x - (freeCenterX - 0.5f) * 2f * size * aspect;

        transform.position = new Vector3(camX, camY, transform.position.z);
    }
}