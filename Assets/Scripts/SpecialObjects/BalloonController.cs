using UnityEngine;

public class BalloonController : MonoBehaviour
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
        spriteRenderer.sprite = GameManager.Instance.gameConfig.balloonSprite;
        transform.position = gridManager.GridToWorldPosition(gridPos.x, gridPos.y);
    }

    public void ExplodeBalloon()
    {
        // Play balloon sound and effects
        GameManager.Instance.audioManager.PlayBalloonSound();
        GameManager.Instance.effectsManager.PlayExplosionEffect(transform.position);

        // Remove from grid if needed
        GridCell cell = gridManager.GetCell(GridPosition.x, GridPosition.y);
        if (cell != null)
        {
            // If balloon counts as goal, collect it
            // GameManager.Instance.CollectBalloon();
        }

        Destroy(gameObject);
    }
}
