using TMPro;
using UnityEngine;

/// <summary>
/// Animates a TextMeshPro label like "Loading." -> "Loading.." -> "Loading...".
/// The dots that are not shown yet are kept as INVISIBLE characters, so the
/// text stays centered and does not jump left and right.
/// Put this on the LoadingText object (it must have a TextMeshPro - Text (UI)).
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class LoadingDots : MonoBehaviour
{
    [Tooltip("The words before the dots.")]
    [SerializeField] private string baseText = "Loading";

    [Tooltip("How many dots at most.")]
    [SerializeField] private int maxDots = 3;

    [Tooltip("Seconds between each new dot.")]
    [SerializeField] private float interval = 0.4f;

    private TMP_Text label;
    private float timer;
    private int dotCount;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        timer = 0f;
        dotCount = 0;
        Apply();
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer < interval) return;

        timer -= interval;
        dotCount = (dotCount + 1) % (maxDots + 1); // 0,1,2,3,0,1,...
        Apply();
    }

    private void Apply()
    {
        string visible = new string('.', dotCount);
        string hidden = new string('.', maxDots - dotCount);

        // <alpha=#00> makes the following characters fully transparent.
        label.text = baseText + visible + "<alpha=#00>" + hidden;
    }
}