using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField] private List<Color> circleColors;
    [SerializeField] private GameObject debugDirectionObj;

    private TetrisCell placedCell;
    public bool isOccupied => placedCell != null;

    private Vector2Int gridPos;
    public Vector2Int GridPos => gridPos;
    //public bool IsOccupied => isOccupied;

    private bool isInsideRadius;
    public bool IsInsideRaius=> isInsideRadius;

    private bool isSpawnArea;
    public bool IsSpawnArea => isSpawnArea;

    private bool isVisited;
    public bool IsVisited => isVisited;

    private int circleIndex;
    public int CircleIndex => circleIndex;

    private NextInDirectionCellData nextCell;
    public NextInDirectionCellData NextCell => nextCell;

    private void Awake ()
    {
        //nextCell = new NextInDirectionCellData[4];
    }
    public void SetIsOccuped (TetrisCell _cell)
    {
        placedCell = _cell;
        _cell.transform.SetParent(transform);
    }
    public void SetNextCellInDirection(Direction _direction, NextInDirectionCellData _nextCell, GridCell _cell)
    {
        nextCell = _nextCell;

        //if (gridPos == new Vector2Int(14, 14))
        //{
        //    Debug.LogError("Cell Pos: " + transform.position);
        //    Debug.LogError("Next Pos: " + _cell.transform.position);
        //}
        if (_nextCell.exists)
        {
            debugDirectionObj.SetActive(true);
            debugDirectionObj.transform.localPosition = (_cell.transform.localPosition - transform.localPosition) / 2;
        }
        else debugDirectionObj.SetActive(false);
        //directionsView[(int)_direction].SetActive(true);
    }
    public void SetGridPosition (Vector2Int _gridPos)
    {
        gridPos = _gridPos;
    }

    public void Set_InsideRadius ()
    {
        isInsideRadius = true;
    }
    public void Set_OutsideRadius ()
    {
        spriteRenderer.enabled = false;
    }
    public void Set_SpawnArea ()
    {
        isSpawnArea = true;
        isInsideRadius = true;
        spriteRenderer.color = Color.gray;
        //spriteRenderer.enabled = false;
    }
    public void SetIsVisited ()
    {
        isVisited = true;
    }
    public void SetCircleIndex(int index)
    {
        circleIndex = index;
        spriteRenderer.color = circleColors[circleIndex];
        
    }
}
public enum Direction
{
    Left, Right, Down, Up
}
public class NextInDirectionCellData
{
    public Vector2Int cellPosition;
    public bool exists;
}