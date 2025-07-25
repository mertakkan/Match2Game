using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI movesTextTMP;
    public Transform goalsContainer;
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

        if (winPanel)
            winPanel.SetActive(false);
        if (losePanel)
            losePanel.SetActive(false);

        SetupMovesText();
        SetupGoalUI();
        SetupPanels();
    }

    void SetupMovesText()
    {
        if (movesTextTMP != null)
        {
            RectTransform movesRect = movesTextTMP.GetComponent<RectTransform>();

            movesRect.anchorMin = new Vector2(1, 1);
            movesRect.anchorMax = new Vector2(1, 1);
            movesRect.pivot = new Vector2(1, 1);
            movesRect.anchoredPosition = new Vector2(-60, -60);
            movesRect.sizeDelta = new Vector2(150, 100);

            movesTextTMP.text = "" + GameManager.Instance.currentMoves;
            movesTextTMP.fontSize = 90;
            movesTextTMP.color = new Color32(198, 68, 60, 255);
            movesTextTMP.alignment = TextAlignmentOptions.Center;
            movesTextTMP.fontStyle = FontStyles.Bold;
            movesTextTMP.outlineWidth = 0.25f;
            movesTextTMP.outlineColor = new Color32(255, 243, 224, 255);
        }
    }

    void SetupGoalUI()
    {
        RectTransform goalsRect = goalsContainer.GetComponent<RectTransform>();
        if (goalsRect != null)
        {
            goalsRect.anchorMin = new Vector2(0.5f, 1);
            goalsRect.anchorMax = new Vector2(0.5f, 1);
            goalsRect.pivot = new Vector2(0.5f, 1);
            goalsRect.anchoredPosition = new Vector2(25, -55);
            goalsRect.sizeDelta = new Vector2(350, 120);
        }

        foreach (Transform child in goalsContainer)
        {
            Destroy(child.gameObject);
        }
        goalUIItems.Clear();

        HorizontalLayoutGroup layoutGroup = goalsContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = goalsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = 15;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;

        foreach (var goal in config.levelGoals)
        {
            GameObject goalItemObj = CreateGoalItem(goal);
            goalUIItems.Add(goalItemObj.GetComponent<GoalUIItem>());
        }
    }

    // REWORKED: This function now positions the text to the bottom-right.
    GameObject CreateGoalItem(LevelGoal goal)
    {
        GameObject goalItem = new GameObject($"GoalItem_{goal.colorIndex}");
        goalItem.transform.SetParent(goalsContainer, false);
        goalItem.AddComponent<RectTransform>().sizeDelta = new Vector2(110, 110);

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(goalItem.transform, false);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.sprite = config.cubeSprites[goal.colorIndex];
        iconImage.rectTransform.sizeDelta = new Vector2(110, 110); // Make icon fill the space
        iconImage.rectTransform.anchoredPosition = Vector2.zero;

        GameObject textObj = new GameObject("Text (TMP)");
        textObj.transform.SetParent(goalItem.transform, false);
        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();

        // Set the initial text to be the total target amount
        tmpText.text = goal.targetAmount.ToString();
        tmpText.fontSize = 52;
        tmpText.color = new Color32(0, 0, 0, 255);
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.outlineWidth = 0.2f;
        tmpText.outlineColor = new Color32(255, 243, 224, 255);

        // --- POSITIONING & ALIGNMENT CHANGES ---
        RectTransform textRect = textObj.GetComponent<RectTransform>();

        // Anchor and pivot to the bottom-right of the parent (the icon)
        textRect.anchorMin = new Vector2(1, 0);
        textRect.anchorMax = new Vector2(1, 0);
        textRect.pivot = new Vector2(1, 0);

        // Set the size of the text box
        textRect.sizeDelta = new Vector2(80, 50);

        // Position with padding from the corner
        textRect.anchoredPosition = new Vector2(-5, 5);

        // Align the text itself to the center of its own box for a clean look
        tmpText.alignment = TextAlignmentOptions.Center;

        // --- END OF CHANGES ---

        GoalUIItem goalUIComponent = goalItem.AddComponent<GoalUIItem>();
        goalUIComponent.goalIcon = iconImage;
        goalUIComponent.goalTextTMP = tmpText;
        goalUIComponent.Initialize(goal, config.cubeSprites[goal.colorIndex]);

        return goalItem;
    }

    void SetupPanels()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }
        else if (winPanel != null && losePanel != null)
        {
            Button winRestartButton = winPanel.GetComponentInChildren<Button>();
            if (winRestartButton)
                winRestartButton.onClick.AddListener(RestartGame);

            Button loseRestartButton = losePanel.GetComponentInChildren<Button>();
            if (loseRestartButton)
                loseRestartButton.onClick.AddListener(RestartGame);
        }
    }

    public void UpdateUI()
    {
        UpdateMoves(GameManager.Instance.currentMoves);
        UpdateGoals(GameManager.Instance.currentGoals);
    }

    public void UpdateMoves(int moves)
    {
        if (movesTextTMP != null)
            movesTextTMP.text = moves.ToString();
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

    void RestartGame()
    {
        GameManager.Instance.RestartLevel();
        HideGameOverPanels();
    }
}
