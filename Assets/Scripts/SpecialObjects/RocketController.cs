using System.Collections;
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
    private GridManager gridManager;
    private GameConfig config;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gridManager = FindObjectOfType<GridManager>();
        config = GameManager.Instance.gameConfig;
    }

    public void Initialize(RocketDirection dir, Vector2Int gridPos)
    {
        direction = dir;

        // Set appropriate sprite
        if (direction == RocketDirection.Horizontal)
            spriteRenderer.sprite = config.rocketHorizontalSprite;
        else
            spriteRenderer.sprite = config.rocketVerticalSprite;

        transform.position = gridManager.GridToWorldPosition(gridPos.x, gridPos.y);
    }

    void OnMouseDown()
    {
        ActivateRocket();
    }

    public void ActivateRocket()
    {
        StartCoroutine(FireRocket());
    }

    IEnumerator FireRocket()
    {
        Vector2Int gridPos = gridManager.WorldToGridPosition(transform.position);

        // Destroy cubes in rocket's path
        if (direction == RocketDirection.Horizontal)
        {
            // Destroy entire row
            for (int x = 0; x < gridManager.Width; x++)
            {
                DestroyCubeAt(x, gridPos.y);
            }
        }
        else
        {
            // Destroy entire column
            for (int y = 0; y < gridManager.Height; y++)
            {
                DestroyCubeAt(gridPos.x, y);
            }
        }

        // Play sound and effects
        GameManager.Instance.audioManager.PlayExplosionSound();
        GameManager.Instance.effectsManager.PlayExplosionEffect(transform.position);

        // Destroy rocket
        Destroy(gameObject);

        yield return new WaitForSeconds(0.5f);

        // Trigger grid refill
        yield return StartCoroutine(gridManager.ApplyGravityAndFill());
    }

    void DestroyCubeAt(int x, int y)
    {
        GridCell cell = gridManager.GetCell(x, y);
        if (cell != null && !cell.isEmpty)
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
        }
    }
}
