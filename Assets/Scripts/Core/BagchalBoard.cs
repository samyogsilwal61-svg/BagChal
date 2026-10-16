using System.Collections.Generic;

// ============================================================
// BagchalBoard.cs  -  PURE DATA. No GameObjects, no sprites.
// Put this in Assets/Scripts. It does NOT inherit MonoBehaviour,
// so you can't attach it to an object; other scripts create it
// with:  var board = new BagchalBoard();
// ============================================================

// ---------- 1. POINT STATE ----------
// An enum = a fixed list of named options. Each point on the
// board is exactly one of these.
public enum PointState
{
    Empty,
    Tiger,
    Goat
}

// ---------- 2. DIRECTION ----------
// A direction is just "how much does row change, how much does
// column change". Example: right = (0, +1), up-left = (-1, -1).
// A struct is a lightweight bundle of values.
public struct Direction
{
    public int RowDelta;
    public int ColDelta;

    public Direction(int rowDelta, int colDelta)
    {
        RowDelta = rowDelta;
        ColDelta = colDelta;
    }
}

// ---------- 3. NEIGHBOR ----------
// We store WHICH point is adjacent AND which direction we went to
// reach it. The direction is what makes jump logic easy later.
public struct Neighbor
{
    public int Index;       // the neighboring point's number (0-24)
    public Direction Dir;   // the direction we moved to get there

    public Neighbor(int index, Direction dir)
    {
        Index = index;
        Dir = dir;
    }
}

// ---------- 4. THE BOARD ----------
public class BagchalBoard
{
    // Constants: values that never change. "const" = fixed at compile time.
    public const int Size = 5;                 // 5 rows, 5 columns
    public const int PointCount = Size * Size; // 25 points total

    // The 4 straight directions every point can use.
    private static readonly Direction[] StraightDirs =
    {
        new Direction(-1,  0), // up
        new Direction( 1,  0), // down
        new Direction( 0, -1), // left
        new Direction( 0,  1), // right
    };

    // The 4 diagonal directions - ONLY "even" points can use these.
    private static readonly Direction[] DiagonalDirs =
    {
        new Direction(-1, -1), // up-left
        new Direction(-1,  1), // up-right
        new Direction( 1, -1), // down-left
        new Direction( 1,  1), // down-right
    };

    // What is on each of the 25 points. Index = point number.
    private PointState[] states = new PointState[PointCount];

    // For each of the 25 points, a list of its neighbors.
    // neighbors[12] = the list of neighbors of the center point.
    private List<Neighbor>[] neighbors = new List<Neighbor>[PointCount];

    // Constructor: runs once when you write "new BagchalBoard()".
    public BagchalBoard()
    {
        BuildNeighbors(); // connections never change, so build once
        ResetBoard();     // put pieces in starting positions
    }

    // ============================================================
    // INDEX <-> (ROW, COLUMN) CONVERSION
    // Board numbering:   0  1  2  3  4
    //                    5  6  7  8  9   ... and so on
    // ============================================================

    // "/" on two ints throws away the remainder: 7 / 5 = 1
    public static int ToRow(int index) { return index / Size; }

    // "%" gives the remainder: 7 % 5 = 2
    public static int ToCol(int index) { return index % Size; }

    // Reverse: row 1, col 2 -> 1*5 + 2 = 7
    public static int ToIndex(int row, int col) { return row * Size + col; }

    // Is this row/col inside the 5x5 board? Use before converting back
    // to an index, or you'll read outside the array and crash.
    public static bool IsOnBoard(int row, int col)
    {
        return row >= 0 && row < Size && col >= 0 && col < Size;
    }

    // Only points where (row + col) is EVEN have diagonal lines.
    // "% 2 == 0" is the standard even check.
    public static bool HasDiagonals(int index)
    {
        return (ToRow(index) + ToCol(index)) % 2 == 0;
    }

    // ============================================================
    // BUILDING THE ADJACENCY LIST (runs once)
    // ============================================================
    private void BuildNeighbors()
    {
        for (int i = 0; i < PointCount; i++)
        {
            neighbors[i] = new List<Neighbor>();

            int row = ToRow(i);
            int col = ToCol(i);

            // Every point tries the 4 straight directions.
            AddNeighborsFor(i, row, col, StraightDirs);

            // Only even points also try the 4 diagonal directions.
            if (HasDiagonals(i))
                AddNeighborsFor(i, row, col, DiagonalDirs);
        }
    }

    // Helper: try each direction from point i; keep the ones on the board.
    private void AddNeighborsFor(int i, int row, int col, Direction[] dirs)
    {
        foreach (Direction d in dirs)
        {
            int newRow = row + d.RowDelta;
            int newCol = col + d.ColDelta;

            // Skip anything that falls off the board.
            if (!IsOnBoard(newRow, newCol)) continue;

            neighbors[i].Add(new Neighbor(ToIndex(newRow, newCol), d));
        }
    }

    // ============================================================
    // READING AND WRITING STATE
    // ============================================================

    // Starting position: tigers on the 4 corners, everything else empty.
    public void ResetBoard()
    {
        for (int i = 0; i < PointCount; i++)
            states[i] = PointState.Empty;

        states[ToIndex(0, 0)]             = PointState.Tiger;
        states[ToIndex(0, Size - 1)]      = PointState.Tiger;
        states[ToIndex(Size - 1, 0)]      = PointState.Tiger;
        states[ToIndex(Size - 1, Size - 1)] = PointState.Tiger;
    }

    public PointState GetState(int index) { return states[index]; }

    public void SetState(int index, PointState value) { states[index] = value; }

    // Read-only access to a point's neighbors (for move highlighting etc).
    public List<Neighbor> GetNeighbors(int index) { return neighbors[index]; }

    // ============================================================
    // JUMP LOGIC
    // ============================================================

    // GEOMETRY ONLY: if a tiger at 'from' jumps in direction 'dir',
    // where does it land? Returns -1 if no such jump line exists.
    // Does NOT look at who is standing where.
    public int GetJumpLanding(int from, Direction dir)
    {
        // Diagonal direction? Then the start point must have diagonals.
        bool isDiagonal = dir.RowDelta != 0 && dir.ColDelta != 0;
        if (isDiagonal && !HasDiagonals(from)) return -1;

        // Step one: the point being jumped over (B).
        int midRow = ToRow(from) + dir.RowDelta;
        int midCol = ToCol(from) + dir.ColDelta;
        if (!IsOnBoard(midRow, midCol)) return -1;

        // Step two: the landing point (C) = same direction again.
        int landRow = midRow + dir.RowDelta;
        int landCol = midCol + dir.ColDelta;
        if (!IsOnBoard(landRow, landCol)) return -1;

        return ToIndex(landRow, landCol);
    }

    // GEOMETRY + STATE: can the tiger at 'from' capture in direction 'dir'?
    // Needs: a goat in between, and an empty landing point.
    public bool CanJump(int from, Direction dir)
    {
        int landing = GetJumpLanding(from, dir);
        if (landing == -1) return false;

        int middle = ToIndex(ToRow(from) + dir.RowDelta,
                             ToCol(from) + dir.ColDelta);

        return states[middle] == PointState.Goat
            && states[landing] == PointState.Empty;
    }
}