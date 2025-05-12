using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public Vector3Int position { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public TetrominoData data { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float moveDelay = 0.1f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float moveTime;
    private float lockTime;
    //private bool isOnGround; // Track if the piece is touching the ground

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;

        this.stepTime = Time.time + this.stepDelay;
        this.moveTime = Time.time + this.moveDelay;
        this.lockTime = 0f;
        this.isOnGround = false;

        if (this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }

        UpdateVisualPosition();
    }

    public void AdjustPositionForGridRotation(float previousRotation, float newRotation)
    {
        // Transform the current position from the previous grid rotation to world coordinates
        Vector3Int worldPos = board.TransformToWorldCoordinates(position);

        // Transform back to the new grid rotation
        position = board.TransformToGridCoordinates(worldPos);

        // Update visual position
        UpdateVisualPosition();
    }

    private void UpdateVisualPosition()
    {
        // Convert grid position to world coordinates for rendering
        Vector3Int worldPos = board.TransformToWorldCoordinates(position);
        transform.position = new Vector3(worldPos.x, worldPos.y, worldPos.z);
    }

    private void Update()
    {
        if (this.board == null) return;

        this.board.Clear(this);

        if (Time.time > this.moveTime)
        {
            HandleMoveInputs();
        }

        if (Time.time > this.stepTime)
        {
            Step();
        }

        this.board.Set(this);
    }

    private void HandleMoveInputs()
    {
        // Rotation inputs (Q/E for counterclockwise/clockwise piece rotation)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (Rotate(-1)) // Counterclockwise
            {
                // Reset lockTime if rotation is successful
                this.lockTime = 0f;
                this.isOnGround = false; // Recheck if on ground after rotation
            }
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (Rotate(1)) // Clockwise
            {
                this.lockTime = 0f;
                this.isOnGround = false;
            }
        }

        // Movement inputs (A/D for left/right, S for soft drop, Space for hard drop)
        if (Input.GetKey(KeyCode.A))
        {
            if (Move(new Vector2Int(-1, 0))) // Move left in world space
            {
                this.lockTime = 0f; // Reset lockTime on successful lateral move
                this.isOnGround = false; // Recheck if on ground
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (Move(new Vector2Int(1, 0))) // Move right in world space
            {
                this.lockTime = 0f;
                this.isOnGround = false;
            }
        }

        if (Input.GetKey(KeyCode.S))
        {
            Move(new Vector2Int(0, -1)); // Soft drop
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
    }

    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay;

        // Always fall downward in world space (screen downward)
        bool moved = Move(new Vector2Int(0, -1));

        if (!moved)
        {
            this.isOnGround = true; // Piece has hit the ground or another piece
            this.lockTime += Time.deltaTime;

            // Lock immediately if lockTime exceeds lockDelay and no lateral movement/rotation is happening
            if (this.lockTime >= this.lockDelay || (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && !Input.GetKeyDown(KeyCode.Q) && !Input.GetKeyDown(KeyCode.E)))
            {
                Lock();
            }
        }
        else
        {
            this.isOnGround = false; // Piece is still falling
            this.lockTime = 0f; // Reset lockTime since the piece moved down
        }
    }

    private void HardDrop()
    {
        while (Move(new Vector2Int(0, -1)))
        {
            continue;
        }

        Lock();
    }

    private bool Move(Vector2Int worldTranslation)
    {
        Vector2Int gridTranslation = board.TransformWorldDirectionToGrid(worldTranslation);

        Vector3Int newPosition = this.position + (Vector3Int)gridTranslation;

        bool valid = this.board.IsValidPosition(this, newPosition);

        if (valid)
        {
            this.position = newPosition;
            this.moveTime = Time.time + this.moveDelay;
            UpdateVisualPosition();
        }
        else if (worldTranslation.y < 0) // Moving downward in world space
        {
            this.isOnGround = true; // Mark as on ground if downward move fails
        }

        return valid;
    }

    private void Lock()
    {
        if (!board.IsValidPosition(this, this.position))
        {
            //Debug.LogError($"Locking at invalid position {this.position}! This should not happen.");
        }

        this.board.Set(this);
        this.board.ClearLines();
        this.board.SpawnPiece();
    }

    private bool Rotate(int direction)
    {
        int originalRotationIndex = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);

        if (!TestWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotationIndex;
            ApplyRotationMatrix(-direction);
            return false;
        }

        UpdateVisualPosition();
        return true;
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];

            int x, y;

            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];
            Vector3Int gridTranslation = new Vector3Int(translation.x, translation.y, 0);

            Vector3Int newPosition = this.position + gridTranslation;

            if (this.board.IsValidPosition(this, newPosition))
            {
                this.position = newPosition;
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;
        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
}