using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Put this on a popup panel (e.g. HowToPlayPanel).
///  - Open()  : shows the panel with a small pop-in animation.
///  - Close() : hides the panel, which reveals the main menu behind it.
///  - Pressing Esc (PC) or the Android Back button also closes it.
/// Hook Open() to the How To Play button and Close() to the Back button.
/// Works with both the old and the new Input System.
/// </summary>
public class PanelController : MonoBehaviour
{
    [Tooltip("The object that pops in (usually ParchmentFrame). Leave empty to skip the animation.")]
    [SerializeField] private RectTransform popTarget;

    [Tooltip("Seconds the pop-in takes.")]
    [SerializeField] private float popDuration = 0.25f;

    [Tooltip("Scale the pop-in starts from.")]
    [SerializeField] private float startScale = 0.8f;

    /// <summary>Call from the How To Play button's On Click.</summary>
    public void Open()
    {
        gameObject.SetActive(true); // OnEnable will play the pop-in
    }

    /// <summary>Call from the Back button's On Click.</summary>
    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (popTarget != null)
            StartCoroutine(PopIn());
    }

    private void Update()
    {
        // Esc on PC, and the Android Back button (Unity reports it as Escape).
        if (EscapePressed())
            Close();
    }

    private IEnumerator PopIn()
    {
        float t = 0f;
        popTarget.localScale = Vector3.one * startScale;

        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / popDuration);
            // Ease out, so it slows down as it arrives.
            float eased = 1f - (1f - p) * (1f - p);
            popTarget.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, eased);
            yield return null;
        }
        popTarget.localScale = Vector3.one;
    }

    private static bool EscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}