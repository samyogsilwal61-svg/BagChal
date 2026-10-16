using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Boot scene controller:
///  1) Fades in from black.
///  2) Starts loading the next scene in the background (not activated yet).
///  3) Waits until the scene is ready AND the minimum show time has passed,
///     so the logo animation is always seen.
///  4) Fades to black, then switches scene.
/// Put this on an empty GameObject (e.g. "LoadingScreen") in the Boot scene.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [Header("Scene to open")]
    [Tooltip("Must match the scene name EXACTLY and be in Build Settings.")]
    [SerializeField] private string nextSceneName = "GameScene";

    [Header("Timing")]
    [Tooltip("Minimum seconds the loading screen stays visible.")]
    [SerializeField] private float minimumShowTime = 2.5f;
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 0.4f;

    [Header("Fade overlay (full-screen black Image, last child of Canvas)")]
    [SerializeField] private Image fadeOverlay;

    private void Start()
    {
        // Make sure the overlay never blocks taps and starts fully black.
        if (fadeOverlay != null)
        {
            fadeOverlay.raycastTarget = false;
            SetFade(1f);
        }

        StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        // 1) Fade in (black -> clear).
        yield return Fade(1f, 0f, fadeInTime);

        // 2) Start loading in the background, but do not switch yet.
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
        if (op == null)
        {
            Debug.LogError("LoadingScreen: scene '" + nextSceneName +
                           "' was not found. Check the name and Build Settings.");
            yield break;
        }
        op.allowSceneActivation = false;

        // 3) Wait for min time AND for loading to reach 0.9 (Unity's "ready").
        float elapsed = 0f;
        while (elapsed < minimumShowTime || op.progress < 0.9f)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // 4) Fade out, then activate the new scene.
        yield return Fade(0f, 1f, fadeOutTime);
        op.allowSceneActivation = true;
    }

    /// <summary>Animates the overlay alpha from 'from' to 'to'.</summary>
    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeOverlay == null || duration <= 0f)
        {
            SetFade(to);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetFade(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetFade(to);
    }

    private void SetFade(float alpha)
    {
        if (fadeOverlay == null) return;
        Color c = fadeOverlay.color;
        c.a = alpha;
        fadeOverlay.color = c;
    }
}