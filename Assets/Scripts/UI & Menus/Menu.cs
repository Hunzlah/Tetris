using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Serialization;

public class Menu : MonoBehaviour
{
    [SerializeField] private RectTransform quitButton, creditsButton;
    [SerializeField] private float buttonEndPos, buttonMovementSpeed,buttonResetPos;
    
    [SerializeField] private RectTransform creditsBox;
    [SerializeField] private float popAnimationEffect = 0.5f;
    
    [SerializeField] private Vector2 hiddenUiBoxVisiblePos;
    [SerializeField] private Vector2 visibleUiBoxVisiblePos;

    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
        Application.Quit();
    }
    
    //Start DOTween animations for each main menu button on startup of game
    void Awake()
    {
       
        visibleUiBoxVisiblePos = creditsBox.anchoredPosition;
        
        float screenWidth = Screen.width;
        hiddenUiBoxVisiblePos = new Vector2(screenWidth + 200f, visibleUiBoxVisiblePos.y);
        
        creditsBox.anchoredPosition = hiddenUiBoxVisiblePos;
        creditsBox.gameObject.SetActive(false);
        
        quitButton.DOAnchorPosX(buttonEndPos, buttonMovementSpeed).SetEase(Ease.OutBack);
        creditsButton.DOAnchorPosX(buttonEndPos, buttonMovementSpeed).SetEase(Ease.OutBack);
    }

    public void Credits()
    {
        //animates main menu buttons to the side 
        quitButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad);
        creditsButton.DOAnchorPosX(buttonResetPos, buttonMovementSpeed).SetEase(Ease.InOutQuad);

        //sets credits box as visible and animates it with a pop from the right side of the screen
        creditsBox.gameObject.SetActive(true);
        creditsBox.anchoredPosition = hiddenUiBoxVisiblePos;
        creditsBox.DOAnchorPos(visibleUiBoxVisiblePos, popAnimationEffect).SetEase(Ease.OutBack);
    }

    //returns UI boxes to their hidden position and on completion sets their game object to false while pushing the initial main menu buttons back. 
    public void MainMenu()
    {
        creditsBox.DOAnchorPos(hiddenUiBoxVisiblePos, popAnimationEffect).SetEase(Ease.InBack)
            .OnComplete(() => {
                creditsBox.gameObject.SetActive(false);

                quitButton.DOAnchorPosX(buttonEndPos, buttonMovementSpeed).SetEase(Ease.OutBack);
                creditsButton.DOAnchorPosX(buttonEndPos, buttonMovementSpeed).SetEase(Ease.OutBack);
            });

        //quitButton.DOAnchorPosX(buttonEndPos, buttonMovementSpeed).SetEase(Ease.OutBack);
       // creditsButton.DOAnchorPosX(buttonEndPos, buttonMovementSpeed).SetEase(Ease.OutBack);
    }
}
