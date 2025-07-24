using UnityEngine;

public class DuckController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private GridManager gridManager;
    public Vector2Int GridPosition { get; private set; }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gridManager = FindObjectOfType<GridManager>();
    }

    public void Initialize(Vector2Int gridPos)
    {
        GridPosition = gridPos;
        spriteRenderer.sprite = GameManager.Instance.gameConfig.duckSprite;
        transform.position = gridManager.GridToWorldPosition(gridPos.x, gridPos.y);
    }

    public void CheckIfAtBottom()
    {
        if (GridPosition.y == 0) // Bottom row
        {
            CollectDuck();
        }
    }

    void CollectDuck()
    {
        // Play duck sound and collection effects
        GameManager.Instance.audioManager.PlayDuckSound();

        // Collect for goals if applicable
        // GameManager.Instance.CollectDuck();

        Destroy(gameObject);
    }

    public void UpdateGridPosition(Vector2Int newPos)
    {
        GridPosition = newPos;
        CheckIfAtBottom();
    }
}
