using System;
using System.Net;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private const string HighscoreKey = "Highscore";
    [SerializeField] private TextMeshProUGUI highScoreText;

    // Call this to update the highscore if the new score is higher
    public void SetNewHighscore(int currentScore)
    {
        int storedHighscore = PlayerPrefs.GetInt(HighscoreKey);

        if (currentScore > storedHighscore)
        {
            PlayerPrefs.SetInt(HighscoreKey, currentScore); 
            PlayerPrefs.Save();
        }
    }

    // Call this to get the saved highscore
    public int GetHighscore()
    {
        return PlayerPrefs.GetInt(HighscoreKey);
    }

    //Function that prints the highscore. Use for UI elements
    public void PrintHighscore()
    {
        highScoreText.text = GetHighscore().ToString();
    }

    // use this to reset the highscore
    public void ResetHighscore()
    {
        PlayerPrefs.DeleteKey(HighscoreKey);
        highScoreText.text = "Removed Highscore";
    }
}
