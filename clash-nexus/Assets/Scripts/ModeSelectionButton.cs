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
        dataManager.SetGameMode(isTwoPlayerMode);
        
        // Reset practice mode when selecting a regular game mode (not practice mode)
        // This ensures practice mode doesn't persist if user went back and selected a new mode
        dataManager.SetPracticeMode(false);

        Debug.Log($"ModeSelectionButton: Game mode set to {(isTwoPlayerMode ? "2 Player" : "1 Player vs CPU")}, practice mode reset to false");
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}

