using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource backgroundMusic;
    
    private void Start()
    {
        if (backgroundMusic != null)
        {
            PlayMusic(); 
        }
    }
    
   private void PlayMusic()
    {
        backgroundMusic.loop = true;
        backgroundMusic.Play();
    }
}
