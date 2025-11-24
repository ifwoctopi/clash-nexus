using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the match timer countdown and displays it on screen.
/// Place this in the game scene and assign a UI Text component to display the timer.
/// </summary>
public class MatchTimer : MonoBehaviour
{
    [Header("UI Display")]
    [Tooltip("Text component to display the timer (optional - will search if not assigned)")]
    [SerializeField] private Text timerText;

    [Header("Timer Settings")]
    [Tooltip("What happens when timer reaches 0")]
    [SerializeField] private bool endMatchOnTimeout = true;

    private float currentTime;
    private bool isTimerActive = false;
    private bool isUnlimited = false;

    void Start()
    {
        // Get timer setting from GameDataManager
        GameDataManager dataManager = GameDataManager.Instance;
        float timerDuration = dataManager.GetMatchTimer();
        
        isUnlimited = dataManager.IsTimerUnlimited();

        if (isUnlimited)
        {
            // Unlimited time - don't show timer or hide it
            if (timerText != null)
            {
                timerText.gameObject.SetActive(false);
            }
            isTimerActive = false;
            Debug.Log("MatchTimer: Unlimited time mode - timer disabled");
        }
        else
        {
            // Set up countdown timer
            currentTime = timerDuration;
            isTimerActive = true;
            
            // Find timer text if not assigned
            if (timerText == null)
            {
                GameObject timerObj = GameObject.Find("TimerText");
                if (timerObj != null)
                {
                    timerText = timerObj.GetComponent<Text>();
                }
            }

            if (timerText != null)
            {
                timerText.gameObject.SetActive(true);
                UpdateTimerDisplay();
            }
            
            Debug.Log($"MatchTimer: Timer started with {timerDuration} seconds");
        }
    }

    void Update()
    {
        if (!isTimerActive || isUnlimited) return;

        // Countdown
        currentTime -= Time.deltaTime;

        // Update display
        if (timerText != null)
        {
            UpdateTimerDisplay();
        }

        // Check if timer reached 0
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            OnTimerExpired();
        }
    }

    /// <summary>
    /// Updates the timer display text
    /// </summary>
    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int seconds = Mathf.FloorToInt(currentTime);
        
        // Always keep timer text white
        timerText.color = Color.white;

        timerText.text = $"{seconds}s";
    }

    /// <summary>
    /// Called when the timer reaches 0
    /// </summary>
    private void OnTimerExpired()
    {
        isTimerActive = false;
        
        if (timerText != null)
        {
            timerText.text = "0s";
            timerText.color = Color.white;
        }

        Debug.Log("MatchTimer: Time expired!");

        if (endMatchOnTimeout)
        {
            // You can add match end logic here
            // For example: determine winner, show end screen, etc.
            EndMatch();
        }
    }

    /// <summary>
    /// Ends the match when timer expires
    /// </summary>
    private void EndMatch()
    {
        // TODO: Add match end logic here
        // For now, just log that the match should end
        Debug.Log("MatchTimer: Match should end - implement match end logic here");
        
        // Example: You might want to:
        // - Determine winner based on health/score
        // - Show end game screen
        // - Return to character select
        // SceneManager.LoadScene("CharacterSelect");
    }

    /// <summary>
    /// Gets the current time remaining
    /// </summary>
    public float GetTimeRemaining()
    {
        return isUnlimited ? -1f : currentTime;
    }

    /// <summary>
    /// Checks if the timer is unlimited
    /// </summary>
    public bool IsUnlimited()
    {
        return isUnlimited;
    }
}

