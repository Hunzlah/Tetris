using UnityEngine;
using UnityEngine.UI;

public class SpaceBackgroundMovement : MonoBehaviour
{
    [Header("Background Layers")]
    [SerializeField] RawImage[] backgroundLayers;
    
    [Header("Movement Settings")]
    [SerializeField] float baseScrollSpeed = 0.02f;
    [SerializeField] Vector2[] scrollDirections = {
        new Vector2(0.1f, 0.05f),  
        new Vector2(-0.05f, 0.1f), 
        new Vector2(0.08f, -0.03f) 
    };
    
    [Header("Parallax Settings")]
    [SerializeField] float[] parallaxDepth = { 1f, 0.6f, 0.3f };
    
    [Header("Pulse Settings")]
    [SerializeField] bool enablePulsing = true;
    [SerializeField] float pulseSpeed = 0.5f;
    [SerializeField] float pulseIntensity = 0.1f;
    
    private Rect[] uvRects;

    void Start()
    {
        // Initialize UV rectangles
        uvRects = new Rect[backgroundLayers.Length];
        
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            uvRects[i] = backgroundLayers[i].uvRect;
            
            // Set initial alpha for depth effect
            Color color = backgroundLayers[i].color;
            color.a = 1f - (i * 0.2f); // Layers get slightly more transparent
            backgroundLayers[i].color = color;
        }
    }

    void Update()
    {
        
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            // Calculate movement for this layer
            float layerSpeed = baseScrollSpeed * parallaxDepth[i];
            Vector2 movement = scrollDirections[i] * layerSpeed * Time.deltaTime;
            
            // Update UV position
            uvRects[i].x += movement.x;
            uvRects[i].y += movement.y;
            
            // Wrap UV coordinates to create infinite scrolling
            uvRects[i].x = Mathf.Repeat(uvRects[i].x, 1f);
            uvRects[i].y = Mathf.Repeat(uvRects[i].y, 1f);
            
            // Apply UV changes
            backgroundLayers[i].uvRect = uvRects[i];
        }
    }
}