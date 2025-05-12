using UnityEngine;

public class TetrisCell : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Vector2Int cellOffset;

    public Vector2Int CellOffset => cellOffset;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
}
