using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalUIItem : MonoBehaviour
{
    [Header("UI References")]
    public Image goalIcon;
    public Text goalText; // Legacy text support
    public TextMeshProUGUI goalTextTMP; // TextMeshPro support - ADD THIS LINE
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
        string displayText = $"{currentGoal.currentAmount}/{currentGoal.targetAmount}";

        // Update whichever text component exists
        if (goalTextTMP != null)
            goalTextTMP.text = displayText;
        else if (goalText != null)
            goalText.text = displayText;

        if (progressBar)
        {
            float progress = (float)currentGoal.currentAmount / currentGoal.targetAmount;
            progressBar.fillAmount = Mathf.Clamp01(progress);
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
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }

        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    public Vector3 GetWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(transform.position);
    }
}
