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
                if (dist * cellSize <= circleRadius && dist * cellSize >= minRadius)
                {
                    gridCells[x, y].Set_InsideRadius();
                }
                else
                {
                    gridCells[x, y].Set_OutsideRadius();
                }
                gridCells[x, y].transform.SetParent(container.transform);
            }
        }
    }
}