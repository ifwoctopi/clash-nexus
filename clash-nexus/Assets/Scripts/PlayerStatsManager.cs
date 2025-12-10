using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance;

    private Dictionary<int, PlayerStats> stats = new Dictionary<int, PlayerStats>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("[STATS MANAGER] Duplicate instance detected — destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[STATS MANAGER] Initialized and marked DontDestroyOnLoad.");

        Initialize(2);
    }

    public void Initialize(int playerCount)
    {
        stats.Clear();
        Debug.Log($"[STATS MANAGER] Initializing stats for {playerCount} players.");

        for (int i = 1; i <= playerCount; i++)
        {
            stats[i] = new PlayerStats(i);
            Debug.Log($"[STATS MANAGER] Player {i} stats created.");
        }
    }

    public PlayerStats GetStats(int number)
    {
        if (stats.TryGetValue(number, out var s))
        {
            Debug.Log($"[STATS MANAGER] Returned stats for Player {number}");
            return s;
        }

        Debug.LogError($"[STATS MANAGER] ERROR! No stats found for Player {number}!");
        return null;
    }
}
