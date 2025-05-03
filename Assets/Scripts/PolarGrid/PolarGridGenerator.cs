using UnityEngine;

public class PolarGridGenerator : MonoBehaviour
{
    [Header("Grid Dimensions")]
    public int numRings = 20;
    public int numSectors = 36;

    [Header("Cell Settings")]
    public float ringSpacing = 0.5f;
    public GameObject cellPrefab; // A simple visual like a quad, circle, or sprite

    [Header("Gizmo Options")]
    public bool showDebugGizmos = false;
    public float gizmoSize = 0.05f;

    // Stores cell world positions
    private Vector3[,] worldGrid;

    void Start ()
    {
        GenerateGrid();
    }

    void GenerateGrid ()
    {
        worldGrid = new Vector3[numRings, numSectors];

        for (int ring = 0; ring < numRings; ring++)
        {
            float radius = ring * ringSpacing;
            for (int sector = 0; sector < numSectors; sector++)
            {
                float angle = (360f / numSectors) * sector;
                float radians = angle * Mathf.Deg2Rad;

                float x = radius * Mathf.Cos(radians);
                float y = radius * Mathf.Sin(radians);
                Vector3 pos = new Vector3(x, y, 0f);

                worldGrid[ring, sector] = pos;

                // Optional: instantiate a visual representation
                if (cellPrefab)
                {
                    GameObject obj = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                    obj.name = $"Cell [{ring},{sector}]";
                    obj.transform.localScale = Vector3.one * ringSpacing * 0.8f;
                }
            }
        }
    }

    public Vector3 PolarToWorld (int ring, int sector)
    {
        if (ring < 0 || ring >= numRings) return Vector3.zero;
        sector = ((sector % numSectors) + numSectors) % numSectors; // Wrap-around
        return worldGrid[ring, sector];
    }

    private void OnDrawGizmos ()
    {
        if (!showDebugGizmos || worldGrid == null) return;

        Gizmos.color = Color.yellow;
        foreach (var pos in worldGrid)
        {
            Gizmos.DrawSphere(pos, gizmoSize);
        }
    }

}
