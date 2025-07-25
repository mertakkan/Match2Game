using UnityEngine;

public class DuckController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private GridManager gridManager;
    private bool isCollected = false;
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
        gameObject.name = $"Duck_{gridPos.x}_{gridPos.y}";
        isCollected = false;
    }

    public void CollectDuck()
    {
        if (isCollected || this == null || gameObject == null)
            return;

        isCollected = true;

        GameManager.Instance.audioManager.PlayDuckSound();
        GameManager.Instance.CollectDuck();
        GameManager.Instance.effectsManager.PlayExplosionEffect(transform.position);

        if (Application.isPlaying)
        {
            Destroy(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    public void SetGridPosition(Vector2Int newPos)
    {
        GridPosition = newPos;
        gameObject.name = $"Duck_{newPos.x}_{newPos.y}";
    }
}
