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
    
    void Update()
    {
        currentScoreText.text = currentScore.ToString();
        highScoreText.text = currentHighscore.ToString();
        
        if (currentScore >= currentHighscore)
        {
            currentHighscore = currentScore;
            scoreManager.SetNewHighscore(currentScore);
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            currentScore += 100;
        }
    }
    
    public void UpdateScore(int score)
    {
        score += currentScore;
    }
}
