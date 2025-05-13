using System;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.Serialization;

public class MateChanBounce : MonoBehaviour
{
    public int mateState = 2;
    // implement later maybe [SerializeField] private GameObject quoteBox;
    [SerializeField] private float bounceHeight = 1f; 
    [SerializeField] private float bounceDuration = 0.5f; 

    [SerializeField] private TextMeshProUGUI mataChanText;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
       
        transform.DOMoveY(startPos.y + bounceHeight, bounceDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

   
    //an array of character comment for multiple states inside the main menu
    private void FixedUpdate()
    {
        switch (mateState)
        {
            case 1:
                mataChanText.text ="Have Fun";
                break;
            case 2:
                mataChanText.text = "Welcome Back!";
                break;
            case 3:
                mataChanText.text = "They created me";
                break;
            case 4:
                mataChanText.text = "Don't leave me";
                break;
            case 5:
                mataChanText.text = "Thank you for staying with me! Let's have some fun ^_^";
                break;
            case 6:
                mataChanText.text = "That's a tiny score, you can do better!";
                break;
            case 7:
                mataChanText.text = "You got this I know you can reach even further beyond";
                break;
            case 8:
                mataChanText.text = "You're starting to impress me, but I know you can do even better!";
                break;
            case 9:
                mataChanText.text = "That's one might high score you got going there. I'm really impressed";
                break;
            case 10:
                mataChanText.text = "Time to get your game on!";
                break;
            case 11:
                mataChanText.text = "Time for some fun!";
                break;
            case 12:
                mataChanText.text = "You can call me Maru-Sensei";
                break;
            case 13:
                mataChanText.text = "You ready to play now?";
                break;
            
            default:
                mataChanText.text = "Error";
                break;
        }
    }
}
