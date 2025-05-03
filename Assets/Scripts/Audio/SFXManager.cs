using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using static EnemyHealthManager;

public class SFXManager : MonoBehaviour, IGameDataPersistence
{
    public int musicVolume = 100;
    public int sfxVolume = 100;
    [SerializeField] private AudioSource sfxSource;
    [Header("Sound list")]
    [SerializeField] private AudioClip shootingSFX;
    [SerializeField] private AudioClip enemyDeathSFX;
    [SerializeField] private AudioClip explosionSFX;
    [JsonIgnore]
    public static SFXManager instance;
    private List<AudioSource> playingSources = new List<AudioSource>();

    void OnEnable()
    {
        EnemyHealthManager.OnEnemyDeath += EnemyDeathSFX;
    }
    void OnDisable()
    {
        EnemyHealthManager.OnEnemyDeath -= EnemyDeathSFX;
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        playingSources = new List<AudioSource>();
    }

    void Update()
    {
        playingSources.RemoveAll(source => !source.isPlaying);
    }

    public void ShootSFX(AudioSource audioSource)
    {
        SetSFXVolume(audioSource);
        audioSource.clip = shootingSFX;
        audioSource.Play();
    }
    public void ExplosionSFX(AudioSource audioSource)
    {
        SetSFXVolume(audioSource);
        audioSource.clip = explosionSFX;
        audioSource.Play();
    }

    private void EnemyDeathSFX(EnemyHealthManager enemyHealth, DeathReason reason)
    {
        if (reason == DeathReason.KilledByTower)
        {
            AudioSource audioSource = Instantiate(sfxSource, transform.position, Quaternion.identity);
            SetSFXVolume(audioSource);
            audioSource.clip = enemyDeathSFX;
            audioSource.Play();
            playingSources.Add(audioSource);
        }
    }
    private void SetSFXVolume(AudioSource audioSource)
    {
        audioSource.volume = sfxVolume / 100f;
    }
    public void SetMusicVolume(int volume)
    {
        musicVolume = volume;
        GetComponent<AudioSource>().volume = musicVolume / 100f;
    }

    public void LoadData(GameData data)
    {
        sfxVolume = data.sfxVolume;
        musicVolume = data.musicVolume;
        SetMusicVolume(musicVolume);
    }

    public void SaveData(ref GameData data)
    {
        data.sfxVolume = sfxVolume;
        data.musicVolume = musicVolume;
    }
}
