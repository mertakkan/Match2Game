using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Setup")]
    public Transform gridParent;
    public GameObject cubePrefab;

    public GameObject duckPrefab;

    public GameObject balloonPrefab;

    private GridCell[,] grid;
    private GameConfig config;
    private Camera mainCamera;
    private bool isProcessingMatches = false;

    public int Width => config.gridWidth;
    public int Height => config.gridHeight;
    private const float VerticalOverlapFactor = 1f;

    void Awake()
    {
        mainCamera = Camera.main;

        if (cubePrefab == null)
        {
            CreateCubePrefab();
        }

        if (balloonPrefab == null)
            CreateBalloonPrefab();

        if (duckPrefab == null)
        {
            CreateDuckPrefab();
        }
    }

    void CreateCubePrefab()
    {
        cubePrefab = new GameObject("CubePrefab");
        SpriteRenderer sr = cubePrefab.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 0;
        sr.color = new Color(1f, 1f, 1f, 1f);
        sr.material = new Material(Shader.Find("Sprites/Default"));
        BoxCollider2D collider = cubePrefab.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        cubePrefab.AddComponent<CubeController>();
        cubePrefab.SetActive(false);
        DontDestroyOnLoad(cubePrefab);
    }

    void CreateDuckPrefab()
    {
        duckPrefab = new GameObject("DuckPrefab");
        SpriteRenderer sr = duckPrefab.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 5;
        sr.color = Color.white;
        duckPrefab.AddComponent<DuckController>();
        duckPrefab.SetActive(false);
        DontDestroyOnLoad(duckPrefab);
    }

    void CreateBalloonPrefab()
    {
        balloonPrefab = new GameObject("BalloonPrefab");
        SpriteRenderer sr = balloonPrefab.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 5;
        balloonPrefab.AddComponent<BalloonController>();
        balloonPrefab.SetActive(false);
        DontDestroyOnLoad(balloonPrefab);
    }

    public void InitializeGrid()
    {
        config = GameManager.Instance.gameConfig;
        if (config == null)
        {
            Debug.LogError("GameConfig is null!");
            return;
        }

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

        SpawnInitialDucks();

        SpawnInitialBalloons();

        CheckAndCollectBottomRowDucks();
    }

    void SetupCameraAndGrid()
    {
        float totalWidth = Width * config.cellSize + (Width - 1) * config.gridSpacing;
        float verticalStep = config.cellSize * VerticalOverlapFactor;
        float totalHeight = ((Height - 1) * verticalStep) + config.cellSize;
        Vector3 gridOffset = new Vector3(
            -totalWidth * 0.5f + config.cellSize * 0.5f,
            -totalHeight * 0.5f + config.cellSize * 0.5f,
            0
        );

        if (gridParent != null)
        {
            gridParent.position = gridOffset;
        }
        float requiredSize = Mathf.Max(totalHeight * 0.7f, totalWidth * 0.6f / mainCamera.aspect);
        mainCamera.orthographicSize = requiredSize;
        mainCamera.transform.position = new Vector3(0, 0, -10f);
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

    void SpawnInitialDucks()
    {
        int ducksToPlace = config.ducksInLevel;
        if (ducksToPlace <= 0)
            return;

        List<Vector2Int> availableCells = new List<Vector2Int>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 1; y < Height; y++)
            {
                if (grid[x, y].isEmpty || grid[x, y].hasCube)
                {
                    availableCells.Add(new Vector2Int(x, y));
                }
            }
        }

        for (int i = 0; i < ducksToPlace; i++)
        {
            if (availableCells.Count == 0)
            {
                Debug.LogWarning("Not enough available cells to place all ducks.");
                break;
            }

            int randomIndex = Random.Range(0, availableCells.Count);
            Vector2Int cellPos = availableCells[randomIndex];
            availableCells.RemoveAt(randomIndex);

            if (grid[cellPos.x, cellPos.y].hasCube)
            {
                Destroy(grid[cellPos.x, cellPos.y].cube.gameObject);
                grid[cellPos.x, cellPos.y].ClearCube();
            }

            DuckController newDuck = CreateDuck(cellPos.x, cellPos.y);
        }
    }

    DuckController CreateDuck(int x, int y)
    {
        if (duckPrefab == null)
        {
            Debug.LogError("DuckPrefab is not set!");
            return null;
        }
        GameObject duckObj = Instantiate(duckPrefab, gridParent);
        duckObj.SetActive(true);

        SpriteRenderer sr = duckObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = y;
        }

        DuckController duck = duckObj.GetComponent<DuckController>();
        duck.Initialize(new Vector2Int(x, y));

        Vector3 worldPos = GridToWorldPosition(x, y);
        duck.transform.position = worldPos;

        grid[x, y].SetDuck(duck);

        return duck;
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

        SpriteRenderer sr = cubeObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = y;
        }

        int randomColorIndex = Random.Range(0, config.cubeSprites.Length);
        cube.Initialize(randomColorIndex, config.cubeSprites[randomColorIndex]);

        Vector3 worldPos = GridToWorldPosition(x, y);
        cube.transform.position = worldPos;

        cube.SetGridPosition(new Vector2Int(x, y));
        grid[x, y].SetCube(cube);
    }

    public Vector3 GridToWorldPosition(int x, int y)
    {
        float worldX = x * (config.cellSize + config.gridSpacing);
        float worldY = y * (config.cellSize * VerticalOverlapFactor);
        return gridParent.position + new Vector3(worldX, worldY, 0);
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - gridParent.position;
        int x = Mathf.RoundToInt(localPos.x / (config.cellSize + config.gridSpacing));
        int y = Mathf.RoundToInt(localPos.y / (config.cellSize * VerticalOverlapFactor));
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

        if (clickedCell.hasRocket)
        {
            if (GameManager.Instance.TryMakeMove())
            {
                StartCoroutine(ProcessRocketActivation(clickedCell));
            }
            return;
        }

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

        rocketCell.ClearRocket();

        Vector3 targetPosition;
        if (rocket.direction == RocketController.RocketDirection.Horizontal)
        {
            targetPosition = GridToWorldPosition(-1, rocketGridPos.y);
        }
        else
        {
            targetPosition = GridToWorldPosition(rocketGridPos.x, Height);
        }

        yield return StartCoroutine(rocket.AnimateTravel(targetPosition));

        List<GridCell> cellsToDestroy = new List<GridCell>();
        if (rocket.direction == RocketController.RocketDirection.Horizontal)
        {
            for (int ix = 0; ix < Width; ix++)
            {
                cellsToDestroy.Add(GetCell(ix, rocketGridPos.y));
            }
        }
        else
        {
            for (int iy = 0; iy < Height; iy++)
            {
                cellsToDestroy.Add(GetCell(rocketGridPos.x, iy));
            }
        }

        foreach (var cell in cellsToDestroy)
        {
            if (cell == null)
                continue;

            if (cell.hasCube)
            {
                GameManager.Instance.CollectCube(cell.cube.ColorIndex, 1);
                GameManager.Instance.effectsManager.PlayExplosionEffect(
                    cell.cube.transform.position,
                    cell.cube.ColorIndex
                );
                Destroy(cell.cube.gameObject);
                cell.ClearCube();
            }
            if (cell.hasRocket)
            {
                GameManager.Instance.effectsManager.PlayExplosionEffect(
                    cell.rocket.transform.position
                );
                Destroy(cell.rocket.gameObject);
                cell.ClearRocket();
            }
        }

        GameManager.Instance.audioManager.PlayExplosionSound();
        yield return new WaitForSeconds(config.explosionDuration);

        yield return StartCoroutine(ApplyGravity());

        CheckAndCollectBottomRowDucks();

        yield return StartCoroutine(FillEmptySpaces());

        UpdateAllObjectPositions();

        isProcessingMatches = false;
    }

    void SpawnInitialBalloons()
    {
        int balloonsToPlace = config.balloonsInLevel;
        if (balloonsToPlace <= 0)
            return;

        List<Vector2Int> availableCells = new List<Vector2Int>();
        for (int x = 0; x < Width; x++)
        {
            for (int y = Height / 2; y < Height; y++)
            {
                if (grid[x, y].isEmpty || grid[x, y].hasCube)
                {
                    availableCells.Add(new Vector2Int(x, y));
                }
            }
        }

        for (int i = 0; i < balloonsToPlace; i++)
        {
            if (availableCells.Count == 0)
                break;

            int randomIndex = Random.Range(0, availableCells.Count);
            Vector2Int cellPos = availableCells[randomIndex];
            availableCells.RemoveAt(randomIndex);

            if (grid[cellPos.x, cellPos.y].hasCube)
            {
                Destroy(grid[cellPos.x, cellPos.y].cube.gameObject);
                grid[cellPos.x, cellPos.y].ClearCube();
            }
            CreateBalloon(cellPos.x, cellPos.y);
        }
    }

    private void CheckAndCollectBottomRowDucks()
    {
        for (int x = 0; x < Width; x++)
        {
            GridCell cell = grid[x, 0];
            if (cell != null && cell.hasDuck && cell.duck != null)
            {
                DuckController duck = cell.duck;
                cell.ClearDuck();
                duck.CollectDuck();
            }
        }
    }

    void CreateBalloon(int x, int y)
    {
        if (balloonPrefab == null)
            return;

        GameObject balloonObj = Instantiate(balloonPrefab, gridParent);
        balloonObj.SetActive(true);

        SpriteRenderer sr = balloonObj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = y;

        BalloonController balloon = balloonObj.GetComponent<BalloonController>();
        balloon.Initialize(new Vector2Int(x, y));
        balloon.transform.position = GridToWorldPosition(x, y);
        grid[x, y].SetBalloon(balloon);
    }

    IEnumerator ProcessMatches(List<GridCell> matches, Vector2Int clickPosition)
    {
        isProcessingMatches = true;

        int colorIndex = matches[0].cube.ColorIndex;
        GameManager.Instance.CollectCube(colorIndex, matches.Count);

        bool shouldCreateRocket = matches.Count >= config.rocketTriggerSize;
        Vector3 rocketWorldPosition = GridToWorldPosition(clickPosition.x, clickPosition.y);

        List<GridCell> adjacentBalloons = new List<GridCell>();

        foreach (var matchCell in matches)
        {
            Vector2Int[] adjacentPositions =
            {
                new Vector2Int(matchCell.position.x + 1, matchCell.position.y),
                new Vector2Int(matchCell.position.x - 1, matchCell.position.y),
                new Vector2Int(matchCell.position.x, matchCell.position.y + 1),
                new Vector2Int(matchCell.position.x, matchCell.position.y - 1)
            };

            foreach (var pos in adjacentPositions)
            {
                GridCell adjacentCell = GetCell(pos.x, pos.y);
                if (
                    adjacentCell != null
                    && adjacentCell.hasBalloon
                    && !adjacentBalloons.Contains(adjacentCell)
                )
                {
                    adjacentBalloons.Add(adjacentCell);
                }
            }
        }
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

        foreach (var balloonCell in adjacentBalloons)
        {
            if (balloonCell.balloon != null)
            {
                balloonCell.balloon.Explode();
                balloonCell.ClearBalloon();
            }
        }

        GameManager.Instance.audioManager.PlayExplosionSound();
        yield return new WaitForSeconds(config.explosionDuration);

        if (shouldCreateRocket)
        {
            CreateRocket(clickPosition, rocketWorldPosition);
        }

        yield return StartCoroutine(ApplyGravity());

        CheckAndCollectBottomRowDucks();

        yield return StartCoroutine(FillEmptySpaces());

        UpdateAllObjectPositions();

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
        sr.sortingOrder = 6;

        BoxCollider2D col = rocketObj.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        RocketController rocket = rocketObj.AddComponent<RocketController>();
        rocket.Initialize(direction, gridPosition);
        rocketObj.transform.position = worldPosition;

        GridCell cell = GetCell(gridPosition.x, gridPosition.y);
        if (cell != null)
        {
            cell.SetRocket(rocket);
        }
    }

    void UpdateAllObjectPositions()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                GridCell cell = grid[x, y];
                Vector3 correctPosition = GridToWorldPosition(x, y);

                if (cell.hasCube && cell.cube != null)
                {
                    cell.cube.SetGridPosition(new Vector2Int(x, y));
                    cell.cube.transform.position = correctPosition;
                }

                if (cell.hasBalloon && cell.balloon != null)
                {
                    cell.balloon.SetGridPosition(new Vector2Int(x, y));
                    cell.balloon.transform.position = correctPosition;
                }
                if (cell.hasRocket && cell.rocket != null)
                {
                    cell.rocket.SetGridPosition(new Vector2Int(x, y));
                    cell.rocket.transform.position = correctPosition;
                }

                if (cell.hasDuck && cell.duck != null)
                {
                    cell.duck.SetGridPosition(new Vector2Int(x, y));
                    cell.duck.transform.position = correctPosition;
                }
            }
        }
    }

    IEnumerator ApplyGravity()
    {
        bool itemsMoved = true;
        while (itemsMoved)
        {
            itemsMoved = false;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height - 1; y++)
                {
                    GridCell currentCell = grid[x, y];

                    if (!currentCell.isEmpty)
                        continue;

                    for (int yAbove = y + 1; yAbove < Height; yAbove++)
                    {
                        GridCell aboveCell = grid[x, yAbove];

                        if (aboveCell.hasDuck)
                        {
                            DuckController duckToMove = aboveCell.duck;
                            currentCell.SetDuck(duckToMove);
                            aboveCell.ClearDuck();

                            SpriteRenderer sr = duckToMove.GetComponent<SpriteRenderer>();
                            if (sr != null)
                                sr.sortingOrder = y;

                            StartCoroutine(
                                AnimateObjectFall(duckToMove.transform, GridToWorldPosition(x, y))
                            );
                            itemsMoved = true;

                            break;
                        }

                        if (aboveCell.hasBalloon)
                        {
                            BalloonController balloonToMove = aboveCell.balloon;
                            currentCell.SetBalloon(balloonToMove);
                            aboveCell.ClearBalloon();

                            SpriteRenderer sr = balloonToMove.GetComponent<SpriteRenderer>();
                            if (sr != null)
                                sr.sortingOrder = y;

                            StartCoroutine(
                                AnimateObjectFall(
                                    balloonToMove.transform,
                                    GridToWorldPosition(x, y)
                                )
                            );
                            itemsMoved = true;
                            break;
                        }

                        if (aboveCell.hasCube)
                        {
                            CubeController cubeToMove = aboveCell.cube;
                            currentCell.SetCube(cubeToMove);
                            aboveCell.ClearCube();

                            SpriteRenderer sr = cubeToMove.GetComponent<SpriteRenderer>();
                            if (sr != null)
                                sr.sortingOrder = y;

                            StartCoroutine(
                                AnimateObjectFall(cubeToMove.transform, GridToWorldPosition(x, y))
                            );
                            itemsMoved = true;
                            break;
                        }
                    }
                }
            }
            if (itemsMoved)
            {
                yield return new WaitForSeconds(0.1f);
            }
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

                    if (cell.hasCube && cell.cube != null)
                    {
                        Vector3 startPos = GridToWorldPosition(x, Height);
                        Vector3 endPos = GridToWorldPosition(x, y);
                        cell.cube.transform.position = startPos;
                        StartCoroutine(AnimateObjectFall(cell.cube.transform, endPos));
                    }
                }
            }
        }
        yield return new WaitForSeconds(config.cubeFallSpeed / 5f);
    }

    IEnumerator AnimateObjectFall(Transform objTransform, Vector3 targetPosition)
    {
        if (objTransform == null)
            yield break;

        Vector3 startPosition = objTransform.position;
        float journey = 0f;
        float speed = config.cubeFallSpeed;

        while (journey <= 1f && objTransform != null)
        {
            journey += Time.deltaTime * speed;
            objTransform.position = Vector3.Lerp(startPosition, targetPosition, journey);
            yield return null;
        }

        if (objTransform != null)
        {
            objTransform.position = targetPosition;
        }
    }
}
