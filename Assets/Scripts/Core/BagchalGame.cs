using System.Collections.Generic;

// ============================================================
// BagchalGame.cs - THE RULES. Plain C# (not a MonoBehaviour).
// Knows nothing about sprites, clicks, or screens.
// It owns a BagchalBoard and decides what is legal.
// Put in Assets/Scripts/Core.
// ============================================================

public enum Side { Goat, Tiger }
public enum Phase { Placing, Moving }
public enum GameResult { Ongoing, TigersWin, GoatsWin }

public enum MoveError
{
    None,
    GameOver,
    WrongPhase,          // e.g. a goat tries to move before all 20 are placed
    NotYourTurn,
    NotYourPiece,
    InvalidIndex,
    DestinationOccupied,
    IllegalMove
}

// What an action returns, so the UI never has to guess.
public struct MoveResult
{
    public bool Success;
    public MoveError Error;
    public int CapturedIndex; // point of the captured goat, or -1

    public static MoveResult Ok(int capturedIndex = -1)
    {
        return new MoveResult { Success = true, Error = MoveError.None, CapturedIndex = capturedIndex };
    }

    public static MoveResult Fail(MoveError error)
    {
        return new MoveResult { Success = false, Error = error, CapturedIndex = -1 };
    }
}

public class BagchalGame
{
    public const int TotalGoats = 20;
    public const int CapturesToWin = 5;

    public BagchalBoard Board { get; private set; }
    public Side Turn { get; private set; }
    public int GoatsPlaced { get; private set; }
    public int GoatsCaptured { get; private set; }
    public GameResult Result { get; private set; }

    // Derived values (always computed, so they can never get out of sync).
    public Phase CurrentPhase { get { return GoatsPlaced < TotalGoats ? Phase.Placing : Phase.Moving; } }
    public int GoatsInHand { get { return TotalGoats - GoatsPlaced; } }
    public int GoatsOnBoard { get { return GoatsPlaced - GoatsCaptured; } }

    public BagchalGame()
    {
        Board = new BagchalBoard();
        NewGame();
    }

    // Reset everything. Goats always move first.
    public void NewGame()
    {
        Board.ResetBoard();
        Turn = Side.Goat;
        GoatsPlaced = 0;
        GoatsCaptured = 0;
        Result = GameResult.Ongoing;
    }

    // ============================================================
    // ACTIONS (each one validates by itself - never trust the caller)
    // ============================================================

    public MoveResult PlaceGoat(int index)
    {
        if (Result != GameResult.Ongoing) return MoveResult.Fail(MoveError.GameOver);
        if (Turn != Side.Goat) return MoveResult.Fail(MoveError.NotYourTurn);
        if (CurrentPhase != Phase.Placing) return MoveResult.Fail(MoveError.WrongPhase);
        if (!IsValidIndex(index)) return MoveResult.Fail(MoveError.InvalidIndex);
        if (Board.GetState(index) != PointState.Empty) return MoveResult.Fail(MoveError.DestinationOccupied);

        Board.SetState(index, PointState.Goat);
        GoatsPlaced++;
        EndTurn();
        return MoveResult.Ok();
    }

    // Moves the piece on 'from' to 'to'. Handles steps AND tiger jumps.
    public MoveResult MovePiece(int from, int to)
    {
        if (Result != GameResult.Ongoing) return MoveResult.Fail(MoveError.GameOver);
        if (!IsValidIndex(from) || !IsValidIndex(to)) return MoveResult.Fail(MoveError.InvalidIndex);

        PointState piece = Board.GetState(from);
        if (piece == PointState.Empty || SideOf(piece) != Turn) return MoveResult.Fail(MoveError.NotYourPiece);
        if (piece == PointState.Goat && CurrentPhase == Phase.Placing) return MoveResult.Fail(MoveError.WrongPhase);
        if (Board.GetState(to) != PointState.Empty) return MoveResult.Fail(MoveError.DestinationOccupied);

        // 1) A normal step to a neighboring point
        if (IsStep(from, to))
        {
            Board.SetState(to, piece);
            Board.SetState(from, PointState.Empty);
            EndTurn();
            return MoveResult.Ok();
        }

        // 2) A tiger jump. Look through each direction: the neighbor in that
        //    direction is the point being jumped over (the goat).
        if (piece == PointState.Tiger)
        {
            foreach (Neighbor n in Board.GetNeighbors(from))
            {
                if (Board.CanJump(from, n.Dir) && Board.GetJumpLanding(from, n.Dir) == to)
                {
                    Board.SetState(n.Index, PointState.Empty); // remove captured goat
                    Board.SetState(to, PointState.Tiger);
                    Board.SetState(from, PointState.Empty);
                    GoatsCaptured++;
                    EndTurn();
                    return MoveResult.Ok(n.Index);
                }
            }
        }

        return MoveResult.Fail(MoveError.IllegalMove);
    }

