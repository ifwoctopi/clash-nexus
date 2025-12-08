using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to mode selection buttons (1 Player or 2 Player) to set the game mode before transitioning.
/// </summary>
public class ModeSelectionButton : MonoBehaviour
{
    [Header("Mode Settings")]
    [Tooltip("Is this button for 2-player mode? (true = 2 players, false = 1 player vs CPU)")]
    [SerializeField] private bool isTwoPlayerMode = false;

    private Button button;
    private ButtonSceneTransition sceneTransition;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            button = GetComponentInParent<Button>();
        }

        sceneTransition = GetComponent<ButtonSceneTransition>();
        if (sceneTransition == null)
        {
            sceneTransition = GetComponentInParent<ButtonSceneTransition>();
        }
    }

    void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    /// <summary>
    /// Called when the button is clicked - sets game mode before scene transition
    /// </summary>
    private void OnButtonClicked()
    {
        // Set the game mode in GameDataManager
        GameDataManager dataManager = GameDataManager.Instance;
        if (dataManager == null) return;
        
        // Check current scene - only change game mode if we're in ModeChooser
        // This prevents accidentally resetting game mode when navigating from other scenes
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isModeChooserScene = currentScene == "ModeChooser";
        
        // Check if we're already in practice mode - if so, don't reset it
        // This prevents practice mode from being reset when navigating back to CharacterSelect
        bool wasInPracticeMode = dataManager.IsPracticeMode();
        
        // Only change game mode if we're actually in ModeChooser (selecting a mode)
        // Otherwise, preserve the current game mode (for navigation buttons)
        if (isModeChooserScene)
        {
            dataManager.SetGameMode(isTwoPlayerMode);
            Debug.Log($"ModeSelectionButton: Mode selection - game mode set to {(isTwoPlayerMode ? "2 Player" : "1 Player vs CPU")}");
        }
        else
        {
            // Preserve current game mode - don't change it
            Debug.Log($"ModeSelectionButton: Navigation button detected (scene: {currentScene}) - preserving current game mode");
        }
        
        // Only reset practice mode if we weren't already in practice mode and we're selecting a mode
        if (!wasInPracticeMode && isModeChooserScene)
        {
            dataManager.SetPracticeMode(false);
            Debug.Log($"ModeSelectionButton: Practice mode reset to false");
        }
        else if (wasInPracticeMode)
        {
            Debug.Log($"ModeSelectionButton: Practice mode detected - preserving practice mode");
        }
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}

