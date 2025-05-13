using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private RectTransform quitButton, creditsButton, scoreButton, playButton;
    [SerializeField] private float buttonEndPos, buttonMovementSpeed,buttonResetPos,popAnimationEffect;
    
    [SerializeField] private RectTransform creditsBox, scoreBox, quitBox;
    
    [SerializeField] private Vector2 hiddenUiBoxVisiblePos, visibleUiBoxVisiblePos;
    [SerializeField] private ScoreManager scoreManager;

    [Header("Mate says:")]
    [SerializeField] private MateChanBounce mateChan;
    [SerializeField] private int mateLowScore;
    [SerializeField] private int mateMidScore;
    [SerializeField] private int mateHighScore;

    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
        Application.Quit();
    }
    
    //Start DOTween animations for each main menu button on startup of game and configured base settings for sub menues in main menu
    void Awake()
    { 
        visibleUiBoxVisiblePos = creditsBox.anchoredPosition;
        
        float screenWidth = Screen.width;
        hiddenUiBoxVisiblePos = new Vector2(screenWidth + 200f, visibleUiBoxVisiblePos.y);
        
        creditsBox.anchoredPosition = hiddenUiBoxVisiblePos;
        scoreBox.anchoredPosition = hiddenUiBoxVisiblePos;
        //quitButton.anchoredPosition = hiddenUiBoxVisiblePos;
        scoreBox.gameObject.SetActive(false);
        creditsBox.gameObject.SetActive(false);
        quitBox.gameObject.SetActive(false);

        MainMenuButtonInit();
        mateChan.mateState = 2;
    }

    public void PlayGame()
    {
        mateChan.mateState = 1;
        quitButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad);
        creditsButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad);
        scoreButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                SceneManager.LoadScene(1);
        });
    }

    public void OpenCredits()
    {
        CloseMainMenu();

        //sets credits box as visible and animates it with a pop from the right side of the screen
        creditsBox.gameObject.SetActive(true);
        creditsBox.anchoredPosition = hiddenUiBoxVisiblePos;
        creditsBox.DOAnchorPos(visibleUiBoxVisiblePos, popAnimationEffect).SetEase(Ease.OutBack);
        mateChan.mateState = 3;
    }

    //returns UI boxes to their hidden position and on completion sets their game object to false while pushing the initial main menu buttons back. 
    public void CloseCredits()
    {
        creditsBox.DOAnchorPos(hiddenUiBoxVisiblePos, popAnimationEffect).SetEase(Ease.InBack)
            .OnComplete(() => {
                creditsBox.gameObject.SetActive(false);

                MainMenuButtonInit();
                mateChan.mateState = 11;
            });
    }
    
    public void OpenScores()
    {
        CloseMainMenu();
        scoreManager.GetHighscore();
        scoreManager.PrintHighscore();

        //sets score box as visible and animates it with a pop from the right side of the screen
        scoreBox.gameObject.SetActive(true);
        scoreBox.anchoredPosition = hiddenUiBoxVisiblePos;
        scoreBox.DOAnchorPos(visibleUiBoxVisiblePos, popAnimationEffect).SetEase(Ease.OutBack);
        scoreDialogue();
    }
    
    //returns score UI to their hidden position and on completion sets their game object to false while pushing the initial main menu buttons back. 
    public void CloseScores()
    {
        scoreBox.DOAnchorPos(hiddenUiBoxVisiblePos, popAnimationEffect).SetEase(Ease.InBack)
            .OnComplete(() => {
                scoreBox.gameObject.SetActive(false);

                MainMenuButtonInit();
                mateChan.mateState = 10;
            });
    }
    
    public void OpenQuit()
    {
        CloseMainMenu();
        mateChan.mateState = 4;
        
        //sets score box as visible and animates it with a pop from the right side of the screen
        quitBox.gameObject.SetActive(true);
        quitBox.anchoredPosition = hiddenUiBoxVisiblePos;
        quitBox.DOAnchorPos(visibleUiBoxVisiblePos, popAnimationEffect).SetEase(Ease.OutBack);
    }
    
    public void CloseQuit()
    {
        quitBox.DOAnchorPos(hiddenUiBoxVisiblePos, popAnimationEffect).SetEase(Ease.InBack)
            .OnComplete(() => {
                quitBox.gameObject.SetActive(false);

                MainMenuButtonInit();
                mateChan.mateState = 5;
            });
    }

   
    //Method that initializes the core main menu buttons. Will be called once the game starts or when the player goes back from one menu item back to the core main menu.
    private void MainMenuButtonInit()
    {
        quitButton.DOAnchorPosX(buttonEndPos, buttonMovementSpeed-0.05f).SetEase(Ease.OutBack);
        creditsButton.DOAnchorPosX(buttonEndPos, buttonMovementSpeed-0.03f).SetEase(Ease.OutBack);
        scoreButton.DOAnchorPosX(buttonEndPos, buttonMovementSpeed).SetEase(Ease.OutBack);
        playButton.DOAnchorPosX(buttonEndPos, buttonMovementSpeed+0.01f).SetEase(Ease.OutBack);
    }
    private void CloseMainMenu()
    {
        quitButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad);
        creditsButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad);
        scoreButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad);
        playButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad);
    }

    private void scoreDialogue()
    {
        if (scoreManager.GetHighscore() <= 1)
        {
            mateChan.mateState = 6;
        } else if (scoreManager.GetHighscore() >= mateLowScore)
        {
            mateChan.mateState = 7;
        }
        else if (scoreManager.GetHighscore() >= mateMidScore)
        {
            mateChan.mateState = 7;
        }
        else if (scoreManager.GetHighscore() >= mateHighScore)
        {
            mateChan.mateState = 8;
        }
    }
}
