using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource; 
    [SerializeField] private AudioClip backgroungMusic;

    void Start() 
    {
        musicSource.clip = backgroungMusic;
        musicSource.loop = true;
        musicSource.Play();
    }
}
