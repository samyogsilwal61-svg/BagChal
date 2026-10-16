using UnityEngine;

// ============================================================
// SafeArea.cs - keeps UI out of phone notches / rounded corners.
// Put on a full-stretch panel directly under your Canvas, then
// put all HUD elements inside that panel.
// Put in Assets/Scripts/UI.
// ============================================================
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform rt;
    private Rect lastSafe;
    private int lastW, lastH;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        Apply();
    }

    void Update()
    {
        if (Screen.safeArea != lastSafe || Screen.width != lastW || Screen.height != lastH)
            Apply();
    }

    private void Apply()
    {
        Rect safe = Screen.safeArea;
        lastSafe = safe;
        lastW = Screen.width;
        lastH = Screen.height;
        if (lastW <= 0 || lastH <= 0) return;

        // Convert pixel rect to 0..1 anchors.
        Vector2 min = safe.position;
        Vector2 max = safe.position + safe.size;
        min.x /= lastW; min.y /= lastH;
        max.x /= lastW; max.y /= lastH;

        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}