using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Text movesText;
    public TextMeshProUGUI movesTextTMP;
    public Transform goalsContainer;
    public GameObject goalItemPrefab;

    [Header("Panels")]
    public GameObject winPanel;
    public GameObject losePanel;
    public Button restartButton;

    [Header("Font Settings")]
    public Font defaultFont;

    private List<GoalUIItem> goalUIItems = new List<GoalUIItem>();
    private GameConfig config;

    void Start()
    {
        config = GameManager.Instance.gameConfig;

        if (defaultFont == null)
        {
            defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        // Fix the moves text positioning and content
        FixMovesText();

        SetupGoalUI();
        SetupPanels();
    }

    void FixMovesText()
    {
        if (movesTextTMP != null)
        {
            // Fix positioning
            RectTransform movesRect = movesTextTMP.GetComponent<RectTransform>();
            movesRect.anchorMin = new Vector2(0.1f, 0.5f);
            movesRect.anchorMax = new Vector2(0.4f, 0.5f);
            movesRect.anchoredPosition = Vector2.zero;
            movesRect.sizeDelta = new Vector2(200, 50);

            // Fix text content and styling
            movesTextTMP.text = "Moves: " + GameManager.Instance.currentMoves;
            movesTextTMP.fontSize = 28;
            movesTextTMP.color = Color.white;
            movesTextTMP.alignment = TextAlignmentOptions.Center;
            movesTextTMP.fontStyle = FontStyles.Bold;
        }
    }

    void SetupGoalUI()
    {
        // Clear existing goal items
        foreach (Transform child in goalsContainer)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        goalUIItems.Clear();

        // Fix goals container positioning
        RectTransform goalsRect = goalsContainer.GetComponent<RectTransform>();
        goalsRect.anchorMin = new Vector2(0.6f, 0.5f);
        goalsRect.anchorMax = new Vector2(0.9f, 0.5f);
        goalsRect.anchoredPosition = Vector2.zero;
        goalsRect.sizeDelta = new Vector2(300, 80);

        // Create goal UI items
        foreach (var goal in config.levelGoals)
        {
            GameObject goalItem = CreateGoalItem(goal);
            goalUIItems.Add(goalItem.GetComponent<GoalUIItem>());
        }
    }

    GameObject CreateGoalItem(LevelGoal goal)
    {
        GameObject goalItem = new GameObject("GoalItem");
        goalItem.transform.SetParent(goalsContainer, false);

        // Add background
        Image background = goalItem.AddComponent<Image>();
        background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Setup layout
        RectTransform goalRect = goalItem.GetComponent<RectTransform>();
        goalRect.sizeDelta = new Vector2(80, 80);

        // Create icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(goalItem.transform, false);
        Image icon = iconObj.AddComponent<Image>();
        icon.sprite = config.cubeSprites[goal.colorIndex];

        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.7f);
        iconRect.sizeDelta = new Vector2(40, 40);
        iconRect.anchoredPosition = Vector2.zero;

        // Create text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(goalItem.transform, false);

        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = $"0/{goal.targetAmount}";
        tmpText.fontSize = 18;
        tmpText.color = Color.white;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.fontStyle = FontStyles.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = textRect.anchorMax = new Vector2(0.5f, 0.3f);
        textRect.sizeDelta = new Vector2(70, 25);
        textRect.anchoredPosition = Vector2.zero;

        // Add GoalUIItem component
        GoalUIItem goalUIItem = goalItem.AddComponent<GoalUIItem>();
        goalUIItem.goalIcon = icon;
        goalUIItem.goalTextTMP = tmpText;
        goalUIItem.Initialize(goal, config.cubeSprites[goal.colorIndex]);

        return goalItem;
    }

    void SetupPanels()
    {
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
        panel.transform.SetParent(transform, false);

        // Background
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(panel.transform, false);

        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = message;
        tmpText.fontSize = 48;
        tmpText.color = color;
        tmpText.alignment = TextAlignmentOptions.Center;

        // Layout
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(400, 100);
        textRect.anchoredPosition = Vector2.zero;

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
        string moveText = "Moves: " + moves.ToString();

        if (movesTextTMP != null)
            movesTextTMP.text = moveText;
        else if (movesText != null)
            movesText.text = moveText;
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
