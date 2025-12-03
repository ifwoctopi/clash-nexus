using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the main menu button during practice mode in the game scene.
/// Attach this to a GameObject in SampleScene. It will find and show the main menu button.
/// </summary>
public class PracticeModeMenuManager : MonoBehaviour
{
    [Header("Main Menu Button")]
    [Tooltip("The main menu button GameObject to show during practice mode (optional - will search if not assigned)")]
    [SerializeField] private GameObject mainMenuButton;
    
    [Tooltip("Button text to search for if button not assigned (default: 'Main Menu')")]
    [SerializeField] private string buttonTextToFind = "Main Menu";

    private bool isPracticeMode = false;
    private GameObject foundButton;

    void Start()
    {
        // Check if we're in practice mode
        GameDataManager dataManager = GameDataManager.Instance;
        if (dataManager != null)
        {
            isPracticeMode = dataManager.IsPracticeMode();
        }

        if (isPracticeMode)
        {
            ShowMainMenuButton();
        }
        else
        {
            // Hide main menu button if not in practice mode
            HideMainMenuButton();
        }
    }

    void Update()
    {
        // Re-check practice mode in case it changes
        // But don't hide the button if we're transitioning (button click just happened)
        GameDataManager dataManager = GameDataManager.Instance;
        if (dataManager != null)
        {
            bool currentPracticeMode = dataManager.IsPracticeMode();
            if (currentPracticeMode != isPracticeMode)
            {
                isPracticeMode = currentPracticeMode;
                if (isPracticeMode)
                {
                    ShowMainMenuButton();
                }
                // Don't hide button if practice mode is turned off - might be transitioning
                // The button will be hidden naturally when the scene changes
            }
        }
    }

    /// <summary>
    /// Shows the main menu button during practice mode
    /// </summary>
    private void ShowMainMenuButton()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.SetActive(true);
            Debug.Log("PracticeModeMenuManager: Main menu button shown (from assigned reference)");
            return;
        }

        // Try to find the main menu button
        if (foundButton == null)
        {
            // Search for button by text
            Text[] allTexts = FindObjectsOfType<Text>();
            foreach (Text text in allTexts)
            {
                if (text.text.Contains(buttonTextToFind) || text.text.ToLower().Contains("main menu"))
                {
                    // Found text, check if it's on a button
                    Button button = text.GetComponentInParent<Button>();
                    if (button != null)
                    {
                        foundButton = button.gameObject;
                        foundButton.SetActive(true);
                        Debug.Log($"PracticeModeMenuManager: Found and activated main menu button: {foundButton.name}");
                        return;
                    }
                }
            }

            // Search for button by name
            Button[] allButtons = FindObjectsOfType<Button>();
            foreach (Button button in allButtons)
            {
                if (button.name.ToLower().Contains("main") && button.name.ToLower().Contains("menu"))
                {
                    foundButton = button.gameObject;
                    foundButton.SetActive(true);
                    Debug.Log($"PracticeModeMenuManager: Found and activated main menu button by name: {foundButton.name}");
                    return;
                }
            }

            Debug.LogWarning($"PracticeModeMenuManager: Main menu button not found. Please assign it manually or ensure a button with text '{buttonTextToFind}' exists in the scene.");
        }
        else
        {
            foundButton.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the main menu button when not in practice mode
    /// </summary>
    private void HideMainMenuButton()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.SetActive(false);
        }
        else if (foundButton != null)
        {
            foundButton.SetActive(false);
        }
    }
}

