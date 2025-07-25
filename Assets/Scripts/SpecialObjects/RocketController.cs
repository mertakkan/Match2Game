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

        // Set appropriate sprite
        if (direction == RocketDirection.Horizontal)
        {
            spriteRenderer.sprite = config.rocketHorizontalSprite;
        }
        else
        {
            spriteRenderer.sprite = config.rocketVerticalSprite;
        }

        // Set proper sorting and rendering
        spriteRenderer.sortingLayerName = "Default";
        spriteRenderer.sortingOrder = 6; // Above cubes
        spriteRenderer.color = Color.white;

        // Ensure collider is properly set up
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
            // Delegate click handling to the central GridManager
            gridManager.HandleCubeClick(GridPosition.x, GridPosition.y);
        }
    }

    /// <summary>
    /// Animates the rocket's travel to a target position and then destroys it.
    /// </summary>
    /// <param name="targetPosition">The world position to travel to.</param>
    /// <returns>IEnumerator for the coroutine.</returns>
    public IEnumerator AnimateTravel(Vector3 targetPosition)
    {
        // Disable the collider to prevent clicking it again while it's moving
        if (rocketCollider != null)
        {
            rocketCollider.enabled = false;
        }

        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);

        // Ensure rocketMoveSpeed is not zero to avoid division by zero errors
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
            // Move the rocket smoothly over time
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure it ends exactly at the target and then destroy it
        transform.position = targetPosition;
        Destroy(gameObject);
    }

    // Visual feedback on hover
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
