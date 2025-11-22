using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages background music that persists across scenes and can switch tracks for specific scenes.
/// Place this on a GameObject in your first scene (or create a prefab).
/// </summary>
public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Default Music")]
    [Tooltip("The default background music that plays across all scenes")]
    [SerializeField] private AudioClip defaultMusic;
    
    [Header("Scene-Specific Music")]
    [Tooltip("Music to play for specific scenes. Leave empty to use default music.")]
    [SerializeField] private SceneMusic[] sceneMusic;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    [Tooltip("Volume of the background music")]
    [SerializeField] private float volume = 0.5f;
    
    [Tooltip("Should the music loop?")]
    [SerializeField] private bool loopMusic = true;
    
    [Tooltip("Fade time when switching between tracks (in seconds)")]
    [SerializeField] private float fadeTime = 1f;

    private AudioSource audioSource;
    private static BackgroundMusicManager instance;
    private string currentSceneName;
    private AudioClip currentClip;
    private bool isFading = false;

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
    }

    void Awake()
    {
        // Ensure only one instance exists (singleton pattern)
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = loopMusic;
        audioSource.volume = volume;
    }

    void Start()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Play music for the current scene
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called when a new scene is loaded
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string newSceneName = scene.name;
        
        // Don't do anything if we're already playing the correct music for this scene
        if (newSceneName == currentSceneName && audioSource.isPlaying && audioSource.clip == currentClip)
        {
            return;
        }

        currentSceneName = newSceneName;

        // Find music for this scene
        AudioClip musicToPlay = GetMusicForScene(newSceneName);

        // If no specific music found, use default
        if (musicToPlay == null)
        {
            musicToPlay = defaultMusic;
        }

        // Switch to the new music
        if (musicToPlay != null)
        {
            PlayMusic(musicToPlay);
        }
        else if (audioSource.isPlaying)
        {
            // If no music is assigned, stop playing
            StopMusic();
        }
    }

    /// <summary>
    /// Gets the music clip for a specific scene
    /// </summary>
    private AudioClip GetMusicForScene(string sceneName)
    {
        if (sceneMusic == null || sceneMusic.Length == 0)
            return null;

        foreach (var sceneMusicEntry in sceneMusic)
        {
            if (sceneMusicEntry.sceneName == sceneName)
            {
                return sceneMusicEntry.musicClip;
            }
        }

        return null;
    }

    /// <summary>
    /// Plays the specified music clip
    /// </summary>
    private void PlayMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        // If it's the same clip already playing, don't restart it
        if (audioSource.clip == clip && audioSource.isPlaying)
        {
            return;
        }

        currentClip = clip;

        // If we're already playing music, fade out then fade in
        if (audioSource.isPlaying && audioSource.clip != null)
        {
            StartCoroutine(FadeAndSwitch(clip));
        }
        else
        {
            // Just start playing
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Stops the current music
    /// </summary>
    private void StopMusic()
    {
        if (audioSource.isPlaying)
        {
            StartCoroutine(FadeOut());
        }
    }

    /// <summary>
    /// Fades out current music and fades in new music
    /// </summary>
    private System.Collections.IEnumerator FadeAndSwitch(AudioClip newClip)
    {
        isFading = true;
        float startVolume = audioSource.volume;

        // Fade out
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        // Switch clip
        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in
        while (audioSource.volume < volume)
        {
            audioSource.volume += volume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.volume = volume;
        isFading = false;
    }

    /// <summary>
    /// Fades out the current music
    /// </summary>
    private System.Collections.IEnumerator FadeOut()
    {
        isFading = true;
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = volume;
        isFading = false;
    }

    /// <summary>
    /// Manually set the volume (useful for settings menu)
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (!isFading)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// Get the current volume
    /// </summary>
    public float GetVolume()
    {
        return volume;
    }
}

