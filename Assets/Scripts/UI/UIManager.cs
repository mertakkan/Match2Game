using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public void UpdateUI()
    {
        Debug.Log("UI Updated");
    }

    public void UpdateMoves(int moves)
    {
        Debug.Log($"Moves remaining: {moves}");
    }

    public void UpdateGoals(List<LevelGoal> goals)
    {
        Debug.Log("Goals updated");
    }

    public void ShowWinScreen()
    {
        Debug.Log("Level Complete!");
    }

    public void ShowLoseScreen()
    {
        Debug.Log("Game Over!");
    }
}
