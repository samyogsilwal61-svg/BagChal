using UnityEngine;

/// <summary>
/// Animates a UI logo (RectTransform):
///  1) Pop-in with a little bounce when the scene starts.
///  2) Then loops a gentle up/down float and a soft "breathing" scale.
/// Put this on the Logo object (the one with the Image).
/// Uses unscaled time, so it still works if Time.timeScale is 0.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class LogoAnimator : MonoBehaviour
{
    [Header("Pop-in")]
    [Tooltip("Seconds the pop-in takes.")]
    [SerializeField] private float popDuration = 0.7f;
    [Tooltip("Seconds to wait before the pop-in starts.")]
    [SerializeField] private float popDelay = 0.2f;

    [Header("Float (up and down)")]
    [Tooltip("How far the logo moves up/down, in canvas units.")]
    [SerializeField] private float floatHeight = 18f;
    [Tooltip("Seconds for one full up-and-down cycle.")]
    [SerializeField] private float floatPeriod = 2.5f;

    [Header("Breathing (scale)")]
    [Tooltip("0.04 means the logo grows/shrinks by 4%.")]
    [SerializeField] private float breatheAmount = 0.04f;
    [Tooltip("Seconds for one full breath.")]
    [SerializeField] private float breathePeriod = 3f;

    private RectTransform rt;
    private Vector2 basePosition;   // where you placed the logo in the editor
    private Vector3 baseScale;      // the scale you set in the editor
    private float startTime;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        basePosition = rt.anchoredPosition;
        baseScale = rt.localScale;
    }

    private void OnEnable()
    {
        startTime = Time.unscaledTime;
        rt.localScale = Vector3.zero; // start invisible, then pop in
    }

    private void Update()
    {
        float t = Time.unscaledTime - startTime - popDelay;

        // Waiting for the delay: stay hidden.
        if (t < 0f)
        {
            rt.localScale = Vector3.zero;
            return;
        }

        // Pop-in phase: scale 0 -> 1 with overshoot (bounce).
        float pop = 1f;
        if (t < popDuration)
            pop = EaseOutBack(t / popDuration);

        // Idle loop (only starts to matter once the pop is finished).
        float idleTime = Mathf.Max(0f, t - popDuration);
        float floatOffset = Mathf.Sin(idleTime * Mathf.PI * 2f / floatPeriod) * floatHeight;
        float breathe = 1f + Mathf.Sin(idleTime * Mathf.PI * 2f / breathePeriod) * breatheAmount;

        rt.anchoredPosition = basePosition + new Vector2(0f, floatOffset);
        rt.localScale = baseScale * (pop * breathe);
    }

    /// <summary>Easing curve that overshoots slightly past 1, then settles at 1.</summary>
    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float p = x - 1f;
        return 1f + c3 * p * p * p + c1 * p * p;
    }
}