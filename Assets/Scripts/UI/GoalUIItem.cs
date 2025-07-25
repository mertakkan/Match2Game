using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalUIItem : MonoBehaviour
{
    [Header("UI References")]
    public Image goalIcon;
    public TextMeshProUGUI goalTextTMP;

    private LevelGoal currentGoal;

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

    public void UpdateGoal(LevelGoal goal)
    {
        int oldAmount = currentGoal != null ? currentGoal.currentAmount : 0;
        currentGoal = goal;
        UpdateDisplay();

        if (goal.currentAmount > oldAmount)
        {
            AnimateGoalCollection();
        }
    }

    void UpdateDisplay()
    {
        if (currentGoal == null)
            return;

        int remainingAmount = currentGoal.targetAmount - currentGoal.currentAmount;
        if (remainingAmount < 0)
            remainingAmount = 0;

        if (goalTextTMP != null)
        {
            goalTextTMP.text = remainingAmount.ToString();
        }

        if (remainingAmount <= 0)
        {
            goalTextTMP.gameObject.SetActive(false);

            if (goalIcon)
                goalIcon.color = Color.gray;
        }
    }

    void AnimateGoalCollection()
    {
        StartCoroutine(ScaleAnimation());
    }

    System.Collections.IEnumerator ScaleAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;
        float duration = 0.1f;

        for (float t = 0; t < 1; t += Time.deltaTime / duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        for (float t = 0; t < 1; t += Time.deltaTime / duration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
