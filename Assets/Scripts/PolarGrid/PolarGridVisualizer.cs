using UnityEngine;

public class PolarGridVisualizer : MonoBehaviour
{
    [Header ("Grid Settings")]
    public int numRings = 20;
    public int numSectors = 36;
    public float ringSpacing = 0.5f;
    [Header("Gizmo Settings")]
    public float pointRadius = 0.05f;
    public Color gridColor = Color.yellow;
    public bool drawLines = false;

    private void OnDrawGizmos ()
    {
        Gizmos.color = gridColor;

        for (int ring = 0; ring < numRings; ring++)
        {
            float radius = ring * ringSpacing;
            for (int sector = 0; sector < numSectors; sector++)
            {
                float angleDeg = (360f / numSectors) * sector;
                float angleRad = angleDeg * Mathf.Deg2Rad;

                float x = radius * Mathf.Cos(angleRad);
                float y = radius * Mathf.Sin(angleRad);
                Vector3 worldPos = transform.position + new Vector3(x, y, 0f);

                Gizmos.DrawSphere(worldPos, pointRadius);

                // Optionally draw radial and ring lines
                if (drawLines && ring > 0)
                {
                    // Line to center
                    if (sector == 0)
                        Gizmos.DrawLine(transform.position, worldPos);

                    // Line to previous ring (same sector)
                    float prevRadius = (ring - 1) * ringSpacing;
                    float px = prevRadius * Mathf.Cos(angleRad);
                    float py = prevRadius * Mathf.Sin(angleRad);
                    Vector3 prevWorldPos = transform.position + new Vector3(px, py, 0f);
                    Gizmos.DrawLine(prevWorldPos, worldPos);
                }
            }
        }
    }

}
