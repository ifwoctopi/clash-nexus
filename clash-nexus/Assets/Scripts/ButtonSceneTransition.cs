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
        
        // Use scene name if provided, otherwise use build index
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else if (targetSceneIndex >= 0)
        {
            SceneManager.LoadScene(targetSceneIndex);
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

