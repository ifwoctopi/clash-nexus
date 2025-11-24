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
    
    [Tooltip("Button that appears when a player wins (optional)")]
    [SerializeField] private Button winButton;

    [Header("Timer Settings")]
    [Tooltip("What happens when timer reaches 0")]
    [SerializeField] private bool endMatchOnTimeout = true;

    private float currentTime;
    private bool isTimerActive = false;
    private bool isUnlimited = false;
    private bool matchEnded = false;
    private PlayerSpawner playerSpawner;

    void Start()
    {
        // Find PlayerSpawner to access players
        playerSpawner = FindObjectOfType<PlayerSpawner>();
        
        // Find win button if not assigned
        if (winButton == null)
        {
            GameObject buttonObj = GameObject.Find("WinButton");
            if (buttonObj != null)
            {
                winButton = buttonObj.GetComponent<Button>();
            }
        }
        
        // Hide win button initially
        if (winButton != null)
        {
            winButton.gameObject.SetActive(false);
        }
        
        // Get timer setting from GameDataManager
        GameDataManager dataManager = GameDataManager.Instance;
        float timerDuration = dataManager.GetMatchTimer();
        
        isUnlimited = dataManager.IsTimerUnlimited();

        if (isUnlimited)
        {
            // Unlimited time - show timer text but don't countdown
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
                timerText.text = ""; // Empty for unlimited time
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
        // Check for win conditions even if timer is unlimited
        if (!matchEnded)
        {
            CheckForWinCondition();
        }
        
        if (!isTimerActive || isUnlimited || matchEnded) return;

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

        Debug.Log("MatchTimer: Time expired!");

        if (endMatchOnTimeout)
        {
            // Determine winner based on health
            DetermineWinner();
        }
    }
    
    /// <summary>
    /// Checks for win conditions (player death or timer expiry)
    /// </summary>
    private void CheckForWinCondition()
    {
        if (playerSpawner == null) return;
        
        GameObject player1 = playerSpawner.GetSpawnedPlayer(1);
        GameObject player2 = playerSpawner.GetSpawnedPlayer(2);
        
        if (player1 == null || player2 == null) return;
        
        PlayerHealth p1Health = player1.GetComponent<PlayerHealth>();
        PlayerHealth p2Health = player2.GetComponent<PlayerHealth>();
        
        if (p1Health == null || p2Health == null) return;
        
        // Check if either player has 0 or less health
        if (p1Health.currentHealth <= 0 || p2Health.currentHealth <= 0)
        {
            DetermineWinner();
        }
    }
    
    /// <summary>
    /// Determines the winner based on health and displays win message
    /// </summary>
    private void DetermineWinner()
    {
        if (matchEnded) return; // Prevent multiple calls
        
        matchEnded = true;
        isTimerActive = false;
        
        if (playerSpawner == null)
        {
            playerSpawner = FindObjectOfType<PlayerSpawner>();
        }
        
        if (playerSpawner == null)
        {
            Debug.LogWarning("MatchTimer: PlayerSpawner not found, cannot determine winner");
            return;
        }
        
        GameObject player1 = playerSpawner.GetSpawnedPlayer(1);
        GameObject player2 = playerSpawner.GetSpawnedPlayer(2);
        
        if (player1 == null || player2 == null)
        {
            Debug.LogWarning("MatchTimer: Players not found, cannot determine winner");
            return;
        }
        
        PlayerHealth p1Health = player1.GetComponent<PlayerHealth>();
        PlayerHealth p2Health = player2.GetComponent<PlayerHealth>();
        
        if (p1Health == null || p2Health == null)
        {
            Debug.LogWarning("MatchTimer: PlayerHealth components not found, cannot determine winner");
            return;
        }
        
        // Determine winner based on health
        int winner = 1;
        if (p1Health.currentHealth <= 0 && p2Health.currentHealth > 0)
        {
            winner = 2;
        }
        else if (p2Health.currentHealth <= 0 && p1Health.currentHealth > 0)
        {
            winner = 1;
        }
        else if (p1Health.currentHealth > p2Health.currentHealth)
        {
            winner = 1;
        }
        else if (p2Health.currentHealth > p1Health.currentHealth)
        {
            winner = 2;
        }
        else
        {
            // Tie - both have same health (or both dead)
            winner = 0;
        }
        
        // Display win message
        ShowWinMessage(winner);
    }
    
    /// <summary>
    /// Displays the win message in the timer text and shows the win button
    /// </summary>
    private void ShowWinMessage(int winner)
    {
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
            
            if (winner == 0)
            {
                timerText.text = "Tie!";
            }
            else
            {
                timerText.text = $"Player {winner} Wins!";
            }
            timerText.color = Color.yellow; // Make it stand out
        }
        
        // Show win button
        if (winButton != null)
        {
            winButton.gameObject.SetActive(true);
        }
        else
        {
            // Try to find it again
            GameObject buttonObj = GameObject.Find("WinButton");
            if (buttonObj != null)
            {
                winButton = buttonObj.GetComponent<Button>();
                if (winButton != null)
                {
                    winButton.gameObject.SetActive(true);
                }
            }
        }
        
        Debug.Log($"MatchTimer: Player {winner} wins!");
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
    /// Called when a player dies. Can be used to stop the timer or trigger match-end logic.
    /// </summary>
    public void OnPlayerDied()
    {
        // Stop the timer
        isTimerActive = false;

        Debug.Log("MatchTimer: A player died - checking for winner.");

        // Check for winner (will determine based on health)
        if (!matchEnded)
        {
            DetermineWinner();
        }
    }
}

