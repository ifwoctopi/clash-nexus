using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// A back button that navigates to the previous scene or a specified target scene.
/// Attach this to a button (or its child) to make it go back when clicked.
/// Can be used standalone or alongside ButtonSceneTransition.
/// </summary>
public class BackButton : MonoBehaviour
{
    [Header("Back Navigation Settings")]
    [Tooltip("Specific scene to go back to (leave empty to use automatic navigation based on current scene)")]
    [SerializeField] private string targetBackScene = "";
    
    [Tooltip("Delay before loading scene (to allow sound to play)")]
    [SerializeField] private float loadDelay = 0.4f;
    
    [Tooltip("Use SceneTransitionManager for smooth transitions (if available)")]
    [SerializeField] private bool useTransitionManager = true;

    private Button button;

    void Awake()
    {
        // Try to get Button component from this object or parent
        button = GetComponent<Button>();
        if (button == null)
        {
            button = GetComponentInParent<Button>();
        }
    }

    void Start()
    {
        // Subscribe to the button's onClick event if we found one
        if (button != null)
        {
            button.onClick.AddListener(GoBack);
            Debug.Log($"BackButton: Added listener to button for back navigation");
        }
        else
        {
            Debug.LogError($"BackButton: Could not find Button component on {gameObject.name} or its parent!");
        }
    }

    /// <summary>
    /// Public method that goes back - can be called from Inspector OnClick event
    /// </summary>
    public void GoBack()
    {
        // Start coroutine to delay scene load (allows sound to play)
        StartCoroutine(GoBackDelayed());
    }
    
    private IEnumerator GoBackDelayed()
    {
        // Wait a moment to allow sound to play
        yield return new WaitForSeconds(loadDelay);
        
        // Determine target scene
        string targetScene = GetBackScene();
        
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("BackButton: No target scene determined. Cannot go back.");
            yield break;
        }
        
        // Reset practice mode when going back to MainMenuScreen or ModeChooser
        // (ModeChooser is where you select the mode, so practice mode should be reset there)
        if (targetScene == "MainMenuScreen" || targetScene == "ModeChooser")
        {
            GameDataManager dataManager = GameDataManager.Instance;
            if (dataManager != null)
            {
                dataManager.SetPracticeMode(false);
                Debug.Log($"BackButton: Practice mode reset to false when returning to {targetScene}");
            }
        }
        
        // Use SceneTransitionManager if available and enabled
        if (useTransitionManager)
        {
            SceneTransitionManager transitionManager = SceneTransitionManager.Instance;
            if (transitionManager != null)
            {
                transitionManager.TransitionToScene(targetScene);
                Debug.Log($"BackButton: Transitioning to '{targetScene}' using SceneTransitionManager");
                yield break;
            }
        }
        
        // Fallback to direct scene load
        // Check if scene exists in build settings
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == targetScene)
            {
                sceneExists = true;
                break;
            }
        }
        
        if (sceneExists)
        {
            SceneManager.LoadScene(targetScene);
            Debug.Log($"BackButton: Loaded scene '{targetScene}'");
        }
        else
        {
            Debug.LogError($"BackButton: Scene '{targetScene}' not found in Build Settings! " +
                $"Please add it to File > Build Settings > Scenes In Build.");
        }
    }
    
    /// <summary>
    /// Determines which scene to go back to based on current scene or configured target
    /// </summary>
    private string GetBackScene()
    {
        // If a specific target scene is configured, use it
        if (!string.IsNullOrEmpty(targetBackScene))
        {
            return targetBackScene;
        }
        
        // Otherwise, determine based on current scene
        string currentScene = SceneManager.GetActiveScene().name;
        
        // Navigation flow:
        // MainMenuScreen → ModeChooser → CharacterSelect → ChallengeSystem/MapChooser → SampleScene
        
        switch (currentScene)
        {
            case "ModeChooser":
                return "MainMenuScreen";
                
            case "CharacterSelect":
                return "ModeChooser";
                
            case "ChallengeSystem":
            case "MapChooser":
                return "CharacterSelect";
                
            case "SampleScene":
                // From game scene, go back to main menu (or could go to ChallengeSystem/MapChooser)
                // Check if we came from ChallengeSystem or MapChooser
                GameDataManager dataManager = GameDataManager.Instance;
                if (dataManager != null && dataManager.IsPracticeMode())
                {
                    // In practice mode, go back to character select
                    return "CharacterSelect";
                }
                // Otherwise go to main menu
                return "MainMenuScreen";
                
            case "IntroScene":
                return "MainMenuScreen";
                
            default:
                // Default fallback to main menu
                Debug.LogWarning($"BackButton: Unknown scene '{currentScene}', defaulting to MainMenuScreen");
                return "MainMenuScreen";
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the button's onClick event to prevent memory leaks
        if (button != null)
        {
            button.onClick.RemoveListener(GoBack);
        }
    }
}

