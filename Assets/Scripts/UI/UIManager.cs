using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Text movesText;
    public Transform goalsContainer;
    public GameObject goalItemPrefab;

    [Header("Panels")]
    public GameObject winPanel;
    public GameObject losePanel;
    public Button restartButton;

    private List<GoalUIItem> goalUIItems = new List<GoalUIItem>();
    private GameConfig config;

    void Start()
    {
        config = GameManager.Instance.gameConfig;
        SetupGoalUI();
        SetupPanels();
    }

    void SetupGoalUI()
    {
        // Clear existing goal items
        foreach (Transform child in goalsContainer)
        {
            Destroy(child.gameObject);
        }
        goalUIItems.Clear();

        // Create goal UI items
        foreach (var goal in config.levelGoals)
        {
            GameObject goalItem = CreateGoalItem(goal);
            goalUIItems.Add(goalItem.GetComponent<GoalUIItem>());
        }
    }

    GameObject CreateGoalItem(LevelGoal goal)
    {
        // Create goal item programmatically if prefab doesn't exist
        GameObject goalItem = new GameObject("GoalItem");
        goalItem.transform.SetParent(goalsContainer);

        // Add components
        Image background = goalItem.AddComponent<Image>();
        background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(goalItem.transform);
        Image icon = iconObj.AddComponent<Image>();
        icon.sprite = config.cubeSprites[goal.colorIndex];

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(goalItem.transform);
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        // Setup GoalUIItem component
        GoalUIItem goalUIItem = goalItem.AddComponent<GoalUIItem>();
        goalUIItem.goalIcon = icon;
        goalUIItem.goalText = text;
        goalUIItem.Initialize(goal, config.cubeSprites[goal.colorIndex]);

        // Set RectTransforms
        SetupGoalItemLayout(goalItem, iconObj, textObj);

        return goalItem;
    }

    void SetupGoalItemLayout(GameObject goalItem, GameObject iconObj, GameObject textObj)
    {
        // Goal item layout
        RectTransform goalRect = goalItem.GetComponent<RectTransform>();
        goalRect.sizeDelta = new Vector2(100, 100);
        goalRect.anchorMin = goalRect.anchorMax = new Vector2(0.5f, 0.5f);
        goalRect.localScale = Vector3.one;
        goalRect.localPosition = Vector3.zero;

        // Icon layout
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(50, 50);
        iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.7f);
        iconRect.localPosition = Vector3.zero;
        iconRect.localScale = Vector3.one;

        // Text layout
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(90, 30);
        textRect.anchorMin = textRect.anchorMax = new Vector2(0.5f, 0.3f);
        textRect.localPosition = Vector3.zero;
        textRect.localScale = Vector3.one;
    }

    void SetupPanels()
    {
        // Create win/lose panels if they don't exist
        if (winPanel == null)
            winPanel = CreateGameOverPanel("You Win!", Color.green);

        if (losePanel == null)
            losePanel = CreateGameOverPanel("Game Over", Color.red);

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    GameObject CreateGameOverPanel(string message, Color color)
    {
        GameObject panel = new GameObject(message + "Panel");
        panel.transform.SetParent(transform);

        // Background
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(panel.transform);
        Text text = textObj.AddComponent<Text>();
        text.text = message;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 48;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;

        // Layout
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.localPosition = Vector3.zero;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(400, 100);
        textRect.localPosition = Vector3.zero;

        panel.SetActive(false);
        return panel;
    }

    public void UpdateUI()
    {
        UpdateMoves(GameManager.Instance.currentMoves);
        UpdateGoals(GameManager.Instance.currentGoals);
    }

    public void UpdateMoves(int moves)
    {
        if (movesText)
            movesText.text = "Moves: " + moves.ToString();
    }

    public void UpdateGoals(List<LevelGoal> goals)
    {
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
        foreach (var goalItem in goalUIItems)
        {
            // Find matching color goal
            if (goalItem != null)
            {
                return goalItem.GetWorldPosition();
            }
        }
        return Vector3.zero;
    }

    void RestartGame()
    {
        GameManager.Instance.RestartLevel();
        HideGameOverPanels();
    }
}
