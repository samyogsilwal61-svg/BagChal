using System.Collections.Generic;
using UnityEngine;

// ============================================================
// BoardView.cs - DISPLAY ONLY. (Replaces the earlier version.)
// Draws pieces from the game data and shows selection/move markers.
// Put on your "Board" object (position 0,0,0).
// Put in Assets/Scripts/UI.
// ============================================================
public class BoardView : MonoBehaviour
{
    [Header("Piece prefabs")]
    public GameObject tigerPrefab;
    public GameObject goatPrefab;

    [Header("Grid layout (relative to this object)")]
    public Vector2 origin = new Vector2(-4f, 4f); // point 0 (top-left)
    public float spacing = 2f;                    // distance between points

    [Header("Markers (optional - leave empty to auto-generate circles)")]
    public GameObject selectionPrefab;
    public GameObject movePrefab;

    [Header("Marker look")]
    public Color selectionColor = new Color(1f, 0.9f, 0.1f, 0.9f);
    public Color moveColor = new Color(0.2f, 0.9f, 0.3f, 0.75f);
    public Color captureColor = new Color(1f, 0.25f, 0.2f, 0.8f);
    public float selectionDiameter = 1.5f;
    public float moveDiameter = 0.55f;
    public string highlightsLayer = "Highlights";

    [Header("Debug")]
    public bool drawGizmos = true;

    private BagchalGame game;
    private readonly GameObject[] pieceObjects = new GameObject[BagchalBoard.PointCount];
    private readonly List<GameObject> markers = new List<GameObject>();
    private Sprite discSprite;
    private Sprite ringSprite;

    void Awake()
    {
        discSprite = MakeCircleSprite(false);
        ringSprite = MakeCircleSprite(true);
    }

    // Tell the view which game to display.
    public void Bind(BagchalGame g)
    {
        game = g;
    }

    // ------------------------------------------------------------
    // INDEX <-> WORLD
    // ------------------------------------------------------------
    public Vector3 IndexToWorld(int index)
    {
        int row = BagchalBoard.ToRow(index);
        int col = BagchalBoard.ToCol(index);
        float x = origin.x + col * spacing;
        float y = origin.y - row * spacing; // y flip: row 0 is the TOP
        return transform.TransformPoint(new Vector3(x, y, 0f));
    }

    // Nearest point to a world position, or -1 if none within maxDistance.
    public int WorldToIndex(Vector3 world, float maxDistance)
    {
        int best = -1;
        float bestSqr = maxDistance * maxDistance;
        for (int i = 0; i < BagchalBoard.PointCount; i++)
        {
            Vector3 p = IndexToWorld(i);
            float dx = p.x - world.x;
            float dy = p.y - world.y;
            float sqr = dx * dx + dy * dy;
            if (sqr <= bestSqr) { bestSqr = sqr; best = i; }
        }
        return best;
    }

    // ------------------------------------------------------------
    // PIECES
    // ------------------------------------------------------------
    public void Refresh()
    {
        if (game == null) return;

        for (int i = 0; i < BagchalBoard.PointCount; i++)
        {
            RemoveAt(i);
            switch (game.Board.GetState(i))
            {
                case PointState.Tiger: SpawnAt(i, tigerPrefab); break;
                case PointState.Goat: SpawnAt(i, goatPrefab); break;
            }
        }
    }

    private void SpawnAt(int index, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("BoardView: a piece prefab slot is empty.");
            return;
        }
        GameObject piece = Instantiate(prefab, IndexToWorld(index), Quaternion.identity, transform);
        piece.name = prefab.name + "_" + index;
        pieceObjects[index] = piece;
    }

    public void RemoveAt(int index)
    {
        if (pieceObjects[index] != null)
        {
            Destroy(pieceObjects[index]);
            pieceObjects[index] = null;
        }
    }

    // ------------------------------------------------------------
    // MARKERS (selection ring + legal-move dots)
    // ------------------------------------------------------------
    public void ShowSelection(int index)
    {
        markers.Add(CreateMarker(selectionPrefab, ringSprite, index, selectionColor, selectionDiameter));
    }

    public void ShowDestination(int index, bool isCapture)
    {
        Color c = isCapture ? captureColor : moveColor;
        markers.Add(CreateMarker(movePrefab, discSprite, index, c, moveDiameter));
    }

    public void ClearHighlights()
    {
        foreach (GameObject m in markers)
            if (m != null) Destroy(m);
        markers.Clear();
    }

    private GameObject CreateMarker(GameObject prefab, Sprite fallback, int index, Color color, float diameter)
    {
        if (prefab != null)
            return Instantiate(prefab, IndexToWorld(index), Quaternion.identity, transform);

        var go = new GameObject("Marker_" + index);
        go.transform.SetParent(transform, false);
        go.transform.position = IndexToWorld(index);
        go.transform.localScale = Vector3.one * diameter;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = fallback;
        sr.color = color;
        sr.sortingLayerName = highlightsLayer;
        return go;
    }

    // Draws a 1-world-unit circle (filled disc or ring) in code,
    // so you need no extra art for highlights.
    private static Sprite MakeCircleSprite(bool ring)
    {
        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center = (size - 1) / 2f;
        float radius = size / 2f - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                float a = Mathf.Clamp01(radius - d);               // soft outer edge
                if (ring) a = Mathf.Min(a, Mathf.Clamp01(d - (radius - 10f))); // hollow middle
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        // pixelsPerUnit = size  ->  the sprite is exactly 1 world unit wide.
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < BagchalBoard.PointCount; i++)
            Gizmos.DrawWireSphere(IndexToWorld(i), 0.2f);
    }
}