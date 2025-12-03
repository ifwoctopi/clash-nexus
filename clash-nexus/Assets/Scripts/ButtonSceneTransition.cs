using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Attach this to a button (or its child) to make it load a different scene when clicked.
/// Can also be called directly from the button's OnClick event in Inspector.
/// </summary>
public class ButtonSceneTransition : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load when this button is clicked")]
    [SerializeField] private string targetSceneName;
    
    [Tooltip("Or use scene build index instead (leave Target Scene Name empty if using this)")]
    [SerializeField] private int targetSceneIndex = -1;
    
    [Tooltip("Delay before loading scene (to allow sound to play)")]
    [SerializeField] private float loadDelay = 0.1f;

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
            button.onClick.AddListener(LoadScene);
            Debug.Log($"ButtonSceneTransition: Added listener to button for scene '{targetSceneName}'");
        }
        else
        {
            Debug.LogError($"ButtonSceneTransition: Could not find Button component on {gameObject.name} or its parent!");
        }
    }

    /// <summary>
    /// Public method that loads the scene - can be called from Inspector OnClick event
    /// </summary>
    public void LoadScene()
    {
        // Start coroutine to delay scene load (allows sound to play)
        StartCoroutine(LoadSceneDelayed());
    }
    
    private IEnumerator LoadSceneDelayed()
    {
        // Wait a moment to allow sound to play
        yield return new WaitForSeconds(loadDelay);
        
        // If transitioning to MainMenuScreen, reset practice mode right before loading
        if (targetSceneName == "MainMenuScreen")
        {
            GameDataManager dataManager = GameDataManager.Instance;
            if (dataManager != null)
            {
                dataManager.SetPracticeMode(false);
                Debug.Log("ButtonSceneTransition: Practice mode reset to false when returning to main menu");
            }
        }
        
        // Use scene name if provided, otherwise use build index
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            // Check if scene exists in build settings
            bool sceneExists = false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneName == targetSceneName)
                {
                    sceneExists = true;
                    break;
                }
            }
            
            if (sceneExists)
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogError($"ButtonSceneTransition: Scene '{targetSceneName}' not found in Build Settings! " +
                    $"Please add it to File > Build Settings > Scenes In Build.");
            }
        }
        else if (targetSceneIndex >= 0)
        {
            if (targetSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(targetSceneIndex);
            }
            else
            {
                Debug.LogError($"ButtonSceneTransition: Scene index {targetSceneIndex} is out of range! " +
                    $"Build Settings only has {SceneManager.sceneCountInBuildSettings} scenes.");
            }
        }
        else
        {
            Debug.LogError("ButtonSceneTransition: No target scene specified!");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the button's onClick event to prevent memory leaks
        if (button != null)
        {
            button.onClick.RemoveListener(LoadScene);
        }
    }
}

