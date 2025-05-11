using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioSource clickSound;
    
    private void Start()
    {
        if (backgroundMusic != null)
        {
            PlayBgMusic(); 
        }
    }
    
   private void PlayBgMusic()
    {
        backgroundMusic.loop = true;
        backgroundMusic.Play();
    }

    public void PlayClickSound()
    {
        clickSound.loop = false;
        clickSound.Play();
    }
}
