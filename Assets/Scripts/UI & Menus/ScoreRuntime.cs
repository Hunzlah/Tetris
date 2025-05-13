using TMPro;
using UnityEngine;

public class ScoreRuntime : MonoBehaviour
{
    private int currentScore; 
    private int currentHighscore;
    
    [SerializeField] private TMP_Text currentScoreText, highScoreText;
    
    [SerializeField] private ScoreManager scoreManager;
    
    void Start()
    {
        currentScore = 0;
        currentHighscore = scoreManager.GetHighscore(); 
        highScoreText.text = currentHighscore.ToString();
        
    }
    
    void FixedUpdate()
    {
        currentScoreText.text = currentScore.ToString();
        
        if (currentScore > currentHighscore)
        {
            scoreManager.SetNewHighscore(currentScore);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentScore += 100;
        }
    }
}
