using TMPro;
using UnityEngine;

// ============================================================
// GameHUD.cs - shows counters, turn, messages, and game over.
// Every field is optional: leave a slot empty to skip that item.
// Put on your Canvas (or a child). Put in Assets/Scripts/UI.
// ============================================================
public class GameHUD : MonoBehaviour
{
    [Header("Top bar texts (all optional)")]
    public TMP_Text turnText;
    public TMP_Text goatsInHandText;
    public TMP_Text goatsOnBoardText;
    public TMP_Text capturedText;
    public TMP_Text messageText;

    [Header("Game over")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;

    public void UpdateHUD(BagchalGame game)
    {
        string turn;
        if (game.Turn == Side.Goat)
            turn = game.CurrentPhase == Phase.Placing ? "Goats: place a goat" : "Goats: move a goat";
        else
            turn = "Tigers: move a tiger";

        Set(turnText, turn);
        Set(goatsInHandText, "Goats to place: " + game.GoatsInHand);
        Set(goatsOnBoardText, "Goats on board: " + game.GoatsOnBoard);
        Set(capturedText, "Dead goats: " + game.GoatsCaptured + " / " + BagchalGame.CapturesToWin);
    }

    public void ShowMessage(string msg)
    {
        Set(messageText, msg);
    }

    public void ShowGameOver(string text)
    {
        Set(gameOverText, text);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private static void Set(TMP_Text t, string s)
    {
        if (t != null) t.text = s;
    }
}