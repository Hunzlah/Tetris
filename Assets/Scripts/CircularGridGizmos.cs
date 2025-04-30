using UnityEngine;

[ExecuteAlways]
public class CircularGridGizmos : MonoBehaviour
{
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cellSize = 1f;
    public float circleRadius = 8f;
    public Color cellColor = Color.green;

    private void OnDrawGizmos ()
    {
        Gizmos.color = cellColor;
        Vector3 origin = transform.position;
        Vector2 center = new Vector2(gridWidth / 2f, gridHeight / 2f);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 cellCenter = new Vector2(x + 0.5f, y + 0.5f);
                float dist = Vector2.Distance(cellCenter, center);

                if (dist * cellSize <= circleRadius)
                {
                    Vector3 cellPos = origin + new Vector3(x * cellSize, 0, y * cellSize);
                    Gizmos.DrawWireCube(cellPos + new Vector3(cellSize, 0, cellSize) * 0.5f, new Vector3(cellSize, 0.01f, cellSize));
                }
            }
        }
    }
}
