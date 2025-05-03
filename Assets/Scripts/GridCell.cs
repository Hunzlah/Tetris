using UnityEngine;

public class GridCell : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    private bool isOccupied;

    private Vector2Int gridPos;
    public Vector2Int GridPos => gridPos;
    public bool IsOccupied => isOccupied;

    private bool isInsideRadius;
    public bool IsInsideRaius=> isInsideRadius;

    private bool isSpawnArea;
    public bool IsSpawnArea => isSpawnArea;

    private bool isVisited;
    public bool IsVisited => isVisited;

    private int circleIndex;
    public int CircleIndex => circleIndex;


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

        switch(circleIndex)
        {
            case 0:
                spriteRenderer.color = Color.red;
                break;
            case 1:
                spriteRenderer.color = Color.green;
                break;
            case 2:
                spriteRenderer.color = Color.blue;
                break;
        }
    }
}
