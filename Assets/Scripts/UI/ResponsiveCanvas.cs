using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Keeps the UI correct whenever the screen size or shape changes
/// (resizing a Windows window, rotating a phone, a browser window changing...).
///
/// What it does every time Screen.width / Screen.height change:
///  1) Picks the best Canvas Scaler "Match" value for the current aspect ratio,
///     so the whole 1080x1920 design always fits on screen, in any shape.
///  2) Shows the objects listed under "Portrait Only" or "Landscape Only",
///     depending on whether the screen is taller than wide.
///  3) Fires OnLayoutChanged, so other scripts can react.
///
/// Put this on the Canvas (the object that has the Canvas Scaler).
/// </summary>
[RequireComponent(typeof(CanvasScaler))]
public class ResponsiveCanvas : MonoBehaviour
{
    [Header("Layout groups (optional)")]
    [Tooltip("Objects that should only be visible when the screen is portrait (taller than wide).")]
    [SerializeField] private GameObject[] portraitOnly;

    [Tooltip("Objects that should only be visible when the screen is landscape (wider than tall).")]
    [SerializeField] private GameObject[] landscapeOnly;

    [Header("Events")]
    [Tooltip("Fires when the screen size changes. Passes true if the screen is now landscape.")]
    public UnityEvent<bool> OnLayoutChanged;

    private CanvasScaler scaler;
    private int lastWidth;
    private int lastHeight;

    /// <summary>True when the screen is currently wider than tall.</summary>
    public bool IsLandscape { get; private set; }

    private void Awake()
    {
        scaler = GetComponent<CanvasScaler>();
    }

    private void OnEnable()
    {
        Apply(true);
    }

    private void Update()
    {
        // Cheap check every frame: only does work when the size really changed.
        if (Screen.width != lastWidth || Screen.height != lastHeight)
            Apply(false);
    }

    private void Apply(bool force)
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        if (lastWidth <= 0 || lastHeight <= 0) return;

        float screenAspect = (float)lastWidth / lastHeight;

        // --- 1) Canvas Scaler match -------------------------------------
        // The Canvas Scaler must be "Scale With Screen Size".
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        float referenceAspect = scaler.referenceResolution.x / scaler.referenceResolution.y;

        // Screen narrower than the design -> match WIDTH (0) so nothing is cut off sideways.
        // Screen wider than the design   -> match HEIGHT (1) so nothing is cut off top/bottom.
        // Result: the whole reference rectangle always fits inside the screen.
        scaler.matchWidthOrHeight = screenAspect < referenceAspect ? 0f : 1f;

        // --- 2) Portrait / landscape groups ------------------------------
        bool landscape = lastWidth > lastHeight;
        bool changed = force || landscape != IsLandscape;
        IsLandscape = landscape;

        if (changed)
        {
            SetActive(portraitOnly, !landscape);
            SetActive(landscapeOnly, landscape);
        }

        // --- 3) Tell everyone ---------------------------------------------
        OnLayoutChanged?.Invoke(landscape);
    }

    private static void SetActive(GameObject[] objects, bool active)
    {
        if (objects == null) return;
        foreach (GameObject go in objects)
            if (go != null) go.SetActive(active);
    }
}