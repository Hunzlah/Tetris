using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;
using UnityEngine;

public class CircularGridSpawner : MonoBehaviour
{
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cellSize = 1f;
    public float circleRadius = 8f;
    public float minRadius = 2f;

    //[SerializeField] private bool rotateCells;
    //[SerializeField] private float yRotation;

    [SerializeField] private Vector2Int SpawnArea;

    public GridCell cellPrefab;

    private GridCell[,] gridCells;

    private GameObject container;
    public GameObject Container => container;

    private float[] containerRotations = new float[] { 0, 90, 180, 270 };
    private int currentContainerRotation;

    readonly List<Vector2Int> directions = new List<Vector2Int>()
    {
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    private Direction currentDirection;
    public Direction CurrentDirection => currentDirection;

    public static CircularGridSpawner Instance { get; private set; }

    private void Awake ()
    {
        Instance = this;
    }

    private void Update ()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentContainerRotation = (currentContainerRotation + 1) % containerRotations.Length;
            container.transform.eulerAngles = new Vector3(0, containerRotations[currentContainerRotation], 0);
            switch (currentContainerRotation) 
            {
                case 0:
                    currentDirection = Direction.Down;
                    break;
                case 1:
                    currentDirection = Direction.Left;
                    break;
                case 2:
                    currentDirection = Direction.Up;
                    break;
                case 3:
                    currentDirection = Direction.Right;
                    break;
            }
            SetNextCells(currentDirection);
        }
    }
    void Start ()
    {
        gridCells = new GridCell[gridWidth, gridHeight];
        Vector2 center = new Vector2(gridWidth / 2f, gridHeight / 2f);
        container = Instantiate(new GameObject(), transform);
        container.transform.position = new Vector3(gridWidth * cellSize / 2f - cellSize / 2, 0, gridHeight * cellSize / 2f - cellSize / 2);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 cellCenter = new Vector2(x + 0.5f, y + 0.5f);
                float dist = Vector2.Distance(cellCenter, center);

                Vector3 position = new Vector3(x * cellSize, 0, y * cellSize);
                gridCells[x, y] = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                gridCells[x, y].SetGridPosition(new Vector2Int(x, y));
                if (dist * cellSize <= circleRadius && dist * cellSize >= minRadius)
                {
                    gridCells[x, y].Set_InsideRadius();
                }
                //else if(dist * cellSize < minRadius)
                //{
                //    gridCells[x, y].Set_SpawnArea();
                //}
                else if(dist * cellSize > circleRadius)
                {
                    gridCells[x, y].Set_OutsideRadius();
                }
                gridCells[x, y].transform.SetParent(container.transform);
            }
        }

        Vector2Int gridCenter = new Vector2Int(gridWidth / 2, gridHeight / 2);
        Vector2Int spawnAreaStart = new Vector2Int(gridCenter.x - SpawnArea.x, gridCenter.y - SpawnArea.y);
        Vector2Int spawnAreaEnd = new Vector2Int(gridCenter.x + SpawnArea.x, gridCenter.y + SpawnArea.y);

        for (int x = spawnAreaStart.x; x < spawnAreaEnd.x; x++)
        {
            for(int y= spawnAreaStart.y; y < spawnAreaEnd.y; y++)
            {
                gridCells[x, y].Set_SpawnArea();
            }
        }


            List<GridCell> outerCircle = new List<GridCell>();
        // Set outer circle
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int cellPos = new Vector2Int(x, y);
                if (!AllNeighboursExist(cellPos) && !AnyNeighbourAdjacentToSpawnArea(cellPos))
                {
                    gridCells[x, y].SetCircleIndex(0);
                    gridCells[x, y].SetIsVisited();

                    outerCircle.Add(gridCells[x, y]);
                }
            }
        }

        // Propagate to inner circles

        while (outerCircle.Count > 0)
        {
            outerCircle = VisitInnerCells(outerCircle);
        }

        currentDirection = Direction.Down;
        SetNextCells(currentDirection);
    }

    private void SetNextCells (Direction dir)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GridCell cell = gridCells[x, y];
                // Down
                if (IsIndexInsideGrid(cell.GridPos + directions[(int)dir]) && HasNeighbour(cell.GridPos, directions[(int)dir]) &&
                    GetCell(cell.GridPos + directions[(int)dir]).IsInsideRaius
                    //&&  cell.CircleIndex > GetCell(cell.GridPos + directions[(int)Direction.Down]).CircleIndex
                    /*&& !GetCell(cell.GridPos + directions[0]).IsSpawnArea*/)
                {
                    NextInDirectionCellData downCell = new NextInDirectionCellData()
                    {
                        cellPosition = cell.GridPos + directions[(int)dir],
                        exists = true
                    };
                    cell.SetNextCellInDirection(dir, downCell, GetCell(downCell.cellPosition));
                }
                else
                {
                    NextInDirectionCellData emptyCell = new NextInDirectionCellData()
                    {
                        cellPosition = Vector2Int.zero,
                        exists = false
                    };
                    cell.SetNextCellInDirection(dir, emptyCell, null);
                }


                //// Left
                //if (IsIndexInsideGrid(cell.GridPos + directions[(int)Direction.Left]) && HasNeighbour(cell.GridPos, directions[(int)Direction.Left]) &&
                //    GetCell(cell.GridPos + directions[(int)Direction.Left]).IsInsideRaius /*&& !GetCell(cell.GridPos + directions[0]).IsSpawnArea*/)
                //{
                //    NextInDirectionCellData leftCell = new NextInDirectionCellData()
                //    {
                //        cellPosition = cell.GridPos + directions[(int)Direction.Left],
                //        exists = true
                //    };
                //    cell.SetNextCellInDirection(Direction.Left, leftCell);
                //}
                //else
                //{
                //    NextInDirectionCellData leftCell = new NextInDirectionCellData()
                //    {
                //        cellPosition = Vector2Int.zero,
                //        exists = false
                //    };
                //    cell.SetNextCellInDirection(Direction.Left, leftCell);
                //}

                //// Right
                //if (IsIndexInsideGrid(cell.GridPos + directions[(int)Direction.Right]) && HasNeighbour(cell.GridPos, directions[(int)Direction.Right]) &&
                //    GetCell(cell.GridPos + directions[(int)Direction.Right]).IsInsideRaius /*&& !GetCell(cell.GridPos + directions[0]).IsSpawnArea*/)
                //{
                //    NextInDirectionCellData leftCell = new NextInDirectionCellData()
                //    {
                //        cellPosition = cell.GridPos + directions[(int)Direction.Right],
                //        exists = true
                //    };
                //    cell.SetNextCellInDirection(Direction.Right, leftCell);
                //}
                //else
                //{
                //    NextInDirectionCellData leftCell = new NextInDirectionCellData()
                //    {
                //        cellPosition = Vector2Int.zero,
                //        exists = false
                //    };
                //    cell.SetNextCellInDirection(Direction.Right, leftCell);
                //}

                //// Up
                //if (IsIndexInsideGrid(cell.GridPos + directions[(int)Direction.Right]) && HasNeighbour(cell.GridPos, directions[(int)Direction.Right]) &&
                //    GetCell(cell.GridPos + directions[(int)Direction.Right]).IsInsideRaius /*&& !GetCell(cell.GridPos + directions[0]).IsSpawnArea*/)
                //{
                //    NextInDirectionCellData leftCell = new NextInDirectionCellData()
                //    {
                //        cellPosition = cell.GridPos + directions[(int)Direction.Right],
                //        exists = true
                //    };
                //    cell.SetNextCellInDirection(Direction.Right, leftCell);
                //}
                //else
                //{
                //    NextInDirectionCellData leftCell = new NextInDirectionCellData()
                //    {
                //        cellPosition = Vector2Int.zero,
                //        exists = false
                //    };
                //    cell.SetNextCellInDirection(Direction.Right, leftCell);
                //}
            }
        }
    }

    private List<GridCell> VisitInnerCells(List<GridCell> cells)
    {
        List<GridCell> updatedCells = new List<GridCell>();
        foreach (GridCell _cell in cells) 
        {
            Vector2Int cellPos = _cell.GridPos;
            foreach (Vector2Int direction in directions)
            {
                VisitNeighbour(cellPos + direction, _cell.CircleIndex, out GridCell updatedCell);

                if (updatedCell != null)
                {
                    updatedCells.Add(updatedCell);
                }
            }
        }

        
        return updatedCells;
    }
    private void VisitNeighbour (Vector2Int cellPos, int circleIndex, out GridCell updatedCell)
    {
        if (IsIndexInsideGrid(cellPos) && IsCellInsideRadius(cellPos) && !IsCellInSpawnArea(cellPos) &&
            !GetIsVisited(cellPos))
        {
            circleIndex++;
            SetCircleIndex(cellPos, circleIndex);
            SetIsVisited(cellPos);
            updatedCell = GetCell(cellPos);
        }
        else
        {
            updatedCell = null;
        }
    }


    private void SetCircleIndex(Vector2Int cellPos, int circleIndex)
    {
        gridCells[cellPos.x, cellPos.y].SetCircleIndex(circleIndex);
    }
    private bool GetIsVisited(Vector2Int cellPos)
    {
        return gridCells[cellPos.x, cellPos.y].IsVisited;
    }
    private void SetIsVisited(Vector2Int cellPos)
    {
        gridCells[cellPos.x, cellPos.y].SetIsVisited();
    }
    private bool AllNeighboursExist(Vector2Int cellPos)
    {
        return HasNeighbour(cellPos, directions[0]) && HasNeighbour(cellPos, directions[1]) && 
            HasNeighbour(cellPos, directions[2]) && HasNeighbour(cellPos, directions[3]);
    }
    
    private bool HasNeighbour(Vector2Int cellPos, Vector2Int direction)
    {
        Vector2Int currentPos = cellPos + direction;

        return IsIndexInsideGrid(currentPos) && IsCellInsideRadius(currentPos);
    }
    private bool AnyNeighbourAdjacentToSpawnArea (Vector2Int cellPos)
    {
        return IsAdjacentToSpawnArea(cellPos, directions[0]) && IsAdjacentToSpawnArea(cellPos, directions[1]) &&
            IsAdjacentToSpawnArea(cellPos, directions[2]) && IsAdjacentToSpawnArea(cellPos, directions[3]);
    }
    private bool IsAdjacentToSpawnArea (Vector2Int cellPos, Vector2Int direction)
    {
        Vector2Int currentPos = cellPos + direction;

        return IsIndexInsideGrid(currentPos) && IsCellInSpawnArea(currentPos);
    }

    private bool IsIndexInsideGrid(Vector2Int _pos)
    {
        return _pos.x >= 0 && _pos.y >= 0 && _pos.x < gridWidth && _pos.y < gridHeight;
    }
    private bool IsCellInSpawnArea (Vector2Int _pos)
    {
        return gridCells[_pos.x, _pos.y].IsSpawnArea;
    }
    private bool IsCellInsideRadius (Vector2Int _pos)
    {
        return gridCells[_pos.x, _pos.y].IsInsideRaius;
    }
    public GridCell GetCell(Vector2Int _pos)
    {
        return gridCells[_pos.x, _pos.y];
    }

    public bool CanShapeMoveNextInCurrentDirection(TetrisShape _shape)
    {
        bool found = true;



        return found;
    }
    public bool HasNextDestinationInCurrentDirection(Vector2Int _pos)
    {
        Vector2Int cellPos = _pos + directions[(int)currentDirection];
        return IsIndexInsideGrid(cellPos) && GetCell(_pos).NextCell.exists && !GetCell(GetCell(_pos).NextCell.cellPosition).isOccupied;
    }
    public bool CanMoveInDirection(Vector2Int _pos, Direction direction)
    {
        Vector2Int cellPos = _pos + directions[(int)direction];
        return IsIndexInsideGrid(cellPos) && GetCell(_pos).IsInsideRaius && !GetCell(cellPos).isOccupied;
    }
    public bool CanMoveShapeInDirection (TetrisShape _shape, Direction direction)
    {
        bool found = true;
        foreach(var cell in _shape.CellsOnGrid)
        {
            Vector2Int cellPos = _shape.CurrentPosOnGrid + cell.CellOffset + directions[(int)direction];
            if(!IsIndexInsideGrid(cellPos) || !GetCell(cellPos).IsInsideRaius || GetCell(cellPos).isOccupied)
            {
                found = false; 
                break;
            }
        }
        return found;
        
    }
    public bool CanRotateShape(TetrisShape _shape)
    {
        // To be implemented
        return false;
    }
}