using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip _buttonClip;
    [SerializeField] private AudioClip _coinClip;
    [SerializeField] private AudioClip _flyClip;
    [SerializeField] private AudioClip _gameOverClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayButton()
    {
        PlaySFX(_buttonClip);
    }

    public void PlayCoin()
    {
        PlaySFX(_coinClip);
    }

    public void PlayFly()
    {
        PlaySFX(_flyClip);
    }

    public void PlayGameOver()
    {
        PlaySFX(_gameOverClip);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (_sfxSource == null || clip == null) return;
        _sfxSource.PlayOneShot(clip);
    }
}