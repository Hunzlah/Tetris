using TMPro;
using UnityEngine;

public class ScoreRuntime : MonoBehaviour
{
    private int currentScore; 
    [SerializeField] private TMP_Text currentScoreText, highScoreText;
    
    [SerializeField] private ScoreManager scoreManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentScore = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
