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
            transform.rotation = Quaternion.Euler(0, 0, 90); // Rotate for vertical
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

        if (GameManager.Instance.gameActive)
        {
            ActivateRocket();
        }
    }

    public void ActivateRocket()
    {
        Debug.Log($"Activating rocket: {direction} at {GridPosition}");

        if (GameManager.Instance.TryMakeMove())
        {
            StartCoroutine(FireRocket());
        }
    }

    IEnumerator FireRocket()
    {
        // Clear rocket from grid first
        GridCell rocketCell = gridManager.GetCell(GridPosition.x, GridPosition.y);
        if (rocketCell != null)
        {
            rocketCell.ClearRocket();
        }

        List<Vector2Int> destroyPositions = new List<Vector2Int>();

        // Determine which cubes to destroy
        if (direction == RocketDirection.Horizontal)
        {
            // Destroy entire row
            for (int x = 0; x < gridManager.Width; x++)
            {
                destroyPositions.Add(new Vector2Int(x, GridPosition.y));
            }
            Debug.Log($"Rocket destroying row {GridPosition.y}");
        }
        else
        {
            // Destroy entire column
            for (int y = 0; y < gridManager.Height; y++)
            {
                destroyPositions.Add(new Vector2Int(GridPosition.x, y));
            }
            Debug.Log($"Rocket destroying column {GridPosition.x}");
        }

        // Destroy cubes and collect them
        int totalDestroyed = 0;
        foreach (Vector2Int pos in destroyPositions)
        {
            GridCell cell = gridManager.GetCell(pos.x, pos.y);
            if (cell != null && cell.hasCube)
            {
                // Collect for goals
                GameManager.Instance.CollectCube(cell.cube.ColorIndex, 1);

                // Play explosion effect
                GameManager.Instance.effectsManager.PlayExplosionEffect(
                    cell.cube.transform.position,
                    cell.cube.ColorIndex
                );

                // Destroy cube
                Destroy(cell.cube.gameObject);
                cell.ClearCube();
                totalDestroyed++;
            }
        }

        Debug.Log($"Rocket destroyed {totalDestroyed} cubes");

        // Play sound and effects
        GameManager.Instance.audioManager.PlayExplosionSound();
        GameManager.Instance.effectsManager.PlayExplosionEffect(transform.position);

        // Destroy rocket
        Destroy(gameObject);

        // Wait a bit for destruction to complete
        yield return new WaitForSeconds(0.2f);

        // CRITICAL: Trigger immediate grid update through GridManager
        yield return StartCoroutine(gridManager.ProcessRocketDestruction());
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
