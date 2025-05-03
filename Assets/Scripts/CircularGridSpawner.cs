using UnityEngine;

public class CircularGridSpawner : MonoBehaviour
{
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cellSize = 1f;
    public float circleRadius = 8f;
    public float minRadius = 2f;

    public GridCell cellPrefab;

    private GridCell[,] gridCells;



    readonly Vector2Int dirLeft = new Vector2Int(-1, 0);
    readonly Vector2Int dirRight = new Vector2Int(1, 0);
    readonly Vector2Int dirUp = new Vector2Int(0, 1);
    readonly Vector2Int dirDown = new Vector2Int(0, -1);

    void Start ()
    {
        gridCells = new GridCell[gridWidth, gridHeight];
        Vector2 center = new Vector2(gridWidth / 2f, gridHeight / 2f);
        GameObject container = Instantiate(new GameObject(), transform);
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
                else if(dist * cellSize < minRadius)
                {
                    gridCells[x, y].Set_SpawnArea();
                }
                else
                {
                    gridCells[x, y].Set_OutsideRadius();
                }
                gridCells[x, y].transform.SetParent(container.transform);
            }
        }

        // Set outer circle
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int cellPos = new Vector2Int(x, y);
                //if (gridCells[x, y].IsSpawnArea) continue;
                if(!AllNeighboursExist(cellPos) && !AnyNeighbourAdjacentToSpawnArea(cellPos))
                {
                    gridCells[x,y].SetCircleIndex(0);
                }
            }
        }
    }
    private bool AllNeighboursExist(Vector2Int cellPos)
    {
        return HasNeighbour(cellPos, dirLeft) && HasNeighbour(cellPos, dirRight) && 
            HasNeighbour(cellPos, dirUp) && HasNeighbour(cellPos, dirDown);
    }
    
    private bool HasNeighbour(Vector2Int cellPos, Vector2Int direction)
    {
        Vector2Int currentPos = cellPos + direction;

        return currentPos.x >= 0 && currentPos.y >= 0 && currentPos.x < gridWidth && currentPos.y < gridHeight &&
            gridCells[currentPos.x, currentPos.y].IsInsideRaius;
    }
    private bool AnyNeighbourAdjacentToSpawnArea (Vector2Int cellPos)
    {
        return IsAdjacentToSpawnArea(cellPos, dirLeft) && IsAdjacentToSpawnArea(cellPos, dirRight) &&
            IsAdjacentToSpawnArea(cellPos, dirUp) && IsAdjacentToSpawnArea(cellPos, dirDown);
    }
    private bool IsAdjacentToSpawnArea (Vector2Int cellPos, Vector2Int direction)
    {
        Vector2Int currentPos = cellPos + direction;

        return currentPos.x >= 0 && currentPos.y >= 0 && currentPos.x < gridWidth && currentPos.y < gridHeight &&
            gridCells[currentPos.x, currentPos.y].IsSpawnArea;
    }
}