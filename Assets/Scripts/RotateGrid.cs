using UnityEngine;

public class RotateWithKeys : MonoBehaviour
{
    public GameObject targetObject; // Assign the container GameObject in the Inspector

    void Update()
    {
        // Rotate left with A key
        if (Input.GetKeyDown(KeyCode.A))
        {
            targetObject.transform.Rotate(0, 45, 0); // Rotate 45 degrees left (around Y-axis)
        }
        // Rotate right with D key
        if (Input.GetKeyDown(KeyCode.D))
        {
            targetObject.transform.Rotate(0, -45, 0); // Rotate 45 degrees right (around Y-axis)
        }
    }
}