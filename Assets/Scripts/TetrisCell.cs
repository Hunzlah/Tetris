using UnityEngine;

public class TetrisCell : MonoBehaviour
{
    [SerializeField]
    private Vector2Int cellOffset;

    public Vector2Int CellOffset => cellOffset;
}
