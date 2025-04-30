using UnityEngine;

public class GridCell : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    private bool isOccupied;
    public bool IsOccupied => isOccupied;

    private bool isInsideRadius;
    public bool IsInsideRaius=> isInsideRadius;

    public void Set_InsideRadius ()
    {
        isInsideRadius = true;
    }
    public void Set_OutsideRadius ()
    {
        spriteRenderer.enabled = false;
    }
}
