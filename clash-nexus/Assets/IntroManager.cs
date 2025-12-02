using UnityEngine;
using UnityEngine.Video;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class IntroEntry
{
    public string characterTagA;     // Tag of first character
    public string characterTagB;     // Tag of second character
    public VideoClip introVideo;     // Video to play for this matchup
}

public class IntroManager : MonoBehaviour
{
    [Header("Matchup Intro Clips")]
    public List<IntroEntry> intros = new List<IntroEntry>();

    [Header("Default Intro")]
    public VideoClip defaultIntro;

    [Header("Video Player")]
    public VideoPlayer videoPlayer;

    [Header("Next Scene Name")]
    public string nextScene = "SampleScene";

    private Dictionary<(string, string), VideoClip> lookup;
    private Action onIntroFinished;

    void Awake()
    {
        // Build dictionary for fast lookup
        lookup = new Dictionary<(string, string), VideoClip>();

        foreach (var entry in intros)
        {
            if (entry.introVideo == null) continue;

            // Store both orders so order doesn't matter
            lookup[(entry.characterTagA, entry.characterTagB)] = entry.introVideo;
            lookup[(entry.characterTagB, entry.characterTagA)] = entry.introVideo;
        }

        // Optional: hide RawImage / canvas if needed
        if (videoPlayer.targetTexture != null)
            videoPlayer.targetTexture.Release();
    }

    void Start()
    {
        PlayIntroForSelectedCharacters();
    }

    /// <summary>
    /// Automatically reads the selected characters from GameDataManager
    /// </summary>
    private void PlayIntroForSelectedCharacters()
    {
        if (GameDataManager.Instance == null)
        {
            Debug.LogWarning("IntroManager: GameDataManager not found. Playing default intro.");
            PlayIntro("", "", OnIntroCompleted);
            return;
        }
        
        

        string p1Tag = GameDataManager.Instance.GetPlayerCharacter(1);
        string p2Tag = GameDataManager.Instance.GetPlayerCharacter(2);

        PlayIntro(p1Tag, p2Tag, OnIntroCompleted);
    }

    /// <summary>
    /// Plays the intro for two character tags with a callback when finished
    /// </summary>
    public void PlayIntro(string tag1, string tag2, Action onFinished)
    {
        onIntroFinished = onFinished;

        VideoClip clip = GetIntroClip(tag1, tag2);
        if (clip == null)
        {
            clip = defaultIntro;
            if (clip == null)
            {
                Debug.LogWarning("IntroManager: No clip found for this matchup and defaultIntro is null.");
                onIntroFinished?.Invoke();
                return;
            }
        }

        videoPlayer.clip = clip;
        videoPlayer.Play();
        videoPlayer.loopPointReached += OnVideoFinished;

        Debug.Log($"IntroManager: Playing intro for {tag1} vs {tag2}");
    }

    /// <summary>
    /// Lookup the matchup clip
    /// </summary>
    private VideoClip GetIntroClip(string t1, string t2)
    {
        if (string.IsNullOrEmpty(t1) || string.IsNullOrEmpty(t2)) return null;

        if (lookup.TryGetValue((t1, t2), out VideoClip clip))
            return clip;

        return null;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        videoPlayer.loopPointReached -= OnVideoFinished;
        onIntroFinished?.Invoke();
    }

    /// <summary>
    /// Default callback when intro finishes: load next scene
    /// </summary>
    private void OnIntroCompleted()
    {
        if (!string.IsNullOrEmpty(nextScene))
        {
            Debug.Log($"IntroManager: Intro finished. Loading next scene: {nextScene}");
            SceneManager.LoadScene(nextScene);
        }
    }

    /// <summary>
    /// Optional: skip intro manually
    /// </summary>
    void Update()
    {
        if (Input.anyKeyDown && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            OnIntroCompleted();
        }
    }
}
