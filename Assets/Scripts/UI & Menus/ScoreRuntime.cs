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
        currentScoreText.text = currentScore.ToString();
    }
    
    void FixedUpdate()
    {
        if (currentScore > currentHighscore)
        {
            scoreManager.SetNewHighscore(currentScore);
        }
    }
}
