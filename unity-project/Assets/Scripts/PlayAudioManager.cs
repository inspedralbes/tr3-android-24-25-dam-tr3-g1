using UnityEngine;

public class PlayAudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] public AudioSource SFXSource;
    [SerializeField] private AudioClip backgroungMusic;
    [SerializeField] private AudioClip attckSFX;
    [SerializeField] private AudioClip walkSFX;
    [SerializeField] private AudioClip deathSFX;

    void Start() 
    {
        musicSource.clip = backgroungMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayAttackSFX()
    {
        SFXSource.clip = attckSFX;
        SFXSource.Play();
    }
    public void PlayWalkSFX()
    {
        SFXSource.clip = walkSFX;
        SFXSource.Play();
    }
    public void PlayDeathSFX()
    {
        SFXSource.clip = deathSFX;
        SFXSource.Play();
    }
}
