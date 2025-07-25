using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public GameConfig gameConfig;
    public GridManager gridManager;
    public UIManager uiManager;
    public AudioManager audioManager;
    public EffectsManager effectsManager;

    [Header("Game State")]
    public int currentMoves;
    public bool gameActive = true;
    public List<LevelGoal> currentGoals;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
        if (audioManager == null)
            audioManager = FindObjectOfType<AudioManager>();
        if (effectsManager == null)
            effectsManager = FindObjectOfType<EffectsManager>();

        InitializeGame();
    }

    void InitializeGame()
    {
        if (gameConfig == null)
        {
            gameConfig = Resources.Load<GameConfig>("GameConfig");
            if (gameConfig == null)
            {
                Debug.LogError("GameConfig not found in Resources folder!");
                return;
            }
        }

        currentMoves = gameConfig.movesPerLevel;
        currentGoals = new List<LevelGoal>();

        if (gameConfig.levelGoals != null)
        {
            foreach (var goal in gameConfig.levelGoals)
            {
                currentGoals.Add(
                    new LevelGoal
                    {
                        goalType = goal.goalType,
                        colorIndex = goal.colorIndex,
                        targetAmount = goal.targetAmount,
                        currentAmount = 0
                    }
                );
            }
        }

        if (gridManager != null)
            gridManager.InitializeGrid();
        else
            Debug.LogError("GridManager reference is missing!");

        if (uiManager != null)
            uiManager.UpdateUI();
        else
            Debug.LogError("UIManager reference is missing!");
    }

    public bool TryMakeMove()
    {
        if (!gameActive || currentMoves <= 0)
            return false;

        currentMoves--;
        uiManager.UpdateMoves(currentMoves);

        CheckGameEnd();
        return true;
    }

    public void CollectCube(int colorIndex, int amount = 1)
    {
        foreach (var goal in currentGoals)
        {
            if (goal.goalType == GoalType.Cube && goal.colorIndex == colorIndex)
            {
                goal.currentAmount = Mathf.Min(goal.currentAmount + amount, goal.targetAmount);
                uiManager.UpdateGoals(currentGoals);
                break;
            }
        }
        CheckWinCondition();
    }

    public void CollectDuck(int amount = 1)
    {
        foreach (var goal in currentGoals)
        {
            if (goal.goalType == GoalType.Duck)
            {
                goal.currentAmount = Mathf.Min(goal.currentAmount + amount, goal.targetAmount);
                uiManager.UpdateGoals(currentGoals);
                break;
            }
        }
        CheckWinCondition();
    }

    public void CollectBalloon(int amount = 1)
    {
        foreach (var goal in currentGoals)
        {
            if (goal.goalType == GoalType.Balloon)
            {
                goal.currentAmount = Mathf.Min(goal.currentAmount + amount, goal.targetAmount);
                uiManager.UpdateGoals(currentGoals);
                break;
            }
        }
        CheckWinCondition();
    }

    void CheckWinCondition()
    {
        bool allGoalsComplete = true;
        foreach (var goal in currentGoals)
        {
            if (goal.currentAmount < goal.targetAmount)
            {
                allGoalsComplete = false;
                break;
            }
        }

        if (allGoalsComplete)
        {
            gameActive = false;
            uiManager.ShowWinScreen();
        }
    }

    void CheckGameEnd()
    {
        if (currentMoves <= 0)
        {
            bool hasGoalsComplete = true;
            foreach (var goal in currentGoals)
            {
                if (goal.currentAmount < goal.targetAmount)
                {
                    hasGoalsComplete = false;
                    break;
                }
            }

            if (!hasGoalsComplete)
            {
                gameActive = false;
                uiManager.ShowLoseScreen();
            }
        }
    }

    public void RestartLevel()
    {
        gameActive = true;
        InitializeGame();
    }
}
