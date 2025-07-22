using UnityEngine;
using UnityEngine.UI;

public class GoalUIItem : MonoBehaviour
{
    [Header("UI References")]
    public Image goalIcon;
    public Text goalText;
    public Image progressBar;

    private LevelGoal currentGoal;

    public void Initialize(LevelGoal goal, Sprite cubeSprite)
    {
        currentGoal = goal;

        if (goalIcon)
            goalIcon.sprite = cubeSprite;

        UpdateDisplay();
    }

    public void UpdateGoal(LevelGoal goal)
    {
        currentGoal = goal;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (goalText)
            goalText.text = $"{currentGoal.currentAmount}/{currentGoal.targetAmount}";

        if (progressBar)
        {
            float progress = (float)currentGoal.currentAmount / currentGoal.targetAmount;
            progressBar.fillAmount = Mathf.Clamp01(progress);
        }
    }

    public Vector3 GetWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(transform.position);
    }
}
