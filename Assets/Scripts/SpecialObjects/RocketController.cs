using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : MonoBehaviour
{
    public enum RocketDirection
    {
        Horizontal,
        Vertical
    }

    [Header("Rocket Settings")]
    public RocketDirection direction;

    private SpriteRenderer spriteRenderer;
    private Collider2D rocketCollider;
    private GridManager gridManager;
    private GameConfig config;
    public Vector2Int GridPosition { get; private set; }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rocketCollider = GetComponent<Collider2D>();
        gridManager = FindObjectOfType<GridManager>();
        config = GameManager.Instance.gameConfig;
    }

    public void Initialize(RocketDirection dir, Vector2Int gridPos)
    {
        direction = dir;
        GridPosition = gridPos;

        if (direction == RocketDirection.Horizontal)
        {
            spriteRenderer.sprite = config.rocketHorizontalSprite;
        }
        else
        {
            spriteRenderer.sprite = config.rocketVerticalSprite;
        }

        spriteRenderer.sortingLayerName = "Default";
        spriteRenderer.sortingOrder = 6;
        spriteRenderer.color = Color.white;

        if (rocketCollider != null)
        {
            rocketCollider.enabled = true;
            rocketCollider.isTrigger = false;

            if (rocketCollider is BoxCollider2D boxCollider)
            {
                boxCollider.size = Vector2.one;
            }
        }

        gameObject.name = $"Rocket_{direction}_{gridPos.x}_{gridPos.y}";

        Debug.Log($"Rocket initialized: {direction} at {gridPos}");
    }

    public void SetGridPosition(Vector2Int position)
    {
        GridPosition = position;
    }

    void OnMouseDown()
    {
        Debug.Log($"Rocket clicked: {gameObject.name} at {GridPosition}");

        if (gridManager != null)
        {
            gridManager.HandleCubeClick(GridPosition.x, GridPosition.y);
        }
    }

    public IEnumerator AnimateTravel(Vector3 targetPosition)
    {
        if (rocketCollider != null)
        {
            rocketCollider.enabled = false;
        }

        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);

        if (config.rocketMoveSpeed <= 0)
        {
            Debug.LogWarning("Rocket move speed is zero or less. Snapping to target.");
            transform.position = targetPosition;
            Destroy(gameObject);
            yield break;
        }

        float duration = distance / config.rocketMoveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        Destroy(gameObject);
    }

    void OnMouseEnter()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1.2f, 1.2f, 1.2f, 1f);
        }
    }

    void OnMouseExit()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }
}
