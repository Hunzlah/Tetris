using System;
using UnityEngine;

public enum SoundType
{
    HARDDROP,
    CLEARLINE,
    ROTATE,
    GAMEOVER,

}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class EffectManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    private static EffectManager instance;
    private AudioSource audioSource;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volumeScale = 1f)
    {
        if (instance == null || instance.soundList.Length <= (int)sound)
        {
            Debug.LogWarning($"SoundManager: No sound list for {sound}");
            return;
        }

        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"SoundManager: No audio clips for {sound}");
            return;
        }

        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        float soundVolume = instance.soundList[(int)sound].Volume;
        instance.audioSource.PlayOneShot(randomClip, soundVolume * volumeScale);
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].name = names[i];
            // Initialize volume to 1 if not set
            if (soundList[i].Volume == 0f)
            {
                soundList[i].Volume = 1f;
            }
        }
    }
#endif
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds => sounds;
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
    [Range(0f, 1f)] public float Volume; // New volume field for each sound type
}