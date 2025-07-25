using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Setup")]
    public Transform gridParent;
    public GameObject cubePrefab;

    private GridCell[,] grid;
    private GameConfig config;
    private Camera mainCamera;
    private bool isProcessingMatches = false; // Prevent multiple simultaneous matches

    public int Width => config.gridWidth;
    public int Height => config.gridHeight;

    void Awake()
    {
        mainCamera = Camera.main;

        if (cubePrefab == null)
        {
            CreateCubePrefab();
        }
    }

    void CreateCubePrefab()
    {
        cubePrefab = new GameObject("CubePrefab");

        SpriteRenderer sr = cubePrefab.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 5;
        sr.color = new Color(1f, 1f, 1f, 1f);
        sr.material = new Material(Shader.Find("Sprites/Default"));

        BoxCollider2D collider = cubePrefab.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;

        cubePrefab.AddComponent<CubeController>();
        cubePrefab.SetActive(false);
        DontDestroyOnLoad(cubePrefab);
    }

    public void InitializeGrid()
    {
        config = GameManager.Instance.gameConfig;

        if (config == null)
        {
            Debug.LogError("GameConfig is null!");
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

        // Reset processing flag
        isProcessingMatches = false;

        grid = new GridCell[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y] = new GridCell(x, y);
            }
        }

        SetupCameraAndGrid();
        FillInitialGrid();
    }

    void SetupCameraAndGrid()
    {
        float totalWidth = Width * config.cellSize + (Width - 1) * config.gridSpacing;
        float totalHeight = Height * config.cellSize + (Height - 1) * config.gridSpacing;

        Vector3 gridOffset = new Vector3(
            -totalWidth * 0.5f + config.cellSize * 0.5f,
            -totalHeight * 0.5f + config.cellSize * 0.5f + 1f,
            0
        );

        if (gridParent != null)
        {
            gridParent.position = gridOffset;
        }

        float requiredSize = Mathf.Max(totalHeight * 0.8f, totalWidth * 0.6f / mainCamera.aspect);
        mainCamera.orthographicSize = requiredSize;
        mainCamera.transform.position = new Vector3(0, 1f, -10f);
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
    }

    void CreateRandomCube(int x, int y)
    {
        if (cubePrefab == null || config.cubeSprites == null || config.cubeSprites.Length == 0)
        {
            Debug.LogError("Cannot create cube - missing prefab or sprites!");
            return;
        }

        GameObject cubeObj = Instantiate(cubePrefab, gridParent);
        cubeObj.SetActive(true);
        cubeObj.name = $"Cube_{x}_{y}";

        CubeController cube = cubeObj.GetComponent<CubeController>();
        if (cube == null)
        {
            Debug.LogError("CubePrefab doesn't have CubeController component!");
            return;
        }

        int randomColorIndex = Random.Range(0, config.cubeSprites.Length);
        cube.Initialize(randomColorIndex, config.cubeSprites[randomColorIndex]);

        Vector3 worldPos = GridToWorldPosition(x, y);
        cube.transform.position = worldPos;

        // CRITICAL: Set grid position and update grid reference
        cube.SetGridPosition(new Vector2Int(x, y));
        grid[x, y].SetCube(cube);
    }

    public Vector3 GridToWorldPosition(int x, int y)
    {
        float worldX = x * (config.cellSize + config.gridSpacing);
        float worldY = y * (config.cellSize + config.gridSpacing);
        return gridParent.position + new Vector3(worldX, worldY, 0);
    }

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
        if (startCell == null || !startCell.hasCube)
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
        if (cell == null || !cell.hasCube || cell.cube.ColorIndex != targetColor)
            return;

        visited.Add(new Vector2Int(x, y));
        matches.Add(cell);

        FloodFillMatches(x + 1, y, targetColor, matches, visited);
        FloodFillMatches(x - 1, y, targetColor, matches, visited);
        FloodFillMatches(x, y + 1, targetColor, matches, visited);
        FloodFillMatches(x, y - 1, targetColor, matches, visited);
    }

    public void HandleCubeClick(int x, int y)
    {
        if (!GameManager.Instance.gameActive || isProcessingMatches)
        {
            Debug.Log(
                $"Click ignored - Game active: {GameManager.Instance.gameActive}, Processing: {isProcessingMatches}"
            );
            return;
        }

        GridCell clickedCell = GetCell(x, y);
        if (clickedCell == null)
            return;

        // If clicking on a rocket, let the rocket handle it
        if (clickedCell.hasRocket)
        {
            clickedCell.rocket.ActivateRocket();
            return;
        }

        // Otherwise handle cube matching
        if (clickedCell.hasCube)
        {
            Debug.Log($"Cube clicked at ({x}, {y})");

            List<GridCell> matches = FindMatches(x, y);
            if (matches.Count > 0)
            {
                Debug.Log($"Found {matches.Count} matches");
                if (GameManager.Instance.TryMakeMove())
                {
                    StartCoroutine(ProcessMatches(matches, new Vector2Int(x, y)));
                }
            }
            else
            {
                Debug.Log("No matches found");
            }
        }
    }

    public IEnumerator ProcessRocketDestruction()
    {
        Debug.Log("Processing rocket destruction - applying gravity and filling");

        // Apply gravity multiple times to ensure all cubes fall properly
        yield return StartCoroutine(ApplyGravityCompletely());

        // Fill empty spaces
        yield return StartCoroutine(FillEmptySpaces());

        // Update all positions to ensure everything is correct
        UpdateAllCubePositions();

        Debug.Log("Rocket destruction processing complete");
    }

    IEnumerator ApplyGravityCompletely()
    {
        bool cubesMoved = true;
        int iterations = 0;
        int maxIterations = Height; // Prevent infinite loops

        while (cubesMoved && iterations < maxIterations)
        {
            cubesMoved = false;
            iterations++;

            for (int x = 0; x < Width; x++)
            {
                // Process each column from bottom to top
                for (int y = 0; y < Height - 1; y++)
                {
                    GridCell currentCell = grid[x, y];

                    // Skip if cell already has content or has a rocket
                    if (!currentCell.isEmpty || currentCell.hasRocket)
                        continue;

                    // Find the nearest cube above to fall down
                    for (int yAbove = y + 1; yAbove < Height; yAbove++)
                    {
                        GridCell aboveCell = grid[x, yAbove];
                        if (aboveCell.hasCube)
                        {
                            // Move cube down
                            currentCell.SetCube(aboveCell.cube);
                            aboveCell.ClearCube();

                            // Animate cube falling with faster speed for rocket aftermath
                            if (currentCell.cube != null)
                            {
                                StartCoroutine(
                                    AnimateCubeFallFast(currentCell.cube, GridToWorldPosition(x, y))
                                );
                            }

                            cubesMoved = true;
                            break;
                        }
                    }
                }
            }

            if (cubesMoved)
            {
                // Shorter wait time for faster response
                yield return new WaitForSeconds(0.05f);
            }
        }

        // Wait for animations to complete
        yield return new WaitForSeconds(0.2f);

        Debug.Log($"Gravity applied completely in {iterations} iterations");
    }

    IEnumerator ProcessMatches(List<GridCell> matches, Vector2Int clickPosition)
    {
        isProcessingMatches = true;

        int colorIndex = matches[0].cube.ColorIndex;
        GameManager.Instance.CollectCube(colorIndex, matches.Count);

        bool shouldCreateRocket = matches.Count >= config.rocketTriggerSize;

        // Store rocket position before destroying cubes
        Vector3 rocketWorldPosition = GridToWorldPosition(clickPosition.x, clickPosition.y);

        // Explode cubes with effects
        foreach (var cell in matches)
        {
            if (cell.cube != null)
            {
                Vector3 explosionPosition = cell.cube.transform.position;

                if (!float.IsNaN(explosionPosition.x) && !float.IsNaN(explosionPosition.y))
                {
                    GameManager.Instance.effectsManager.PlayExplosionEffect(
                        explosionPosition,
                        cell.cube.ColorIndex
                    );
                }

                Destroy(cell.cube.gameObject);
                cell.ClearCube();
            }
        }

        GameManager.Instance.audioManager.PlayExplosionSound();

        yield return new WaitForSeconds(config.explosionDuration);

        // Create rocket AFTER clearing the cell but BEFORE gravity
        if (shouldCreateRocket)
        {
            CreateRocket(clickPosition, rocketWorldPosition);
        }

        // Apply gravity and fill grid
        yield return StartCoroutine(ApplyGravity());
        yield return StartCoroutine(FillEmptySpaces());

        // CRITICAL: Update all cube positions after movement
        UpdateAllCubePositions();

        isProcessingMatches = false;
    }

    void CreateRocket(Vector2Int gridPosition, Vector3 worldPosition)
    {
        RocketController.RocketDirection direction =
            Random.Range(0, 2) == 0
                ? RocketController.RocketDirection.Horizontal
                : RocketController.RocketDirection.Vertical;

        GameObject rocketObj = new GameObject("Rocket");
        rocketObj.transform.SetParent(gridParent);

        SpriteRenderer sr = rocketObj.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 6; // Above cubes

        BoxCollider2D col = rocketObj.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        RocketController rocket = rocketObj.AddComponent<RocketController>();
        rocket.Initialize(direction, gridPosition);

        // Position rocket at the exact world position
        rocketObj.transform.position = worldPosition;

        // CRITICAL: Place rocket in grid system
        GridCell cell = GetCell(gridPosition.x, gridPosition.y);
        if (cell != null)
        {
            cell.SetRocket(rocket);
        }

        Debug.Log($"Created {direction} rocket at grid {gridPosition}, world {worldPosition}");
    }

    // CRITICAL: This method ensures all cubes have correct grid positions
    void UpdateAllCubePositions()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                GridCell cell = grid[x, y];

                if (cell.hasCube && cell.cube != null)
                {
                    cell.cube.SetGridPosition(new Vector2Int(x, y));
                    Vector3 correctPosition = GridToWorldPosition(x, y);
                    cell.cube.transform.position = correctPosition;
                }

                if (cell.hasRocket && cell.rocket != null)
                {
                    cell.rocket.SetGridPosition(new Vector2Int(x, y));
                    Vector3 correctPosition = GridToWorldPosition(x, y);
                    cell.rocket.transform.position = correctPosition;
                }
            }
        }
        Debug.Log("Updated all cube and rocket positions");
    }

    IEnumerator ApplyGravity()
    {
        bool cubesMoved = true;

        while (cubesMoved)
        {
            cubesMoved = false;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height - 1; y++)
                {
                    GridCell currentCell = grid[x, y];
                    // Skip if cell has a rocket
                    if (!currentCell.isEmpty || currentCell.hasRocket)
                        continue;

                    // Find cube above to fall down
                    for (int yAbove = y + 1; yAbove < Height; yAbove++)
                    {
                        GridCell aboveCell = grid[x, yAbove];
                        if (aboveCell.hasCube)
                        {
                            // Move cube down
                            currentCell.SetCube(aboveCell.cube);
                            aboveCell.ClearCube();

                            // Animate cube falling
                            if (currentCell.cube != null)
                            {
                                StartCoroutine(
                                    AnimateCubeFall(currentCell.cube, GridToWorldPosition(x, y))
                                );
                            }

                            cubesMoved = true;
                            break;
                        }
                    }
                }
            }

            if (cubesMoved)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    IEnumerator FillEmptySpaces()
    {
        // Fill from top to bottom
        for (int x = 0; x < Width; x++)
        {
            for (int y = Height - 1; y >= 0; y--)
            {
                GridCell cell = grid[x, y];
                // Only fill if cell is completely empty (no cube AND no rocket)
                if (cell.isEmpty)
                {
                    CreateRandomCube(x, y);

                    // Animate new cube falling from above
                    if (cell.hasCube && cell.cube != null)
                    {
                        Vector3 startPos = GridToWorldPosition(x, Height);
                        Vector3 endPos = GridToWorldPosition(x, y);
                        cell.cube.transform.position = startPos;
                        StartCoroutine(AnimateCubeFall(cell.cube, endPos));
                    }
                }
            }
        }

        yield return new WaitForSeconds(config.cubeFallSpeed / 5f);
    }

    IEnumerator AnimateCubeFallFast(CubeController cube, Vector3 targetPosition)
    {
        if (cube == null)
            yield break;

        Vector3 startPosition = cube.transform.position;
        float journey = 0f;
        float speed = config.cubeFallSpeed * 2f; // Faster falling for rocket aftermath

        while (journey <= 1f && cube != null)
        {
            journey += Time.deltaTime * speed;
            cube.transform.position = Vector3.Lerp(startPosition, targetPosition, journey);
            yield return null;
        }

        if (cube != null)
        {
            cube.transform.position = targetPosition;
        }
    }

    IEnumerator AnimateCubeFall(CubeController cube, Vector3 targetPosition)
    {
        if (cube == null)
            yield break;

        Vector3 startPosition = cube.transform.position;
        float journey = 0f;
        float speed = config.cubeFallSpeed;

        while (journey <= 1f && cube != null)
        {
            journey += Time.deltaTime * speed;
            cube.transform.position = Vector3.Lerp(startPosition, targetPosition, journey);
            yield return null;
        }

        if (cube != null)
        {
            cube.transform.position = targetPosition;
        }
    }

    // Update this existing method to use the improved gravity
    public IEnumerator ApplyGravityAndFill()
    {
        yield return StartCoroutine(ApplyGravityCompletely());
        yield return StartCoroutine(FillEmptySpaces());
        UpdateAllCubePositions();
    }
}
