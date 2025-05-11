using UnityEngine;
using DG.Tweening;

public class Menu : MonoBehaviour
{
    [SerializeField] private RectTransform quitButton, creditsButton, scoreButton;
    [SerializeField] private float buttonEndPos, buttonMovementSpeed,buttonResetPos,popAnimationEffect;
    
    [SerializeField] private RectTransform creditsBox, scoreBox;
    
    [SerializeField] private Vector2 hiddenUiBoxVisiblePos, visibleUiBoxVisiblePos;

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
        scoreBox.gameObject.SetActive(false);
        creditsBox.gameObject.SetActive(false);

        MainMenuButtonInit();
    }

    public void OpenCredits()
    {
        //animates main menu buttons to the side 
        quitButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad);
        creditsButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad);
        scoreButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad);

        //sets credits box as visible and animates it with a pop from the right side of the screen
        creditsBox.gameObject.SetActive(true);
        creditsBox.anchoredPosition = hiddenUiBoxVisiblePos;
        creditsBox.DOAnchorPos(visibleUiBoxVisiblePos, popAnimationEffect).SetEase(Ease.OutBack);
    }

    //returns UI boxes to their hidden position and on completion sets their game object to false while pushing the initial main menu buttons back. 
    public void CloseCredits()
    {
        creditsBox.DOAnchorPos(hiddenUiBoxVisiblePos, popAnimationEffect).SetEase(Ease.InBack)
            .OnComplete(() => {
                creditsBox.gameObject.SetActive(false);

                MainMenuButtonInit();
            });
    }
    
    public void OpenScores()
    {
        //animates main menu buttons to the side 
        quitButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad);
        creditsButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad);
        scoreButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad);

        //sets score box as visible and animates it with a pop from the right side of the screen
        scoreBox.gameObject.SetActive(true);
        scoreBox.anchoredPosition = hiddenUiBoxVisiblePos;
        scoreBox.DOAnchorPos(visibleUiBoxVisiblePos, popAnimationEffect).SetEase(Ease.OutBack);
    }
    
    //returns score UI to their hidden position and on completion sets their game object to false while pushing the initial main menu buttons back. 
    public void CloseScores()
    {
        scoreBox.DOAnchorPos(hiddenUiBoxVisiblePos, popAnimationEffect).SetEase(Ease.InBack)
            .OnComplete(() => {
                scoreBox.gameObject.SetActive(false);

                MainMenuButtonInit();
            });
    }

   
    //Method that initializes the core main menu buttons. Will be called once the game starts or when the player goes back from one menu item back to the core main menu.
    private void MainMenuButtonInit()
    {
        quitButton.DOAnchorPosX(buttonEndPos, buttonMovementSpeed-0.05f).SetEase(Ease.OutBack);
        creditsButton.DOAnchorPosX(buttonEndPos, buttonMovementSpeed-0.03f).SetEase(Ease.OutBack);
        scoreButton.DOAnchorPosX(buttonEndPos, buttonMovementSpeed).SetEase(Ease.OutBack);
    }
}
