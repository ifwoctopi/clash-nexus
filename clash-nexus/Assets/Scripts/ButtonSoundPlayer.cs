using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays a sound effect when a button is clicked.
/// Attach this script to any button GameObject to add click sound functionality.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSoundPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The sound clip to play when the button is clicked.")]
    [SerializeField] private AudioClip clickSound;
    
    [Tooltip("Volume of the button click sound (0.0 to 1.0)")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private Button button;
    private AudioSource audioSource;

    void Awake()
    {
        button = GetComponent<Button>();
        
        // Try to get AudioSource on this GameObject, or create one
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = volume;
        }
    }

    void Start()
    {
        // Subscribe to the button's onClick event
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    /// <summary>
    /// Plays the button click sound
    /// </summary>
    private void PlayClickSound()
    {
        // Play the sound if we have one assigned
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, volume);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the button's onClick event to prevent memory leaks
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }
}

