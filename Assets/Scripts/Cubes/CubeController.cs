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

        spriteRenderer.sortingLayerName = "Default";
        // MODIFICATION: Removed the line that set a fixed sorting order.
        // The sorting order is now exclusively controlled by the GridManager
        // based on the cube's row (y-position).
        // spriteRenderer.sortingOrder = 5;

        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        spriteRenderer.enabled = true;

        if (spriteRenderer.material == null || spriteRenderer.material.name.Contains("Default"))
        {
            spriteRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        // Ensure collider is enabled and properly sized
        if (cubeCollider != null)
        {
            cubeCollider.enabled = true;
            cubeCollider.isTrigger = false;

            if (cubeCollider is BoxCollider2D boxCollider)
            {
                boxCollider.size = Vector2.one;
            }
        }
    }

    public void SetGridPosition(Vector2Int position)
    {
        GridPosition = position;
        gameObject.name = $"Cube_{position.x}_{position.y}_{ColorIndex}";
    }

    void OnMouseDown()
    {
        // Add debug info
        Debug.Log($"Cube clicked: {gameObject.name} at grid position {GridPosition}");

        if (GridPosition.x >= 0 && GridPosition.y >= 0)
        {
            GridManager gridManager = FindObjectOfType<GridManager>();
            if (gridManager != null)
            {
                gridManager.HandleCubeClick(GridPosition.x, GridPosition.y);
            }
            else
            {
                Debug.LogError("GridManager not found!");
            }
        }
        else
        {
            Debug.LogError($"Invalid grid position: {GridPosition}");
        }
    }

    public void AnimateCollection(Vector3 targetPosition)
    {
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

    // Add this for debugging
    void OnMouseEnter()
    {
        // Optional: Add visual feedback when hovering
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1.2f, 1.2f, 1.2f, 1f);
        }
    }

    void OnMouseExit()
    {
        // Reset color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }
    }
}
