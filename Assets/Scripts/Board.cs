using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }

    public TileBase boardTile;
    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(20, 20);

    [SerializeField]
    public int outerRadiusSquared = 196;

    [SerializeField]
    private GameObject piecePrefab;

    private float gridRotation = 0f;
    private float targetRotation = 0f;
    private bool isRotating = false;
    [SerializeField]
    private float rotationDuration = 0.5f;
    public float GridRotation => gridRotation;

    private int score = 0;
    private const float pixelsPerUnit = 192f;
    private const float pixelSize = 1f / 192f;

    private bool paused = false;
    [SerializeField]
    private GameObject pauseMenuUI;

    // Hardcoded Z value for the board and pieces

    [SerializeField]
    public int boardZLevel = -17; // Adjust this value as needed

    [SerializeField] private ScoreRuntime scoreRuntime;

    public RectInt Bounds
    {
        get
        {
            Vector2Int center = Vector2Int.zero;
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            bool foundTile = false;

            int outerRadius = Mathf.CeilToInt(Mathf.Sqrt(outerRadiusSquared));
            for (int x = -outerRadius; x <= outerRadius; x++)
            {
                for (int y = -outerRadius; y <= outerRadius; y++)
                {
                    Vector2Int currentPosition = new Vector2Int(x, y);
                    int distanceSquared = (currentPosition.x * currentPosition.x) + (currentPosition.y * currentPosition.y);
                    bool withinOuterCircle = distanceSquared <= outerRadiusSquared;

                    if (withinOuterCircle)
                    {
                        minX = Mathf.Min(minX, x);
                        maxX = Mathf.Max(maxX, x);
                        minY = Mathf.Min(minY, y);
                        maxY = Mathf.Max(maxY, y);
                        foundTile = true;
                    }
                }
            }

            if (foundTile)
            {
                return new RectInt(new Vector2Int(minX, minY), new Vector2Int(maxX - minX + 1, maxY - minY + 1));
            }
            else
            {
                return new RectInt(Vector2Int.zero, Vector2Int.one);
            }
        }
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("Tilemap component not found in children of Board!");
            enabled = false;
            return;
        }

        // Set the Tilemap's transform Z position to boardZLevel
        Vector3 tilemapPos = tilemap.transform.localPosition;
        tilemap.transform.localPosition = new Vector3(tilemapPos.x, tilemapPos.y, boardZLevel);
        Debug.Log($"Tilemap initial local position set to: {tilemap.transform.localPosition}");

        activePiece = GetComponentInChildren<Piece>();

        if (boardTile == null)
        {
            Debug.LogError("Board Tile not assigned in the Inspector!");
            enabled = false;
            return;
        }

        if (tetrominoes == null || tetrominoes.Length == 0)
        {
            Debug.LogError("Tetrominoes array is empty or not assigned!");
            enabled = false;
            return;
        }

        if (piecePrefab == null)
        {
            Debug.LogError("Piece Prefab not assigned in the Inspector!");
            enabled = false;
            return;
        }

        if (activePiece == null)
        {
            try
            {
                GameObject pieceObj = Instantiate(piecePrefab, Vector3.zero, Quaternion.identity);
                if (pieceObj == null)
                {
                    Debug.LogError("Failed to instantiate Piece prefab!");
                    enabled = false;
                    return;
                }

                pieceObj.transform.SetParent(transform, false);

                activePiece = pieceObj.GetComponent<Piece>();
                if (activePiece == null)
                {
                    Debug.LogError("Instantiated Piece prefab does not have a Piece component!");
                    Destroy(pieceObj);
                    enabled = false;
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception during Piece instantiation: {e.Message}\n{e.StackTrace}");
                enabled = false;
                return;
            }
        }

        if (activePiece == null)
        {
            Debug.LogError("activePiece is still null after instantiation! Cannot proceed.");
            enabled = false;
            return;
        }

        if (activePiece.transform.parent == tilemap.transform)
        {
            Debug.LogError("Piece is a child of Tilemap! Reparenting to Board.");
            activePiece.transform.SetParent(transform, false);
        }

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }

        CreateShapedBoard();
        UpdateTilemapRotation();

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Esc key pressed. Loading Scene 0 (Main Menu).");
            SceneManager.LoadScene(0);
            return; // Exit Update to prevent further processing after scene change
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        if (paused) return;

        if (!isRotating)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                StartCoroutine(RotateGridSmooth(-90f));
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                StartCoroutine(RotateGridSmooth(90f));
            }
        }
    }

    private void TogglePause()
    {
        paused = !paused;
        if (paused)
        {
            Debug.Log("Game Paused");
            if (pauseMenuUI != null)
            {
                pauseMenuUI.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Game Resumed");
            if (pauseMenuUI != null)
            {
                pauseMenuUI.SetActive(false);
            }
        }
    }

    private Vector3 SnapToPixelGrid(Vector3 position)
    {
        float snappedX = Mathf.Round(position.x / pixelSize) * pixelSize;
        float snappedY = Mathf.Round(position.y / pixelSize) * pixelSize;
        return new Vector3(snappedX, snappedY, position.z); // Preserve the Z value
    }

    private IEnumerator RotateGridSmooth(float angleDelta)
    {
        if (isRotating) yield break;
        isRotating = true;

        Vector3Int referenceTile = new Vector3Int(0, 0, boardZLevel);
        Vector3 worldPosBefore = tilemap.CellToWorld(referenceTile);
        Debug.Log($"Before rotation: Reference tile {referenceTile} world position: {worldPosBefore}");

        float previousRotation = gridRotation;
        targetRotation += angleDelta;

        float elapsedTime = 0f;
        Quaternion startRotation = Quaternion.Euler(0f, 0f, gridRotation);
        Quaternion endRotation = Quaternion.Euler(0f, 0f, targetRotation);

        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / rotationDuration);
            float smoothT = t * t * (3f - 2f * t);
            tilemap.transform.rotation = Quaternion.Lerp(startRotation, endRotation, smoothT);
            gridRotation = Mathf.LerpAngle(previousRotation, targetRotation, smoothT);
            tilemap.transform.position = SnapToPixelGrid(tilemap.transform.position);
            yield return null;
        }

        tilemap.transform.rotation = endRotation;
        gridRotation = targetRotation;

        UpdateTilemapRotation();
        if (activePiece != null)
        {
            activePiece.transform.position = SnapToPixelGrid(activePiece.transform.position);
            activePiece.AdjustPositionForGridRotation(previousRotation, gridRotation);
            activePiece.transform.position = SnapToPixelGrid(activePiece.transform.position);
        }

        Vector3 worldPosAfter = tilemap.CellToWorld(referenceTile);
        Debug.Log($"After rotation: Reference tile {referenceTile} world position: {worldPosAfter}, gridRotation: {gridRotation}");

        isRotating = false;
    }

    private void UpdateTilemapRotation()
    {
        // Ensure the Tilemap's Z position matches boardZLevel
        Vector3 pos = SnapToPixelGrid(Vector3.zero);
        tilemap.transform.position = new Vector3(pos.x, pos.y, boardZLevel);
        Debug.Log($"Tilemap position after rotation: {tilemap.transform.position}");
    }

    public Vector3Int TransformToGridCoordinates(Vector3Int worldPosition)
    {
        float angleRad = -gridRotation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        float x = worldPosition.x * cos + worldPosition.y * sin;
        float y = -worldPosition.x * sin + worldPosition.y * cos;

        const float tolerance = 0.001f;
        int roundedX = Mathf.Abs(x) < tolerance ? 0 : Mathf.RoundToInt(x);
        int roundedY = Mathf.Abs(y) < tolerance ? 0 : Mathf.RoundToInt(y);

        Debug.Log($"TransformToGridCoordinates: worldPosition {worldPosition}, angle {gridRotation}, cos: {cos}, sin: {sin}, x: {x}, y: {y}, rounded: ({roundedX}, {roundedY})");
        return new Vector3Int(roundedX, roundedY, worldPosition.z);
    }

    public Vector3Int TransformToWorldCoordinates(Vector3Int gridPosition)
    {
        float angleRad = gridRotation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        float x = gridPosition.x * cos + gridPosition.y * sin;
        float y = -gridPosition.x * sin + gridPosition.y * cos;

        const float tolerance = 0.001f;
        int roundedX = Mathf.Abs(x) < tolerance ? 0 : Mathf.RoundToInt(x);
        int roundedY = Mathf.Abs(y) < tolerance ? 0 : Mathf.RoundToInt(y);

        Debug.Log($"TransformToWorldCoordinates: gridPosition {gridPosition}, angle {gridRotation}, cos: {cos}, sin: {sin}, x: {x}, y: {y}, rounded: ({roundedX}, {roundedY})");
        return new Vector3Int(roundedX, roundedY, gridPosition.z);
    }

    public Vector2Int TransformDirectionToWorld(Vector2Int gridDirection)
    {
        float angleRad = gridRotation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        float x = gridDirection.x * cos + gridDirection.y * sin;
        float y = -gridDirection.x * sin + gridDirection.y * cos;

        const float tolerance = 0.001f;
        int roundedX = Mathf.Abs(x) < tolerance ? 0 : Mathf.RoundToInt(x);
        int roundedY = Mathf.Abs(y) < tolerance ? 0 : Mathf.RoundToInt(y);

        return new Vector2Int(roundedX, roundedY);
    }

    public Vector2Int TransformWorldDirectionToGrid(Vector2Int worldDirection)
    {
        float angleRad = -gridRotation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        float x = worldDirection.x * cos + worldDirection.y * sin;
        float y = -worldDirection.x * sin + worldDirection.y * cos;

        const float tolerance = 0.001f;
        int roundedX = Mathf.Abs(x) < tolerance ? 0 : Mathf.RoundToInt(x);
        int roundedY = Mathf.Abs(y) < tolerance ? 0 : Mathf.RoundToInt(y);

        if (worldDirection == new Vector2Int(0, -1))
        {
            roundedX = -roundedX;
        }
        else if (worldDirection == new Vector2Int(-1, 0) || worldDirection == new Vector2Int(1, 0))
        {
            float rotationMod = Mathf.Abs(gridRotation % 360);
            if (Mathf.Approximately(rotationMod, 90) || Mathf.Approximately(rotationMod, 270))
            {
                roundedY = -roundedY;
            }
        }

        return new Vector2Int(roundedX, roundedY);
    }

    private void CreateShapedBoard()
    {
        tilemap.ClearAllTiles();
        RectInt bounds = Bounds;
        Vector2Int center = new Vector2Int(0, 0);

        int offsetX = (bounds.xMin + bounds.xMax) / 2;
        int offsetY = (bounds.yMin + bounds.yMax) / 2;
        Debug.Log($"Bounds: minX={bounds.xMin}, maxX={bounds.xMax}, minY={bounds.yMin}, maxY={bounds.yMax}, offsetX={offsetX}, offsetY={offsetY}");

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector2Int adjustedPosition = new Vector2Int(x - offsetX, y - offsetY);
                int distanceSquared = (adjustedPosition.x * adjustedPosition.x) + (adjustedPosition.y * adjustedPosition.y);
                bool withinOuterCircle = distanceSquared <= outerRadiusSquared;

                if (withinOuterCircle)
                {
                    tilemap.SetTile(new Vector3Int(adjustedPosition.x, adjustedPosition.y, boardZLevel), boardTile);
                }
            }
        }

        Vector3Int referenceTile = new Vector3Int(0, 0, boardZLevel);
        Vector3 worldPos = tilemap.CellToWorld(referenceTile);
        Debug.Log($"After CreateShapedBoard: Reference tile {referenceTile} world position: {worldPos}");

        // Center the Tilemap and ensure its Z position matches boardZLevel
        Vector3 centerWorldPos = tilemap.CellToWorld(new Vector3Int(0, 0, boardZLevel));
        Vector3 offset = new Vector3(0, 0, boardZLevel) - centerWorldPos;
        tilemap.transform.position = offset;
        Debug.Log($"Adjusted Tilemap position to: {tilemap.transform.position}");
    }

    public void SpawnPiece()
    {
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];

        // Hardcode the spawn position directly
        Vector3Int spawnPos = new Vector3Int(0, 0, boardZLevel);
        activePiece.Initialize(this, spawnPos, data);

        Vector3Int currentSpawn = spawnPos;
        if (!IsValidPosition(activePiece, currentSpawn))
        {
            List<string> invalidPositions = new List<string>();
            foreach (var cell in activePiece.cells)
            {
                Vector3Int tilePosition = cell + currentSpawn;
                if (!IsValidBoardPosition(tilePosition))
                {
                    invalidPositions.Add($"Position {tilePosition} is outside circular bounds (distance squared: {(tilePosition.x * tilePosition.x + tilePosition.y * tilePosition.y)} > {outerRadiusSquared})");
                }
                else if (tilemap.HasTile(tilePosition) && tilemap.GetTile(tilePosition) != boardTile)
                {
                    invalidPositions.Add($"Position {tilePosition} is occupied by another piece");
                }
            }
            if (invalidPositions.Count > 0)
            {
                Debug.LogWarning($"Spawn at {currentSpawn} failed. Reasons: {string.Join("; ", invalidPositions)}");
            }

            bool foundValidPosition = false;
            for (int dy = -2; dy <= 2; dy++)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    Vector3Int altSpawn = spawnPos + new Vector3Int(dx, dy, 0);
                    altSpawn.z = boardZLevel;
                    if (IsValidBoardPosition(altSpawn) && IsValidPosition(activePiece, altSpawn))
                    {
                        currentSpawn = altSpawn;
                        foundValidPosition = true;
                        break;
                    }
                }
                if (foundValidPosition) break;
            }

            activePiece.Initialize(this, currentSpawn, data);
        }

        if (IsBoardTooFull())
        {
            Debug.LogWarning("Game Over: Board is too full (outermost radius band is nearly full).");
            GameOver();
            return;
        }

        if (IsValidPosition(activePiece, currentSpawn))
        {
            Set(activePiece);
            activePiece.transform.position = SnapToPixelGrid(activePiece.transform.position);
        }
        else
        {
            Debug.LogWarning($"Game Over: Cannot spawn piece at position {currentSpawn}. Cells: {string.Join(", ", data.cells)}");
            GameOver();
        }
    }

    private bool IsBoardTooFull()
    {
        RectInt bounds = Bounds;
        int maxRadius = Mathf.CeilToInt(Mathf.Sqrt(outerRadiusSquared));
        int outermostBand = maxRadius - 1;
        List<Vector3Int> positionsInOutermostBand = new List<Vector3Int>();
        int filledPositions = 0;

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int position = new Vector3Int(x, y, boardZLevel);
                if (IsValidBoardPosition(position))
                {
                    float radius = Mathf.Sqrt((x * x) + (y * y));
                    int radiusBand = Mathf.FloorToInt(radius);
                    if (radiusBand == outermostBand)
                    {
                        positionsInOutermostBand.Add(position);
                        TileBase tile = tilemap.GetTile(position);
                        if (tile != null && tile != boardTile)
                        {
                            filledPositions++;
                        }
                    }
                }
            }
        }

        float fillPercentage = (float)filledPositions / positionsInOutermostBand.Count;
        Debug.Log($"Outermost band (radius {outermostBand} to {outermostBand + 1}): {filledPositions}/{positionsInOutermostBand.Count} positions filled ({fillPercentage * 100:F1}% filled)");
        return fillPercentage >= 0.9f;
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles();
        CreateShapedBoard();
        UpdateTilemapRotation();
        Debug.Log("Game Over! Board cleared and reset.");
        SceneManager.LoadScene(0);
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
            Debug.Log($"Setting piece tile at position {tilePosition}");
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            TileBase currentTile = tilemap.GetTile(tilePosition);
            if (currentTile != null && currentTile != boardTile && IsValidBoardPosition(tilePosition))
            {
                tilemap.SetTile(tilePosition, boardTile);
                Debug.Log($"Clearing piece tile at position {tilePosition}, restoring boardTile");
            }
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        if (piece == null || piece.cells == null)
        {
            Debug.LogError("Piece or cells are null in IsValidPosition!");
            return false;
        }

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!IsValidBoardPosition(tilePosition))
            {
                return false;
            }

            if (tilemap.HasTile(tilePosition) && tilemap.GetTile(tilePosition) != boardTile)
            {
                return false;
            }
        }

        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = Bounds;
        Dictionary<int, List<Vector3Int>> positionsByRadiusBand = new Dictionary<int, List<Vector3Int>>();
        Dictionary<int, int> filledCountByRadiusBand = new Dictionary<int, int>();
        scoreRuntime.UpdateScore(100);

        int maxRadius = Mathf.CeilToInt(Mathf.Sqrt(outerRadiusSquared));

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int position = new Vector3Int(x, y, boardZLevel);
                if (IsValidBoardPosition(position))
                {
                    float radius = Mathf.Sqrt((x * x) + (y * y));
                    int radiusBand = Mathf.FloorToInt(radius);
                    if (!positionsByRadiusBand.ContainsKey(radiusBand))
                    {
                        positionsByRadiusBand[radiusBand] = new List<Vector3Int>();
                        filledCountByRadiusBand[radiusBand] = 0;
                    }
                    positionsByRadiusBand[radiusBand].Add(position);
                }
            }
        }

        foreach (var kvp in positionsByRadiusBand.OrderBy(k => k.Key))
        {
            int radiusBand = kvp.Key;
            var positions = kvp.Value;
            int totalPositions = positions.Count;
            int filledPositions = 0;
            List<string> positionDetails = new List<string>();

            foreach (var pos in positions)
            {
                TileBase tile = tilemap.GetTile(pos);
                bool isFilled = tile != null && tile != boardTile;
                if (isFilled)
                {
                    filledPositions++;
                }
                positionDetails.Add($"Pos {pos}: {(isFilled ? "Filled" : "Empty (boardTile)")}");
            }

            Debug.Log($"Checking radius band {radiusBand} to {radiusBand + 1} (squared distance {radiusBand * radiusBand} to {(radiusBand + 1) * (radiusBand + 1)}): {filledPositions}/{totalPositions} positions filled");
            Debug.Log($"Positions in band: {string.Join(", ", positionDetails)}");

            float fillPercentage = (float)filledPositions / totalPositions;
            if (fillPercentage >= 0.9f && filledPositions > 0)
            {
                Debug.Log($"Clearing full band at radius {radiusBand} to {radiusBand + 1}, {filledPositions}/{totalPositions} positions filled ({fillPercentage * 100:F1}% filled)");
                foreach (var pos in positions)
                {
                    tilemap.SetTile(pos, boardTile);
                }
                filledCountByRadiusBand[radiusBand] = filledPositions;
                score += totalPositions * 10;
                Debug.Log($"Score increased to {score}");
                scoreRuntime.UpdateScore(1000);
            }
        }
        ShiftRingsOutward(positionsByRadiusBand, filledCountByRadiusBand);
    }

    private void ShiftRingsOutward(Dictionary<int, List<Vector3Int>> positionsByRadiusBand, Dictionary<int, int> filledCountByRadiusBand)
    {
        var sortedRadiusBands = positionsByRadiusBand.Keys.OrderByDescending(r => r).ToList();
        
        foreach (var radiusBand in sortedRadiusBands)
        {
            if (filledCountByRadiusBand[radiusBand] > 0)
            {
                Debug.Log($"Shifting outward due to cleared band at radius {radiusBand} to {radiusBand + 1}");
                for (int r = radiusBand - 1; r >= 0; r--)
                {
                    if (positionsByRadiusBand.ContainsKey(r))
                    {
                        var positions = new List<Vector3Int>(positionsByRadiusBand[r]);
                        foreach (var pos in positions)
                        {
                            TileBase tile = tilemap.GetTile(pos);
                            if (tile != null && tile != boardTile)
                            {
                                Vector2Int current = new Vector2Int(pos.x, pos.y);
                                float currentRadius = Mathf.Sqrt((current.x * current.x) + (current.y * current.y));
                                if (currentRadius < 0.5f) continue;

                                float angle = Mathf.Atan2(current.y, current.x);
                                float targetRadius = currentRadius + 1;
                                int newX = Mathf.RoundToInt(targetRadius * Mathf.Cos(angle));
                                int newY = Mathf.RoundToInt(targetRadius * Mathf.Sin(angle));
                                Vector3Int newPosition = new Vector3Int(newX, newY, pos.z);

                                if (IsValidBoardPosition(newPosition) && (!tilemap.HasTile(newPosition) || tilemap.GetTile(newPosition) == boardTile))
                                {
                                    Debug.Log($"Moving tile from {pos} (radius {currentRadius:F2}) to {newPosition} (radius {targetRadius:F2})");
                                    tilemap.SetTile(newPosition, tile);
                                    tilemap.SetTile(pos, boardTile);
                                }
                                else
                                {
                                    Debug.Log($"Cannot move tile from {pos} to {newPosition}: {(IsValidBoardPosition(newPosition) ? "Position occupied" : "Outside grid")}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public bool IsValidBoardPosition(Vector3Int position)
    {
        Vector2Int pos2D = new Vector2Int(position.x, position.y);
        int distanceSquared = (pos2D.x * pos2D.x) + (pos2D.y * pos2D.y);
        bool withinOuterCircle = distanceSquared <= outerRadiusSquared;

        return withinOuterCircle;
    }

    public bool IsPaused => paused;
}