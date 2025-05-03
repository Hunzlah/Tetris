using System.Collections.Generic;
using UnityEditor.Tilemaps;
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

    readonly List<Vector2Int> directions = new List<Vector2Int>()
    {
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };


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

        List<GridCell> outerCircle = new List<GridCell>();
        // Set outer circle
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int cellPos = new Vector2Int(x, y);
                if(!AllNeighboursExist(cellPos) && !AnyNeighbourAdjacentToSpawnArea(cellPos))
                {
                    gridCells[x,y].SetCircleIndex(0);
                    gridCells[x, y].SetIsVisited();

                    outerCircle.Add(gridCells[x,y]);
                }
            }
        }

        // Propagate to inner circles

        while(outerCircle.Count > 0)
        {
            outerCircle = VisitInnerCells(outerCircle);
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
            ;
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
    private GridCell GetCell(Vector2Int _pos)
    {
        return gridCells[_pos.x, _pos.y];
    }
}