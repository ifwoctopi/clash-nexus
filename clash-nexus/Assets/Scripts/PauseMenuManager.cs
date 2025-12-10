using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the pause menu functionality in the battle scene.
/// Press ESC to pause/unpause. Handles resume, restart, and quit to menu.
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The pause menu panel (will be shown/hidden). If not assigned, will search for 'PauseMenuPanel'")]
    [SerializeField] private GameObject pauseMenuPanel;
    
    [Tooltip("Resume button. If not assigned, will search for 'ResumeButton'")]
    [SerializeField] private Button resumeButton;
    
    [Tooltip("Restart button. If not assigned, will search for 'RestartButton'")]
    [SerializeField] private Button restartButton;
    
    [Tooltip("Quit to Menu button. If not assigned, will search for 'QuitToMenuButton'")]
    [SerializeField] private Button quitToMenuButton;

    [Header("Settings")]
    [Tooltip("Key to press to pause/unpause (default: Escape)")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    
    [Tooltip("Allow pausing during practice mode?")]
    [SerializeField] private bool allowPauseInPracticeMode = true;
    
    [Tooltip("Allow pausing when match has ended?")]
    [SerializeField] private bool allowPauseAfterMatchEnd = false;

    private bool isPaused = false;
    private MatchTimer matchTimer;

    void Start()
    {
        // Find UI elements if not assigned
        if (pauseMenuPanel == null)
        {
            GameObject panelObj = GameObject.Find("PauseMenuPanel");
            if (panelObj != null)
            {
                pauseMenuPanel = panelObj;
            }
        }

        if (resumeButton == null)
        {
            GameObject resumeObj = GameObject.Find("ResumeButton");
            if (resumeObj != null)
            {
                resumeButton = resumeObj.GetComponent<Button>();
            }
        }

        if (restartButton == null)
        {
            GameObject restartObj = GameObject.Find("RestartButton");
            if (restartObj != null)
            {
                restartButton = restartObj.GetComponent<Button>();
            }
        }

        if (quitToMenuButton == null)
        {
            GameObject quitObj = GameObject.Find("QuitToMenuButton");
            if (quitObj != null)
            {
                quitToMenuButton = quitObj.GetComponent<Button>();
            }
        }

        // Set up button listeners
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartMatch);
        }

        if (quitToMenuButton != null)
        {
            quitToMenuButton.onClick.AddListener(QuitToMenu);
        }

        // Hide pause menu initially
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Find MatchTimer to check if match has ended
        matchTimer = FindObjectOfType<MatchTimer>();
    }

    void Update()
    {
        // Check for pause key press
        if (Input.GetKeyDown(pauseKey))
        {
            // Check if we can pause
            if (!CanPause())
            {
                return;
            }

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    /// <summary>
    /// Checks if the game can be paused
    /// </summary>
    private bool CanPause()
    {
        // Check practice mode
        GameDataManager dataManager = GameDataManager.Instance;
        if (dataManager != null && dataManager.IsPracticeMode() && !allowPauseInPracticeMode)
        {
            return false;
        }

        // Check if match has ended
        if (matchTimer != null && !allowPauseAfterMatchEnd)
        {
            if (matchTimer.IsMatchEnded())
            {
                return false; // Don't allow pausing after match has ended
            }
        }

        return true;
    }

    /// <summary>
    /// Pauses the game
    /// </summary>
    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f; // Freeze game time

        // Show pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        Debug.Log("PauseMenuManager: Game paused");
    }

    /// <summary>
    /// Resumes the game
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f; // Resume game time

        // Hide pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        Debug.Log("PauseMenuManager: Game resumed");
    }

    /// <summary>
    /// Restarts the current match
    /// </summary>
    public void RestartMatch()
    {
        // Resume time before restarting
        Time.timeScale = 1f;
        isPaused = false;

        // Reload the current scene
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"PauseMenuManager: Restarting match - reloading scene '{currentScene}'");
        SceneManager.LoadScene(currentScene);
    }

    /// <summary>
    /// Quits to main menu
    /// </summary>
    public void QuitToMenu()
    {
        // Resume time before quitting
        Time.timeScale = 1f;
        isPaused = false;

        // Reset practice mode if needed
        GameDataManager dataManager = GameDataManager.Instance;
        if (dataManager != null)
        {
            dataManager.SetPracticeMode(false);
            dataManager.ClearBackgroundSelection();
        }

        Debug.Log("PauseMenuManager: Quitting to main menu");
        SceneManager.LoadScene("MainMenuScreen");
    }

    /// <summary>
    /// Gets whether the game is currently paused
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }

    void OnDestroy()
    {
        // Ensure time scale is reset when this object is destroyed
        Time.timeScale = 1f;

        // Unsubscribe from button events
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartMatch);
        }

        if (quitToMenuButton != null)
        {
            quitToMenuButton.onClick.RemoveListener(QuitToMenu);
        }
    }
}

