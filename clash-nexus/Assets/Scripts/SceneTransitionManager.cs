using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages scene transitions with a transition screen (fade, loading screen, etc.)
/// Place this on a GameObject in your scene or create a prefab.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    [Header("Transition Screen")]
    [Tooltip("The GameObject that contains your transition screen UI (should have an Image component)")]
    [SerializeField] private GameObject transitionScreen;
    
    [Tooltip("The Image component on the transition screen (for fade effects)")]
    [SerializeField] private Image transitionImage;

    [Header("Transition Settings")]
    [Tooltip("Duration of the fade in/out transition (in seconds)")]
    [SerializeField] private float transitionDuration = 1f;
    
    [Tooltip("Color of the transition screen")]
    [SerializeField] private Color transitionColor = Color.black;
    
    [Tooltip("Should the transition screen start hidden?")]
    [SerializeField] private bool startHidden = true;

    private static SceneTransitionManager instance;
    private bool isTransitioning = false;

    void Awake()
    {
        // Ensure only one instance exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup transition screen
        if (transitionScreen != null)
        {
            transitionScreen.SetActive(!startHidden);
        }

        if (transitionImage != null)
        {
            transitionImage.color = transitionColor;
            if (startHidden)
            {
                Color color = transitionColor;
                color.a = 0f;
                transitionImage.color = color;
            }
        }
    }

    /// <summary>
    /// Transitions to a new scene by name
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning)
            return;

        StartCoroutine(TransitionCoroutine(sceneName));
    }

    /// <summary>
    /// Transitions to a new scene by build index
    /// </summary>
    public void TransitionToScene(int sceneBuildIndex)
    {
        if (isTransitioning)
            return;

        StartCoroutine(TransitionCoroutine(sceneBuildIndex));
    }

    /// <summary>
    /// Coroutine that handles the transition
    /// </summary>
    private IEnumerator TransitionCoroutine(string sceneName)
    {
        isTransitioning = true;

        // Fade in (show transition screen)
        yield return StartCoroutine(FadeIn());

        // Load the new scene
        SceneManager.LoadScene(sceneName);

        // Wait a frame for the scene to start loading
        yield return null;

        // Fade out (hide transition screen)
        yield return StartCoroutine(FadeOut());

        isTransitioning = false;
    }

    /// <summary>
    /// Coroutine that handles the transition
    /// </summary>
    private IEnumerator TransitionCoroutine(int sceneBuildIndex)
    {
        isTransitioning = true;

        // Fade in (show transition screen)
        yield return StartCoroutine(FadeIn());

        // Load the new scene
        SceneManager.LoadScene(sceneBuildIndex);

        // Wait a frame for the scene to start loading
        yield return null;

        // Fade out (hide transition screen)
        yield return StartCoroutine(FadeOut());

        isTransitioning = false;
    }

    /// <summary>
    /// Fades in the transition screen
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (transitionScreen != null)
        {
            transitionScreen.SetActive(true);
        }

        if (transitionImage != null)
        {
            float elapsed = 0f;
            Color startColor = transitionColor;
            startColor.a = 0f;
            Color endColor = transitionColor;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionDuration;
                transitionImage.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            transitionImage.color = endColor;
        }
        else
        {
            // If no image, just wait for the duration
            yield return new WaitForSeconds(transitionDuration);
        }
    }

    /// <summary>
    /// Fades out the transition screen
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (transitionImage != null)
        {
            float elapsed = 0f;
            Color startColor = transitionColor;
            Color endColor = transitionColor;
            endColor.a = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionDuration;
                transitionImage.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            transitionImage.color = endColor;
        }
        else
        {
            // If no image, just wait for the duration
            yield return new WaitForSeconds(transitionDuration);
        }

        if (transitionScreen != null)
        {
            transitionScreen.SetActive(false);
        }
    }

    /// <summary>
    /// Get the singleton instance
    /// </summary>
    public static SceneTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SceneTransitionManager>();
            }
            return instance;
        }
    }

    /// <summary>
    /// Quick method to transition to a scene (static access)
    /// </summary>
    public static void TransitionTo(string sceneName)
    {
        if (Instance != null)
        {
            Instance.TransitionToScene(sceneName);
        }
        else
        {
            // Fallback: just load the scene without transition
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// Quick method to transition to a scene by index (static access)
    /// </summary>
    public static void TransitionTo(int sceneBuildIndex)
    {
        if (Instance != null)
        {
            Instance.TransitionToScene(sceneBuildIndex);
        }
        else
        {
            // Fallback: just load the scene without transition
            SceneManager.LoadScene(sceneBuildIndex);
        }
    }
}

