using UnityEngine;
using UnityEngine.UI;

public class SpaceBackgroundUI : MonoBehaviour
{
    [Header("Star Layers")]
    [SerializeField] RawImage[] starLayers;
    
    [Header("Movement Settings")]
    [SerializeField] float baseScrollSpeed = 0.05f;
    [SerializeField] float[] layerSpeedMultipliers = { 1f, 0.7f, 0.4f };
    
    [Header("Fade Settings")]
    [SerializeField] float[] fadeMinAlpha = { 0.3f, 0.2f, 0.1f };
    [SerializeField] float[] fadeMaxAlpha = { 0.8f, 0.6f, 0.4f };
    [SerializeField] float[] fadeSpeeds = { 0.5f, 0.3f, 0.7f };
    
    private float[] currentAlpha;
    private float[] fadeDirection;
    private Rect[] uvRects;

    void Start()
    {
        // Initialize arrays
        int layerCount = starLayers.Length;
        currentAlpha = new float[layerCount];
        fadeDirection = new float[layerCount];
        uvRects = new Rect[layerCount];
        
        // Set initial values
        for (int i = 0; i < layerCount; i++)
        {
            // Initialize UV rect (for texture scrolling)
            uvRects[i] = starLayers[i].uvRect;
            
            // Set random initial alpha and fade direction
            currentAlpha[i] = Random.Range(fadeMinAlpha[i], fadeMaxAlpha[i]);
            fadeDirection[i] = Random.value > 0.5f ? 1f : -1f;
            
            // Apply initial alpha
            Color color = starLayers[i].color;
            color.a = currentAlpha[i];
            starLayers[i].color = color;
        }
    }

    void Update()
    {
        for (int i = 0; i < starLayers.Length; i++)
        {
            // Handle texture scrolling via UV manipulation
            float scrollSpeed = baseScrollSpeed * layerSpeedMultipliers[i] * Time.deltaTime;
            
            // Update UV position
            uvRects[i].x += scrollSpeed;
            uvRects[i].y += scrollSpeed * 0.5f; // Slight vertical movement
            
            // Wrap the UV coordinates
            if (uvRects[i].x > 1f) uvRects[i].x -= 1f;
            if (uvRects[i].y > 1f) uvRects[i].y -= 1f;
            
            // Apply UV changes
            starLayers[i].uvRect = uvRects[i];
            
            // Handle fading
            currentAlpha[i] += fadeDirection[i] * fadeSpeeds[i] * Time.deltaTime;
            
            // Reverse fade direction if limits are reached
            if (currentAlpha[i] >= fadeMaxAlpha[i])
            {
                currentAlpha[i] = fadeMaxAlpha[i];
                fadeDirection[i] = -1f;
            }
            else if (currentAlpha[i] <= fadeMinAlpha[i])
            {
                currentAlpha[i] = fadeMinAlpha[i];
                fadeDirection[i] = 1f;
            }
            
            // Apply alpha changes
            Color color = starLayers[i].color;
            color.a = currentAlpha[i];
            starLayers[i].color = color;
        }
    }
}