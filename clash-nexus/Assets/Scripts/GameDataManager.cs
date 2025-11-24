using UnityEngine;

/// <summary>
/// Manages game data that persists between scenes (like selected characters).
/// This is a singleton that persists across scene loads.
/// </summary>
public class GameDataManager : MonoBehaviour
{
    private static GameDataManager instance;

    [Header("Selected Characters")]
    private string player1CharacterId = "";
    private string player2CharacterId = "";

    [Header("Game Mode")]
    private bool isTwoPlayerMode = false; // false = 1 player (vs CPU), true = 2 players

    [Header("Match Timer")]
    // Timer options: -1 = unlimited, 30 = 30 seconds, 60 = 60 seconds, etc.
    private float matchTimerSeconds = -1f; // Default to unlimited

    void Awake()
    {
        // Ensure only one instance exists (singleton pattern)
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Gets the singleton instance
    /// </summary>
    public static GameDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameDataManager");
                instance = go.AddComponent<GameDataManager>();
            }
            return instance;
        }
    }

    /// <summary>
    /// Sets the selected character for a player
    /// </summary>
    public void SetPlayerCharacter(int playerNumber, string characterId)
    {
        if (playerNumber == 1)
        {
            player1CharacterId = characterId;
        }
        else if (playerNumber == 2)
        {
            player2CharacterId = characterId;
        }
    }

    /// <summary>
    /// Gets the selected character ID for a player
    /// </summary>
    public string GetPlayerCharacter(int playerNumber)
    {
        if (playerNumber == 1)
        {
            return player1CharacterId;
        }
        else if (playerNumber == 2)
        {
            return player2CharacterId;
        }
        return "";
    }

    /// <summary>
    /// Clears all selected characters (useful for reset)
    /// </summary>
    public void ClearSelections()
    {
        player1CharacterId = "";
        player2CharacterId = "";
    }

    /// <summary>
    /// Sets the game mode (1 player vs CPU, or 2 players)
    /// </summary>
    public void SetGameMode(bool twoPlayerMode)
    {
        isTwoPlayerMode = twoPlayerMode;
    }

    /// <summary>
    /// Gets whether the game is in 2-player mode
    /// </summary>
    public bool IsTwoPlayerMode()
    {
        return isTwoPlayerMode;
    }

    /// <summary>
    /// Sets the match timer duration in seconds (-1 for unlimited)
    /// </summary>
    public void SetMatchTimer(float seconds)
    {
        matchTimerSeconds = seconds;
        Debug.Log($"GameDataManager: Match timer set to {(seconds < 0 ? "Unlimited" : seconds + " seconds")}");
    }

    /// <summary>
    /// Gets the match timer duration in seconds (-1 for unlimited)
    /// </summary>
    public float GetMatchTimer()
    {
        return matchTimerSeconds;
    }

    /// <summary>
    /// Checks if the match timer is unlimited
    /// </summary>
    public bool IsTimerUnlimited()
    {
        return matchTimerSeconds < 0f;
    }
}

