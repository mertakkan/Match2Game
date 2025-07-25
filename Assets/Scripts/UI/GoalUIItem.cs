using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalUIItem : MonoBehaviour
{
    [Header("UI References")]
    public Image goalIcon;
    public TextMeshProUGUI goalTextTMP; // Using TextMeshPro

    private LevelGoal currentGoal;

    // Initialize the goal item with the starting target amount
    public void Initialize(LevelGoal goal, Sprite cubeSprite)
    {
        currentGoal = new LevelGoal
        {
            colorIndex = goal.colorIndex,
            targetAmount = goal.targetAmount,
            currentAmount = 0
        };

        if (goalIcon)
            goalIcon.sprite = cubeSprite;

        UpdateDisplay();
    }

    // This is called by UIManager whenever a cube is collected
    public void UpdateGoal(LevelGoal goal)
    {
        int oldAmount = currentGoal != null ? currentGoal.currentAmount : 0;
        currentGoal = goal; // Update the internal goal state
        UpdateDisplay();

        // Animate only when progress is made
        if (goal.currentAmount > oldAmount)
        {
            AnimateGoalCollection();
        }
    }

    // REWORKED: This method now displays the remaining amount
    void UpdateDisplay()
    {
        if (currentGoal == null)
            return;

        // Calculate the remaining amount needed
        int remainingAmount = currentGoal.targetAmount - currentGoal.currentAmount;
        if (remainingAmount < 0)
            remainingAmount = 0; // Prevent negative numbers

        // Update the text to show only the remaining amount
        if (goalTextTMP != null)
        {
            goalTextTMP.text = remainingAmount.ToString();
        }

        // If the goal is complete, you could add extra feedback,
        // like hiding the text or showing a checkmark.
        if (remainingAmount <= 0)
        {
            // For example, hide the text when the goal is met.
            goalTextTMP.gameObject.SetActive(false);
            // Optionally, change the icon to be grayed out
            if (goalIcon)
                goalIcon.color = Color.gray;
        }
    }

    void AnimateGoalCollection()
    {
        // Simple scale animation to give feedback
        StartCoroutine(ScaleAnimation());
    }

    System.Collections.IEnumerator ScaleAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;
        float duration = 0.1f;

        // Scale up
        for (float t = 0; t < 1; t += Time.deltaTime / duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // Scale down
        for (float t = 0; t < 1; t += Time.deltaTime / duration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
