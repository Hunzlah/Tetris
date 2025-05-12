using UnityEngine;

public class TetrisShape : MonoBehaviour
{
    [SerializeField]
    private TetrisCell[] cellsOnGrid;
    public TetrisCell[] CellsOnGrid => cellsOnGrid;

    private Vector2Int currentPosOnGrid;
    public Vector2Int CurrentPosOnGrid => currentPosOnGrid;

    private bool isPlaced;
    public bool IsPlaced => isPlaced;

    public void SetPosOnGrid(Vector2Int _pos)
    {
        currentPosOnGrid = _pos;
    }
    public void SetIsPlaced ()
    {
        isPlaced = true;
    }
}
