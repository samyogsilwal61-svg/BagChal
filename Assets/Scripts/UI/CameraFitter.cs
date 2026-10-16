using UnityEngine;

// ============================================================
// CameraFitter.cs - keeps the whole board visible on ANY screen.
// Put on the Main Camera. Re-fits whenever the screen size or
// orientation changes (window resize, phone rotation, WebGL).
// Put in Assets/Scripts/UI.
// ============================================================
[RequireComponent(typeof(Camera))]
public class CameraFitter : MonoBehaviour
{
    [Header("What to fit")]
    // Drag your board sprite (UI_Board_0) here: its size and position
    // are measured automatically. If empty, the values below are used.
    public SpriteRenderer boardSprite;
    public Vector2 contentCenter = Vector2.zero;
    public Vector2 contentSize = new Vector2(11f, 11f);
    public float extraMargin = 0.3f; // world units of breathing room

    [Header("Space reserved for UI (fraction of screen height)")]
    [Range(0f, 0.4f)] public float topUiFraction = 0.12f;
    [Range(0f, 0.4f)] public float bottomUiFraction = 0.08f;

    private Camera cam;
    private int lastW, lastH;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    void Start() { Fit(); }

    void Update()
    {
        if (Screen.width != lastW || Screen.height != lastH) Fit();
    }

    [ContextMenu("Fit Now")]
    public void Fit()
    {
        if (cam == null) cam = GetComponent<Camera>();
        lastW = Screen.width;
        lastH = Screen.height;
        if (lastW <= 0 || lastH <= 0) return;

        Vector2 center = contentCenter;
        Vector2 size = contentSize;
        if (boardSprite != null)
        {
            center = boardSprite.bounds.center;
            size = boardSprite.bounds.size;
        }
        size += Vector2.one * extraMargin * 2f;

        float aspect = (float)lastW / lastH;
        float usable = Mathf.Max(0.1f, 1f - topUiFraction - bottomUiFraction);

        // Orthographic size = HALF the visible height.
        float sizeForHeight = size.y / usable / 2f;   // fit board height in the usable band
        float sizeForWidth = size.x / aspect / 2f;   // fit board width in the screen width
        float orthoSize = Mathf.Max(sizeForHeight, sizeForWidth); // bigger one fits both
        cam.orthographicSize = orthoSize;

        // Shift the camera so the board sits centered in the usable band.
        float offset = (bottomUiFraction - topUiFraction) * 0.5f;
        Vector3 pos = transform.position;
        pos.x = center.x;
        pos.y = center.y - offset * orthoSize * 2f;
        transform.position = pos;
    }
}