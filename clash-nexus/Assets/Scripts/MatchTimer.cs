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
    
    [Tooltip("Button to show when match ends (e.g., Continue/Return button)")]
    [SerializeField] private Button endMatchButton;

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

        // Find timer text if not assigned (needed for both unlimited and timed modes)
        if (timerText == null)
        {
            GameObject timerObj = GameObject.Find("TimerText");
            if (timerObj != null)
            {
                timerText = timerObj.GetComponent<Text>();
            }
        }

        // Hide end match button at start
        if (endMatchButton != null)
        {
            endMatchButton.gameObject.SetActive(false);
        }

        if (isUnlimited)
        {
            // Unlimited time - hide timer text but keep reference for winner display
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
        
        // Determine winner and update timer text
        string winnerText = DetermineWinner();
        
        if (timerText != null)
        {
            timerText.text = winnerText;
            timerText.color = Color.white;
        }
        
        // Show end match button
        if (endMatchButton != null)
        {
            endMatchButton.gameObject.SetActive(true);
        }

        Debug.Log($"MatchTimer: Time expired! {winnerText}");

        if (endMatchOnTimeout)
        {
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

    /// <summary>
    /// Called when a player dies
    /// </summary>
    public void OnPlayerDied(int playerNumber)
    {
        isTimerActive = false; // Stop the timer
        
        // Ensure timer text is found if it wasn't already
        if (timerText == null)
        {
            GameObject timerObj = GameObject.Find("TimerText");
            if (timerObj != null)
            {
                timerText = timerObj.GetComponent<Text>();
            }
        }
        
        // Determine winner and update timer text
        string winnerText = DetermineWinner();
        
        if (timerText != null)
        {
            // Make sure timer text is visible (even in unlimited time mode)
            timerText.gameObject.SetActive(true);
            timerText.text = winnerText;
            timerText.color = Color.white;
        }
        
        // Show end match button
        if (endMatchButton != null)
        {
            endMatchButton.gameObject.SetActive(true);
        }
        
        Debug.Log($"MatchTimer: Player {playerNumber} has died! {winnerText}");
        EndMatch();
    }
    
    /// <summary>
    /// Determines which player has more health and returns the winner text
    /// </summary>
    private string DetermineWinner()
    {
        PlayerSpawner spawner = FindObjectOfType<PlayerSpawner>();
        if (spawner == null)
        {
            return "Match Over";
        }
        
        GameObject player1 = spawner.GetSpawnedPlayer(1);
        GameObject player2 = spawner.GetSpawnedPlayer(2);
        
        float player1Health = 0f;
        float player2Health = 0f;
        
        if (player1 != null)
        {
            PlayerHealth p1Health = player1.GetComponent<PlayerHealth>();
            if (p1Health != null)
            {
                player1Health = p1Health.currentHealth;
            }
        }
        
        if (player2 != null)
        {
            PlayerHealth p2Health = player2.GetComponent<PlayerHealth>();
            if (p2Health != null)
            {
                player2Health = p2Health.currentHealth;
            }
        }
        
        // Determine winner based on health
        if (player1Health > player2Health)
        {
            return "Player 1 Wins!";
        }
        else if (player2Health > player1Health)
        {
            return "Player 2 Wins!";
        }
        else
        {
            // Tie - both have same health (or both dead)
            return "Draw!";
        }
    }
}

