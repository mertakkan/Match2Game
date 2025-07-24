using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Setup")]
    public Transform gridParent;
    public GameObject cubePrefab; // This should be assigned in inspector

    private GridCell[,] grid;
    private GameConfig config;
    private Camera mainCamera;

    public int Width => config.gridWidth;
    public int Height => config.gridHeight;

    void Awake()
    {
        mainCamera = Camera.main;

        // Create cube prefab if not assigned
        if (cubePrefab == null)
        {
            CreateCubePrefab();
        }
    }

    void CreateCubePrefab()
    {
        // Create prefab programmatically
        cubePrefab = new GameObject("CubePrefab");

        // Add SpriteRenderer
        SpriteRenderer sr = cubePrefab.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 1;

        // Add BoxCollider2D
        BoxCollider2D collider = cubePrefab.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;

        // Add CubeController
        cubePrefab.AddComponent<CubeController>();

        // Don't make it a child of anything - keep it as a prefab reference
        cubePrefab.SetActive(false);
        DontDestroyOnLoad(cubePrefab);
    }

    public void InitializeGrid()
    {
        config = GameManager.Instance.gameConfig;

        if (config == null)
        {
            Debug.LogError("GameConfig is null! Make sure it's assigned in GameManager.");
            return;
        }

        // Clear existing grid
        if (gridParent != null)
        {
            foreach (Transform child in gridParent)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }

        // Create grid array
        grid = new GridCell[Width, Height];

        // Initialize grid cells
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y] = new GridCell(x, y);
            }
        }

        // Position camera and grid
        SetupCameraAndGrid();

        // Fill initial grid
        FillInitialGrid();
    }

    void SetupCameraAndGrid()
    {
        // Calculate grid bounds
        float totalWidth = Width * config.cellSize + (Width - 1) * config.gridSpacing;
        float totalHeight = Height * config.cellSize + (Height - 1) * config.gridSpacing;

        // Center the grid at Z = 0 (in front of camera)
        Vector3 gridOffset = new Vector3(
            -totalWidth * 0.5f + config.cellSize * 0.5f,
            -totalHeight * 0.5f + config.cellSize * 0.5f,
            0 // Changed from -10 to 0
        );

        if (gridParent != null)
        {
            gridParent.position = gridOffset;
        }

        // Adjust camera size to fit grid with some padding
        float requiredSize = Mathf.Max(totalHeight * 0.7f, totalWidth * 0.7f / mainCamera.aspect);
        mainCamera.orthographicSize = requiredSize;

        Debug.Log($"Grid size: {Width}x{Height}, Camera size: {requiredSize}");
    }

    void FillInitialGrid()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                CreateRandomCube(x, y);
            }
        }

        Debug.Log($"Created {Width * Height} cubes");
    }

    void CreateRandomCube(int x, int y)
    {
        if (cubePrefab == null)
        {
            Debug.LogError("CubePrefab is not assigned!");
            return;
        }

        if (config.cubeSprites == null || config.cubeSprites.Length == 0)
        {
            Debug.LogError("No cube sprites assigned in GameConfig!");
            return;
        }

        // Instantiate cube
        GameObject cubeObj = Instantiate(cubePrefab, gridParent);
        cubeObj.SetActive(true); // Make sure it's active
        cubeObj.name = $"Cube_{x}_{y}";

        CubeController cube = cubeObj.GetComponent<CubeController>();

        if (cube == null)
        {
            Debug.LogError("CubePrefab doesn't have CubeController component!");
            return;
        }

        int randomColorIndex = Random.Range(0, config.cubeSprites.Length);
        cube.Initialize(randomColorIndex, config.cubeSprites[randomColorIndex]);

        // Set world position
        Vector3 worldPos = GridToWorldPosition(x, y);
        cube.transform.position = worldPos;

        // Set in grid
        grid[x, y].SetCube(cube);
    }

    public Vector3 GridToWorldPosition(int x, int y)
    {
        float worldX = x * (config.cellSize + config.gridSpacing);
        float worldY = y * (config.cellSize + config.gridSpacing);
        return gridParent.position + new Vector3(worldX, worldY, 0);
    }

    // ... rest of your methods remain the same
    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - gridParent.position;
        int x = Mathf.RoundToInt(localPos.x / (config.cellSize + config.gridSpacing));
        int y = Mathf.RoundToInt(localPos.y / (config.cellSize + config.gridSpacing));
        return new Vector2Int(x, y);
    }

    public GridCell GetCell(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
            return grid[x, y];
        return null;
    }

    public List<GridCell> FindMatches(int x, int y)
    {
        GridCell startCell = GetCell(x, y);
        if (startCell == null || startCell.isEmpty)
            return new List<GridCell>();

        List<GridCell> matches = new List<GridCell>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        FloodFillMatches(x, y, startCell.cube.ColorIndex, matches, visited);

        return matches.Count >= config.minMatchSize ? matches : new List<GridCell>();
    }

    void FloodFillMatches(
        int x,
        int y,
        int targetColor,
        List<GridCell> matches,
        HashSet<Vector2Int> visited
    )
    {
        if (visited.Contains(new Vector2Int(x, y)))
            return;

        GridCell cell = GetCell(x, y);
        if (cell == null || cell.isEmpty || cell.cube.ColorIndex != targetColor)
            return;

        visited.Add(new Vector2Int(x, y));
        matches.Add(cell);

        // Check 4 directions (horizontal and vertical only)
        FloodFillMatches(x + 1, y, targetColor, matches, visited);
        FloodFillMatches(x - 1, y, targetColor, matches, visited);
        FloodFillMatches(x, y + 1, targetColor, matches, visited);
        FloodFillMatches(x, y - 1, targetColor, matches, visited);
    }

    public void HandleCubeClick(int x, int y)
    {
        if (!GameManager.Instance.gameActive)
            return;

        List<GridCell> matches = FindMatches(x, y);
        if (matches.Count > 0)
        {
            if (GameManager.Instance.TryMakeMove())
            {
                StartCoroutine(ProcessMatches(matches, new Vector2Int(x, y)));
            }
        }
    }

    IEnumerator ProcessMatches(List<GridCell> matches, Vector2Int clickPosition)
    {
        // Collect cubes for goals
        int colorIndex = matches[0].cube.ColorIndex;
        GameManager.Instance.CollectCube(colorIndex, matches.Count);

        // Check for rocket creation
        bool shouldCreateRocket = matches.Count >= config.rocketTriggerSize;

        // Explode cubes with proper effects
        foreach (var cell in matches)
        {
            if (cell.cube != null)
            {
                GameManager.Instance.effectsManager.PlayExplosionEffect(
                    cell.cube.transform.position,
                    cell.cube.ColorIndex
                );
                Destroy(cell.cube.gameObject);
                cell.ClearCube();
            }
        }

        GameManager.Instance.audioManager.PlayExplosionSound();

        // Create rocket if needed
        if (shouldCreateRocket)
        {
            CreateRocket(clickPosition);
        }

        yield return new WaitForSeconds(config.explosionDuration);

        // Apply gravity and fill grid
        yield return StartCoroutine(ApplyGravity());
        yield return StartCoroutine(FillEmptySpaces());
    }

    void CreateRocket(Vector2Int position)
    {
        // Random direction
        RocketController.RocketDirection direction =
            Random.Range(0, 2) == 0
                ? RocketController.RocketDirection.Horizontal
                : RocketController.RocketDirection.Vertical;

        // Create rocket GameObject
        GameObject rocketObj = new GameObject("Rocket");
        rocketObj.transform.SetParent(gridParent);

        // Add components
        SpriteRenderer sr = rocketObj.AddComponent<SpriteRenderer>();
        BoxCollider2D col = rocketObj.AddComponent<BoxCollider2D>();
        RocketController rocket = rocketObj.AddComponent<RocketController>();

        // Initialize rocket
        rocket.Initialize(direction, position);

        Debug.Log($"Created {direction} rocket at {position}");
    }

    public IEnumerator ApplyGravityAndFill()
    {
        yield return StartCoroutine(ApplyGravity());
        yield return StartCoroutine(FillEmptySpaces());
    }

    IEnumerator ApplyGravity()
    {
        bool cubesMoved = false;

        // Process each column
        for (int x = 0; x < Width; x++)
        {
            // Move cubes down
            for (int y = 0; y < Height - 1; y++)
            {
                GridCell currentCell = grid[x, y];
                if (!currentCell.isEmpty)
                    continue;

                // Find cube above to fall down
                for (int yAbove = y + 1; yAbove < Height; yAbove++)
                {
                    GridCell aboveCell = grid[x, yAbove];
                    if (!aboveCell.isEmpty)
                    {
                        // Move cube down
                        currentCell.SetCube(aboveCell.cube);
                        aboveCell.ClearCube();

                        // Animate cube falling
                        StartCoroutine(
                            AnimateCubeFall(currentCell.cube, GridToWorldPosition(x, y))
                        );
                        cubesMoved = true;
                        break;
                    }
                }
            }
        }

        if (cubesMoved)
        {
            yield return new WaitForSeconds(config.cubeFallSpeed / 10f);
        }
    }

    IEnumerator FillEmptySpaces()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = Height - 1; y >= 0; y--)
            {
                GridCell cell = grid[x, y];
                if (cell.isEmpty)
                {
                    CreateRandomCube(x, y);

                    // Animate new cube falling from above
                    Vector3 startPos = GridToWorldPosition(x, Height);
                    Vector3 endPos = GridToWorldPosition(x, y);
                    cell.cube.transform.position = startPos;
                    StartCoroutine(AnimateCubeFall(cell.cube, endPos));
                }
            }
        }

        yield return new WaitForSeconds(config.cubeFallSpeed / 10f);
    }

    IEnumerator AnimateCubeFall(CubeController cube, Vector3 targetPosition)
    {
        Vector3 startPosition = cube.transform.position;
        float journey = 0f;

        while (journey <= 1f)
        {
            journey = journey + Time.deltaTime * config.cubeFallSpeed;
            cube.transform.position = Vector3.Lerp(startPosition, targetPosition, journey);
            yield return null;
        }

        cube.transform.position = targetPosition;
    }
}
