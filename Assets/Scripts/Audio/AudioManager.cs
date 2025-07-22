using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;
    private GameConfig config;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        config = GameManager.Instance.gameConfig;
    }

    public void PlayExplosionSound()
    {
        if (config != null && config.cubeExplodeSound != null)
        {
            audioSource.PlayOneShot(config.cubeExplodeSound);
        }
    }

    public void PlayCollectSound()
    {
        if (config != null && config.cubeCollectSound != null)
        {
            audioSource.PlayOneShot(config.cubeCollectSound);
        }
    }

    public void PlayBalloonSound()
    {
        if (config != null && config.balloonSound != null)
        {
            audioSource.PlayOneShot(config.balloonSound);
        }
    }

    public void PlayDuckSound()
    {
        if (config != null && config.duckSound != null)
        {
            audioSource.PlayOneShot(config.duckSound);
        }
    }
}