    // ============================================================
    // QUESTIONS the UI can ask
    // ============================================================

    // All points the piece on 'from' may legally go to right now.
    // Empty list if it isn't that piece's turn, game is over, etc.
    public List<int> GetLegalDestinations(int from)
    {
        var list = new List<int>();
        if (Result != GameResult.Ongoing || !IsValidIndex(from)) return list;

        PointState piece = Board.GetState(from);
        if (piece == PointState.Empty || SideOf(piece) != Turn) return list;
        if (piece == PointState.Goat && CurrentPhase == Phase.Placing) return list;

        CollectMoves(from, piece, list);
        return list;
    }

    // True if moving tiger 'from' -> 'to' is a capture (not a one-step move).
    public bool IsJump(int from, int to)
    {
        return Board.GetState(from) == PointState.Tiger && !IsStep(from, to);
    }

    // ============================================================
    // INTERNAL HELPERS
    // ============================================================

    private static bool IsValidIndex(int index)
    {
        return index >= 0 && index < BagchalBoard.PointCount;
    }

    private static Side SideOf(PointState piece)
    {
        return piece == PointState.Tiger ? Side.Tiger : Side.Goat;
    }

    // Is 'to' directly connected to 'from'?
    private bool IsStep(int from, int to)
    {
        foreach (Neighbor n in Board.GetNeighbors(from))
            if (n.Index == to) return true;
        return false;
    }

    // Fills 'list' with every destination for a piece (steps, plus jumps for tigers).
    // Does NOT check whose turn it is - callers do that.
    private void CollectMoves(int from, PointState piece, List<int> list)
    {
        foreach (Neighbor n in Board.GetNeighbors(from))
        {
            if (Board.GetState(n.Index) == PointState.Empty)
            {
                list.Add(n.Index); // step
            }
            else if (piece == PointState.Tiger && Board.CanJump(from, n.Dir))
            {
                list.Add(Board.GetJumpLanding(from, n.Dir)); // jump over a goat
            }
        }
    }

    private bool HasLegalMove(Side side)
    {
        // In the placing phase goats can always place (the board never fills up).
        if (side == Side.Goat && CurrentPhase == Phase.Placing) return true;

        PointState mine = side == Side.Tiger ? PointState.Tiger : PointState.Goat;
        var tmp = new List<int>();
        for (int i = 0; i < BagchalBoard.PointCount; i++)
        {
            if (Board.GetState(i) != mine) continue;
            tmp.Clear();
            CollectMoves(i, mine, tmp);
            if (tmp.Count > 0) return true;
        }
        return false;
    }

    private void EndTurn()
    {
        Turn = (Turn == Side.Goat) ? Side.Tiger : Side.Goat;
        UpdateResult();
    }

    // Checked after every action.
    private void UpdateResult()
    {
        if (GoatsCaptured >= CapturesToWin)
        {
            Result = GameResult.TigersWin;
            return;
        }

        // The side that must move next has no legal move:
        //  - tigers stuck  -> goats win (the classic win condition)
        //  - goats stuck   -> tigers win (house-rule choice; very rare)
        if (!HasLegalMove(Turn))
            Result = (Turn == Side.Tiger) ? GameResult.GoatsWin : GameResult.TigersWin;
    }
}