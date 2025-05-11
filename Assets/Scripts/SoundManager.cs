using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioSource clickSound;
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float maxVolume = 1f;
    
    private Coroutine fadeCoroutine;
    private bool isFading = false;
    
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
        backgroundMusic.volume = 0f; // Start with volume at 0
        backgroundMusic.Play();
        
        // Start fade in
        StartFade(backgroundMusic, fadeInDuration, maxVolume);
    }
    
    public void StopBgMusic()
    {
        StartFade(backgroundMusic, fadeOutDuration, 0f, () => {
            backgroundMusic.Stop();
        });
    }
    
    private void StartFade(AudioSource audioSource, float duration, float targetVolume, System.Action onComplete = null)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeAudio(audioSource, duration, targetVolume, onComplete));
    }
    
    private IEnumerator FadeAudio(AudioSource audioSource, float duration, float targetVolume, System.Action onComplete = null)
    {
        isFading = true;
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, normalizedTime);
            yield return null;
        }
        
        audioSource.volume = targetVolume;
        isFading = false;
        
        onComplete?.Invoke();
    }
    
    // For non-looping songs, you can use this to fade out before the end
    public void FadeOutBeforeEnd()
    {
        if (backgroundMusic.clip != null && backgroundMusic.loop)
        {
            float timeUntilEnd = backgroundMusic.clip.length - backgroundMusic.time;
            
            if (timeUntilEnd <= fadeOutDuration && !isFading)
            {
                StartFade(backgroundMusic, fadeOutDuration, 0f);
                PlayBgMusic();
            }
        }
    }
    
    private void Update()
    {
        // Check if we need to fade out before the song ends
        if (backgroundMusic.isPlaying && backgroundMusic.loop)
        {
            FadeOutBeforeEnd();
        }
    }
    
    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            clickSound.loop = false;
            clickSound.Play();
        }
    }
}