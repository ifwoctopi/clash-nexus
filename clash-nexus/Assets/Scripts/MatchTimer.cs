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

    [Header("Audio")]
    [Tooltip("Sound to play when a game finishes and winner is declared")]
    [SerializeField] private AudioClip gameFinishSound;
    
    [Range(0f, 1f)]
    [Tooltip("Volume of the game finish sound")]
    [SerializeField] private float gameFinishSoundVolume = 1f;

    private float currentTime;
    private bool isTimerActive = false;
    private bool isUnlimited = false;
    private bool matchEnded = false;
    private PlayerSpawner playerSpawner;
    private AudioSource audioSource;

    void Start()
    {
        // Find PlayerSpawner to access players
        playerSpawner = FindObjectOfType<PlayerSpawner>();
        
        // Get or create AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
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
        else
        {
            // Even if endMatchOnTimeout is false, still determine winner and show button
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
        // Check if we're in practice mode
        GameDataManager dataManager = GameDataManager.Instance;
        bool isPracticeMode = dataManager != null && dataManager.IsPracticeMode();
        
        // In practice mode, don't show win message or play sound at all (P2 is a dummy)
        if (isPracticeMode)
        {
            Debug.Log("MatchTimer: Practice mode - not showing win message or playing sound");
            return;
        }
        
        // Find timer text if not assigned
        if (timerText == null)
        {
            GameObject timerObj = GameObject.Find("TimerText");
            if (timerObj != null)
            {
                timerText = timerObj.GetComponent<Text>();
            }
        }
        
        // Play game finish sound (only if not in practice mode)
        if (!isPracticeMode && gameFinishSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(gameFinishSound, gameFinishSoundVolume);
            Debug.Log("MatchTimer: Game finish sound played");
        }
        
        // Display win message in timer text
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
            Debug.Log($"MatchTimer: Win message displayed: {timerText.text}");
        }
        else
        {
            Debug.LogWarning("MatchTimer: TimerText not found, cannot display win message");
        }
        
        // Show win button - try multiple methods to find it
        bool buttonFound = false;
        
        if (winButton != null)
        {
            winButton.gameObject.SetActive(true);
            buttonFound = true;
            Debug.Log("MatchTimer: Win button shown (from serialized field)");
        }
        else
        {
            // Try to find it by name
            GameObject buttonObj = GameObject.Find("WinButton");
            if (buttonObj != null)
            {
                winButton = buttonObj.GetComponent<Button>();
                if (winButton != null)
                {
                    winButton.gameObject.SetActive(true);
                    buttonFound = true;
                    Debug.Log("MatchTimer: Win button found and shown (by name)");
                }
            }
        }
        
        // If still not found, try finding any button with "Win" in the name
        if (!buttonFound)
        {
            Button[] allButtons = FindObjectsOfType<Button>();
            foreach (Button btn in allButtons)
            {
                if (btn.name.ToLower().Contains("win") || btn.name.ToLower().Contains("menu") || btn.name.ToLower().Contains("next"))
                {
                    winButton = btn;
                    winButton.gameObject.SetActive(true);
                    buttonFound = true;
                    Debug.Log($"MatchTimer: Win button found and shown: {btn.name}");
                    break;
                }
            }
        }
        
        if (!buttonFound)
        {
            Debug.LogWarning("MatchTimer: Win button not found! Please create a UI Button named 'WinButton' or assign it in the Inspector.");
        }
        
        Debug.Log($"MatchTimer: Player {winner} wins! Button shown: {buttonFound}");
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
    /// Checks if the match has ended
    /// </summary>
    public bool IsMatchEnded()
    {
        return matchEnded;
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

