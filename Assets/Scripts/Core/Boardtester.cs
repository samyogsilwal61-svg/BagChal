using UnityEngine;

// ============================================================
// BoardTester.cs - a throwaway MonoBehaviour to test the board.
// 1. Create an empty GameObject in a scene.
// 2. Drag this script onto it.
// 3. Press Play and read the Console window.
// Delete it when you're happy (or convert to Unity Test Runner).
// ============================================================
public class BoardTester : MonoBehaviour
{
    void Start()
    {
        var board = new BagchalBoard();

        // Neighbor counts from the Step 3 checklist
        Check("Point 0 has 3 neighbors", board.GetNeighbors(0).Count == 3);
        Check("Point 1 has 3 neighbors", board.GetNeighbors(1).Count == 3);
        Check("Point 2 has 5 neighbors", board.GetNeighbors(2).Count == 5);
        Check("Point 12 has 8 neighbors", board.GetNeighbors(12).Count == 8);

        // Jump geometry
        var downRight = new Direction(1, 1);
        var right = new Direction(0, 1);
        Check("0 jumping down-right lands on 12",
              board.GetJumpLanding(0, downRight) == 12);
        Check("3 jumping right goes off board",
              board.GetJumpLanding(3, right) == -1);
        Check("1 can't jump diagonally",
              board.GetJumpLanding(1, downRight) == -1);

        // Jump with state: tiger at 0 (default), goat at 6, 12 empty
        board.SetState(6, PointState.Goat);
        Check("Tiger 0 can capture goat at 6",
              board.CanJump(0, downRight));

        // Neighbor relationships must be mutual
        bool mutual = true;
        for (int a = 0; a < BagchalBoard.PointCount; a++)
            foreach (var n in board.GetNeighbors(a))
            {
                bool found = false;
                foreach (var back in board.GetNeighbors(n.Index))
                    if (back.Index == a) found = true;
                if (!found) mutual = false;
            }
        Check("All neighbor links are mutual", mutual);
    }

    void Check(string label, bool passed)
    {
        Debug.Log((passed ? "PASS: " : "FAIL: ") + label);
    }
}