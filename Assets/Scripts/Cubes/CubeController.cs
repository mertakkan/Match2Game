using UnityEngine;

public class CubeController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Collider2D cubeCollider;

    public int ColorIndex { get; private set; }
    public Vector2Int GridPosition { get; private set; }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cubeCollider = GetComponent<Collider2D>();
    }

    public void Initialize(int colorIndex, Sprite cubeSprite)
    {
        ColorIndex = colorIndex;
        spriteRenderer.sprite = cubeSprite;

        // Ensure proper sorting
        spriteRenderer.sortingLayerName = "Default";
        spriteRenderer.sortingOrder = 1;
    }

    public void SetGridPosition(Vector2Int position)
    {
        GridPosition = position;
    }

    void OnMouseDown()
    {
        if (GridPosition.x >= 0 && GridPosition.y >= 0)
        {
            GridManager gridManager = FindObjectOfType<GridManager>();
            if (gridManager != null)
            {
                gridManager.HandleCubeClick(GridPosition.x, GridPosition.y);
            }
        }
    }

    public void AnimateCollection(Vector3 targetPosition)
    {
        // Move towards goal UI element
        StartCoroutine(AnimateToTarget(targetPosition));
    }

    private System.Collections.IEnumerator AnimateToTarget(Vector3 target)
    {
        Vector3 startPos = transform.position;
        float duration = 0.8f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, target, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
