using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to a practice mode button to set practice mode before transitioning.
/// </summary>
public class PracticeModeButton : MonoBehaviour
{
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
    /// Called when the button is clicked - sets practice mode before scene transition
    /// </summary>
    private void OnButtonClicked()
    {
        // Set practice mode in GameDataManager
        GameDataManager dataManager = GameDataManager.Instance;
        dataManager.SetPracticeMode(true);
        dataManager.SetGameMode(false); // Practice mode is always 1 player vs dummy
        dataManager.SetMatchTimer(-1f); // Reset timer to unlimited for practice mode

        Debug.Log("PracticeModeButton: Practice mode enabled, timer reset to unlimited");
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}

