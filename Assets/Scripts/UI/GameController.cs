using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// ============================================================
// GameController.cs - connects INPUT -> RULES -> VIEW -> HUD.
// Put on a new empty object called "GameController".
// Local hot-seat for now: both sides play on the same screen.
// Put in Assets/Scripts/Core (or UI).
// ============================================================
public class GameController : MonoBehaviour
{
    [Header("References")]
    public BoardView view;
    public GameHUD hud;
    public Camera cam;

    [Header("Input")]
    // How close (world units) a click must be to a point to count.
    // Spacing is 2, so ~0.9 covers almost the whole board with no gaps.
    public float tapRadius = 0.9f;

    private BagchalGame game;
    private int selected = -1;                       // selected piece, or -1
    private List<int> destinations = new List<int>(); // its legal destinations

    void Start()
    {
        if (cam == null) cam = Camera.main;
        game = new BagchalGame();
        view.Bind(game);
        NewGame();
    }

    // Also hook this up to the Restart buttons (Button -> OnClick).
    public void NewGame()
    {
        game.NewGame();
        ClearSelection();
        view.Refresh();
        hud.HideGameOver();
        hud.ShowMessage("");
        hud.UpdateHUD(game);
    }

    void Update()
    {
        if (game == null || game.Result != GameResult.Ongoing) return;

        Vector2 screenPos;
        if (!TryGetPress(out screenPos)) return;

        // Ignore presses that land on a UI button/panel.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // Screen pixels -> world position -> nearest board point.
        Vector3 world = cam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z)));
        HandleClick(view.WorldToIndex(world, tapRadius));
    }

    // ------------------------------------------------------------
    // The click flow
    // ------------------------------------------------------------
    private void HandleClick(int index)
    {
        if (index < 0) { ClearSelection(); return; }

        // GOATS PLACING: a click on a point tries to place a goat.
        if (game.Turn == Side.Goat && game.CurrentPhase == Phase.Placing)
        {
            MoveResult r = game.PlaceGoat(index);
            if (r.Success) AfterAction();
            else hud.ShowMessage(Describe(r.Error));
            return;
        }

        // A piece is selected and the click is on one of its destinations: move.
        if (selected != -1 && destinations.Contains(index))
        {
            MoveResult r = game.MovePiece(selected, index);
            ClearSelection();
            if (r.Success) AfterAction();
            else hud.ShowMessage(Describe(r.Error));
            return;
        }

        // Otherwise: try to select the clicked piece.
        List<int> dests = game.GetLegalDestinations(index);
        if (dests.Count > 0)
        {
            Select(index, dests);
            hud.ShowMessage("");
            return;
        }

        ClearSelection();
        PointState s = game.Board.GetState(index);
        if (s == PointState.Empty) return;

        bool mine = (s == PointState.Tiger && game.Turn == Side.Tiger) ||
                    (s == PointState.Goat && game.Turn == Side.Goat);
        hud.ShowMessage(mine ? "That piece has no legal moves."
                             : "It's the " + (game.Turn == Side.Goat ? "Goats'" : "Tigers'") + " turn.");
    }

    private void Select(int index, List<int> dests)
    {
        ClearSelection();
        selected = index;
        destinations = dests;

        view.ShowSelection(index);
        foreach (int d in dests)
            view.ShowDestination(d, game.IsJump(index, d));
    }

    private void ClearSelection()
    {
        selected = -1;
        destinations = new List<int>();
        view.ClearHighlights();
    }

    // Runs after every successful action.
    private void AfterAction()
    {
        ClearSelection();
        view.Refresh();
        hud.ShowMessage("");
        hud.UpdateHUD(game);

        if (game.Result == GameResult.TigersWin) hud.ShowGameOver("Tigers win!");
        else if (game.Result == GameResult.GoatsWin) hud.ShowGameOver("Goats win!");
    }

    private static string Describe(MoveError e)
    {
        switch (e)
        {
            case MoveError.GameOver: return "The game is over.";
            case MoveError.WrongPhase: return "Goats can't move until all 20 are placed.";
            case MoveError.NotYourTurn: return "It's not your turn.";
            case MoveError.NotYourPiece: return "That's not your piece.";
            case MoveError.DestinationOccupied: return "That point is occupied.";
            case MoveError.IllegalMove: return "That move isn't allowed.";
            default: return "";
        }
    }

    // One press for mouse AND touch, on either input system.
    private bool TryGetPress(out Vector2 screenPos)
    {
#if ENABLE_INPUT_SYSTEM
        Pointer p = Pointer.current;
        if (p != null && p.press.wasPressedThisFrame)
        {
            screenPos = p.position.ReadValue();
            return true;
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            screenPos = Input.mousePosition;
            return true;
        }
#endif
        screenPos = Vector2.zero;
        return false;
    }
}