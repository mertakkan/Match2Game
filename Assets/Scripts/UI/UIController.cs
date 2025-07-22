using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("UI References")]
    public Canvas mainCanvas;
    public Image backgroundImage;
    public Image topUIImage;
    public Image bottomUIImage;
    public Text movesText;
    public Transform goalsContainer;
    public GameObject goalItemPrefab;

    [Header("Game Over UI")]
    public GameObject winPanel;
    public GameObject losePanel;
    public Button restartButton;

    private List<GoalUIItem> goalUIItems = new List<GoalUIItem>();

    void Start()
    {
        SetupUI();
        SetupGameOverPanels();
    }

    void SetupUI()
    {
        GameConfig config = GameManager.Instance.gameConfig;

        // Set background and UI sprites
        if (backgroundImage && config.backgroundSprite)
            backgroundImage.sprite = config.backgroundSprite;

        if (topUIImage && config.topUISprite)
            topUIImage.sprite = config.topUISprite;

        if (bottomUIImage && config.bottomUISprite)
            bottomUIImage.sprite = config.bottomUISprite;

        // Setup canvas for different screen sizes
        CanvasScaler scaler = mainCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    void SetupGameOverPanels()
    {
        if (restartButton)
        {
            restartButton.onClick.AddListener(() =>
            {
                GameManager.Instance.RestartLevel();
                HideGameOverPanels();
            });
        }
    }

    public void UpdateMoves(int moves)
    {
        if (movesText)
            movesText.text = moves.ToString();
    }

    public void UpdateGoals(List<LevelGoal> goals)
    {
        // Update existing goal UI items
        for (int i = 0; i < goalUIItems.Count && i < goals.Count; i++)
        {
            goalUIItems[i].UpdateGoal(goals[i]);
        }
    }

    public void ShowWinScreen()
    {
        if (winPanel)
            winPanel.SetActive(true);
    }

    public void ShowLoseScreen()
    {
        if (losePanel)
            losePanel.SetActive(true);
    }

    public void HideGameOverPanels()
    {
        if (winPanel)
            winPanel.SetActive(false);
        if (losePanel)
            losePanel.SetActive(false);
    }

    public Vector3 GetGoalUIPosition(int colorIndex)
    {
        // Return world position of goal UI for animation
        if (colorIndex < goalUIItems.Count)
        {
            return Camera.main.ScreenToWorldPoint(goalUIItems[colorIndex].transform.position);
        }
        return Vector3.zero;
    }
}

[System.Serializable]
public class GoalUIItem : MonoBehaviour
{
    public Image goalIcon;
    public Text goalText;

    public void UpdateGoal(LevelGoal goal)
    {
        if (goalText)
            goalText.text = $"{goal.currentAmount}/{goal.targetAmount}";
    }
}
