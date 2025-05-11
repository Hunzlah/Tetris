using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-1)]
public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }

    public TileBase boardTile; // Assign your tile in the Inspector
    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(20, 20);
    public Vector3Int spawnPosition = new Vector3Int(0, 0, 0); // Center spawn

    [SerializeField]
    public int outerRadiusSquared = 196; // Radius ~14

    [SerializeField]
    private GameObject piecePrefab; // Assign a Piece prefab in the Inspector

    private float gridRotation = 0f; // Current rotation of the grid in degrees (0, 90, -90, etc.)
    public float GridRotation => gridRotation; // Public getter for gridRotation

    private int score = 0; // For tracking score when clearing rings

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
            for (int x = -outerRadius - 1; x <= outerRadius + 1; x++)
            {
                for (int y = -outerRadius - 1; y <= outerRadius + 1; y++)
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
        // Find the Tilemap
        tilemap = GetComponentInChildren<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("Tilemap component not found in children of Board!");
            enabled = false;
            return;
        }

        // Check for existing Piece
        activePiece = GetComponentInChildren<Piece>();

        // Validate boardTile
        if (boardTile == null)
        {
            Debug.LogError("Board Tile not assigned in the Inspector!");
            enabled = false;
            return;
        }

        // Validate tetrominoes
        if (tetrominoes == null || tetrominoes.Length == 0)
        {
            Debug.LogError("Tetrominoes array is empty or not assigned!");
            enabled = false;
            return;
        }

        // Validate piecePrefab
        if (piecePrefab == null)
        {
            Debug.LogError("Piece Prefab not assigned in the Inspector!");
            enabled = false;
            return;
        }

        // If no Piece exists, instantiate one
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

                // Parent the Piece to the Board
                pieceObj.transform.SetParent(transform, false);

                // Get the Piece component
                activePiece = pieceObj.GetComponent<Piece>();
                if (activePiece == null)
                {
                    Debug.LogError("Instantiated Piece prefab does not have a Piece component!");
                    Destroy(pieceObj); // Clean up the instantiated object
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

        // Verify the hierarchy
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

        // Initialize tetrominoes
        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }

        CreateShapedBoard();
        UpdateTilemapRotation();

        // Validate spawn position
        bool spawnValid = false;
        if (IsValidBoardPosition(spawnPosition))
        {
            TetrominoData testData = tetrominoes[0];
            activePiece.Initialize(this, spawnPosition, testData);
            spawnValid = IsValidPosition(activePiece, spawnPosition);
        }

        if (!spawnValid)
        {
            spawnPosition = FindValidSpawnPosition();
            if (spawnPosition == Vector3Int.zero && !IsValidBoardPosition(spawnPosition))
            {
                Debug.LogError("No valid spawn position found! Check board parameters and tetromino cells.");
            }
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    private void Update()
    {
        // Handle grid rotation input
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            RotateGrid(-90f); // Counterclockwise
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            RotateGrid(90f); // Clockwise
        }
    }

    private void RotateGrid(float angleDelta)
    {
        // Update grid rotation
        float previousRotation = gridRotation;
        gridRotation += angleDelta;

        // Update Tilemap rotation
        UpdateTilemapRotation();

        // Adjust active piece position to match the new grid rotation
        if (activePiece != null)
        {
            activePiece.AdjustPositionForGridRotation(previousRotation, gridRotation);
        }
    }

    private void UpdateTilemapRotation()
    {
        // Ensure Tilemap GameObject's position is centered at (0, 0, 0)
        tilemap.transform.position = Vector3.zero;
        tilemap.transform.rotation = Quaternion.Euler(0f, 0f, gridRotation);
    }

    public Vector3Int TransformToGridCoordinates(Vector3Int worldPosition)
    {
        // Rotate the position by the inverse of the grid rotation to get grid coordinates
        float angleRad = -gridRotation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        int x = Mathf.RoundToInt(worldPosition.x * cos + worldPosition.y * sin);
        int y = Mathf.RoundToInt(-worldPosition.x * sin + worldPosition.y * cos);
        return new Vector3Int(x, y, worldPosition.z);
    }

    public Vector3Int TransformToWorldCoordinates(Vector3Int gridPosition)
    {
        // Rotate the position by the grid rotation to get world coordinates
        float angleRad = gridRotation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        int x = Mathf.RoundToInt(gridPosition.x * cos + gridPosition.y * sin);
        int y = Mathf.RoundToInt(-gridPosition.x * sin + gridPosition.y * cos);
        return new Vector3Int(x, y, gridPosition.z);
    }

    public Vector2Int TransformDirectionToWorld(Vector2Int gridDirection)
    {
        // Rotate the direction by the grid rotation to get world direction
        float angleRad = gridRotation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        int x = Mathf.RoundToInt(gridDirection.x * cos + gridDirection.y * sin);
        int y = Mathf.RoundToInt(-gridDirection.x * sin + gridDirection.y * cos); // Fixed: Changed gridPosition to gridDirection
        return new Vector2Int(x, y);
    }

    public Vector2Int TransformWorldDirectionToGrid(Vector2Int worldDirection)
    {
        // Rotate the world direction by the inverse of the grid rotation to get grid direction
        float angleRad = -gridRotation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        int x = Mathf.RoundToInt(worldDirection.x * cos + worldDirection.y * sin);
        int y = Mathf.RoundToInt(-worldDirection.x * sin + worldDirection.y * cos);

        // Adjust directions to match player expectation in world space
        if (worldDirection == new Vector2Int(0, -1)) // Downward movement
        {
            x = -x; // Flip the x-component to correct the falling direction
        }
        else if (worldDirection == new Vector2Int(-1, 0) || worldDirection == new Vector2Int(1, 0)) // Left or right movement
        {
            float rotationMod = Mathf.Abs(gridRotation % 360);
            if (Mathf.Approximately(rotationMod, 90) || Mathf.Approximately(rotationMod, 270))
            {
                y = -y; // Flip the y-component to correct left/right movement at 90 and -90 degrees
            }
        }

        return new Vector2Int(x, y);
    }

    private void CreateShapedBoard()
    {
        tilemap.ClearAllTiles();
        RectInt bounds = Bounds;
        Vector2Int center = new Vector2Int(0, 0);

        // Center the grid by adjusting the bounds
        int offsetX = (bounds.xMin + bounds.xMax) / 2;
        int offsetY = (bounds.yMin + bounds.yMax) / 2;

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                // Adjust position to center the grid around (0, 0)
                Vector2Int adjustedPosition = new Vector2Int(x - offsetX, y - offsetY);
                int distanceSquared = (adjustedPosition.x * adjustedPosition.x) + (adjustedPosition.y * adjustedPosition.y);
                bool withinOuterCircle = distanceSquared <= outerRadiusSquared;

                if (withinOuterCircle)
                {
                    tilemap.SetTile((Vector3Int)adjustedPosition, boardTile);
                }
            }
        }
    }

    private Vector3Int FindValidSpawnPosition()
    {
        int outerRadius = Mathf.CeilToInt(Mathf.Sqrt(outerRadiusSquared));

        // Prioritize the center (0, 0, 0)
        Vector3Int centerPos = new Vector3Int(0, 0, 0);
        bool validForAll = true;
        foreach (var tetromino in tetrominoes)
        {
            activePiece.Initialize(this, centerPos, tetromino);
            if (!IsValidPosition(activePiece, centerPos))
            {
                validForAll = false;
                break;
            }
        }
        if (validForAll)
        {
            return centerPos;
        }

        // Fallback: Search outward from center
        for (int r = 0; r <= outerRadius; r++)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (Mathf.Abs(x) != r && Mathf.Abs(y) != r) continue;

                    int distanceSquared = x * x + y * y;
                    if (distanceSquared <= outerRadiusSquared)
                    {
                        Vector3Int pos = new Vector3Int(x, y, 0);
                        validForAll = true;
                        foreach (var tetromino in tetrominoes)
                        {
                            activePiece.Initialize(this, pos, tetromino);
                            if (!IsValidPosition(activePiece, pos))
                            {
                                validForAll = false;
                                break;
                            }
                        }
                        if (validForAll)
                        {
                            return pos;
                        }
                    }
                }
            }
        }

        Debug.LogError("Failed to find a valid spawn position. Check grid parameters and tetromino cells.");
        return Vector3Int.zero;
    }

    public void SpawnPiece()
    {
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];

        activePiece.Initialize(this, spawnPosition, data);

        // Try alternative spawn positions if blocked
        Vector3Int currentSpawn = spawnPosition;
        if (!IsValidPosition(activePiece, currentSpawn))
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    Vector3Int altSpawn = spawnPosition + new Vector3Int(dx, dy, 0);
                    if (IsValidBoardPosition(altSpawn) && IsValidPosition(activePiece, altSpawn))
                    {
                        currentSpawn = altSpawn;
                        break;
                    }
                }
            }
        }

        activePiece.Initialize(this, currentSpawn, data); // Re-initialize with adjusted position

        if (IsValidPosition(activePiece, currentSpawn))
        {
            Set(activePiece);
        }
        else
        {
            Debug.LogWarning($"Game Over: Cannot spawn piece at position {currentSpawn}. Cells: {string.Join(", ", data.cells)}");
            GameOver();
        }
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles();
        CreateShapedBoard();
        UpdateTilemapRotation();
        // Add additional game over logic here
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            TileBase currentTile = tilemap.GetTile(tilePosition);
            // Only clear the piece's tile and restore the boardTile if the position is within the grid
            if (currentTile != null && currentTile != boardTile && IsValidBoardPosition(tilePosition))
            {
                tilemap.SetTile(tilePosition, boardTile);
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
            // Use the proposed position instead of the piece's current position
            Vector3Int tilePosition = piece.cells[i] + position;

            // Check circular grid boundaries
            if (!IsValidBoardPosition(tilePosition))
            {
                return false;
            }

            // Check for collisions with other pieces
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
        Dictionary<int, List<Vector3Int>> positionsByDistance = new Dictionary<int, List<Vector3Int>>();
        Dictionary<int, int> filledCountByDistance = new Dictionary<int, int>();

        // Group positions by exact radial distance (squared)
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                if (IsValidBoardPosition(position))
                {
                    int distanceSquared = (x * x) + (y * y);
                    if (!positionsByDistance.ContainsKey(distanceSquared))
                    {
                        positionsByDistance[distanceSquared] = new List<Vector3Int>();
                        filledCountByDistance[distanceSquared] = 0;
                    }
                    positionsByDistance[distanceSquared].Add(position);
                }
            }
        }

        // Check for full rings
        foreach (var kvp in positionsByDistance)
        {
            int distance = kvp.Key;
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

            Debug.Log($"Checking ring at squared distance {distance} (radius ~{Mathf.Sqrt(distance):F2}): {filledPositions}/{totalPositions} positions filled");
            Debug.Log($"Positions in ring: {string.Join(", ", positionDetails)}");

            if (filledPositions == totalPositions && filledPositions > 0)
            {
                Debug.Log($"Clearing full ring at squared distance {distance} (radius ~{Mathf.Sqrt(distance):F2}), {filledPositions}/{totalPositions} positions filled");
                foreach (var pos in positions)
                {
                    tilemap.SetTile(pos, boardTile);
                }
                filledCountByDistance[distance] = filledPositions;
                score += totalPositions * 10; // Add score based on number of positions cleared
                Debug.Log($"Score increased to {score}");
            }
        }

        // Shift rings inward
        ShiftRingsInward(positionsByDistance, filledCountByDistance);
    }

    private void ShiftRingsInward(Dictionary<int, List<Vector3Int>> positionsByDistance, Dictionary<int, int> filledCountByDistance)
    {
        var sortedDistances = positionsByDistance.Keys.OrderBy(d => d).ToList();

        foreach (var distance in sortedDistances)
        {
            if (filledCountByDistance[distance] > 0)
            {
                Debug.Log($"Shifting inward due to cleared ring at squared distance {distance} (radius ~{Mathf.Sqrt(distance):F2})");
                for (int d = distance + 1; d <= sortedDistances.Max(); d++)
                {
                    if (positionsByDistance.ContainsKey(d))
                    {
                        foreach (var pos in positionsByDistance[d])
                        {
                            TileBase tile = tilemap.GetTile(pos);
                            if (tile != null && tile != boardTile)
                            {
                                Vector2Int current = new Vector2Int(pos.x, pos.y);
                                float currentRadius = Mathf.Sqrt((current.x * current.x) + (current.y * current.y));
                                if (currentRadius > 0)
                                {
                                    float angle = Mathf.Atan2(current.y, current.x);
                                    float targetRadius = currentRadius - 1;
                                    int newX = Mathf.RoundToInt(targetRadius * Mathf.Cos(angle));
                                    int newY = Mathf.RoundToInt(targetRadius * Mathf.Sin(angle));
                                    Vector3Int newPosition = new Vector3Int(newX, newY, 0);

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
    }

    public bool IsValidBoardPosition(Vector3Int position)
    {
        Vector2Int pos2D = new Vector2Int(position.x, position.y);
        int distanceSquared = (pos2D.x * pos2D.x) + (pos2D.y * pos2D.y);
        bool withinOuterCircle = distanceSquared <= outerRadiusSquared;

        return withinOuterCircle;
    }
}