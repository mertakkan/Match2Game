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

        // Handle rocket click
        if (clickedCell.hasRocket)
        {
            if (GameManager.Instance.TryMakeMove())
            {
                StartCoroutine(ProcessRocketActivation(clickedCell));
            }
            return;
        }

        // Handle cube click
        if (clickedCell.hasCube)
        {
            List<GridCell> matches = FindMatches(x, y);
            if (matches.Count > 0)
            {
                if (GameManager.Instance.TryMakeMove())
                {
                    StartCoroutine(ProcessMatches(matches, new Vector2Int(x, y)));
                }
            }
        }
    }

    private IEnumerator ProcessRocketActivation(GridCell rocketCell)
    {
        isProcessingMatches = true;

        RocketController rocket = rocketCell.rocket;
        Vector2Int rocketGridPos = rocket.GridPosition;

        // Clear the rocket from the grid model immediately
        rocketCell.ClearRocket();

        List<Vector2Int> destroyPositions = new List<Vector2Int>();

        // Determine which cubes to destroy based on rocket direction
        if (rocket.direction == RocketController.RocketDirection.Horizontal)
        {
            for (int x = 0; x < Width; x++)
            {
                destroyPositions.Add(new Vector2Int(x, rocketGridPos.y));
            }
        }
        else // Vertical
        {
            for (int y = 0; y < Height; y++)
            {
                destroyPositions.Add(new Vector2Int(rocketGridPos.x, y));
            }
        }

        // Destroy the rocket GameObject itself
        GameManager.Instance.effectsManager.PlayExplosionEffect(rocket.transform.position);
        Destroy(rocket.gameObject);

        // Destroy cubes and collect them
        foreach (Vector2Int pos in destroyPositions)
        {
            GridCell cell = GetCell(pos.x, pos.y);
            if (cell != null && cell.hasCube)
            {
                GameManager.Instance.CollectCube(cell.cube.ColorIndex, 1);
                GameManager.Instance.effectsManager.PlayExplosionEffect(
                    cell.cube.transform.position,
                    cell.cube.ColorIndex
                );
                Destroy(cell.cube.gameObject);
                cell.ClearCube();
            }
        }

        GameManager.Instance.audioManager.PlayExplosionSound();

        // Wait for visual destruction to be perceived
        yield return new WaitForSeconds(
            config.explosionDuration > 0.2f ? config.explosionDuration : 0.2f
        );

        // Now, run the standard grid update sequence
        yield return StartCoroutine(ApplyGravity());
        yield return StartCoroutine(FillEmptySpaces());
        UpdateAllCubePositions();

        isProcessingMatches = false;
        Debug.Log("Rocket activation processing complete.");
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
}
