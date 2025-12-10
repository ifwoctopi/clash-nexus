using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    public int playerNumber;

    public int attackAttempts;
    public int hitsLanded;
    public int hitsReceived;

    public int totalDamageDealt;
    public int totalDamageTaken;

    public int knockouts;

    public PlayerStats(int number)
    {
        playerNumber = number;
        Debug.Log($"[STATS INIT] Created stats for Player {number}");
    }

    // Called when this player DEALS damage
    public void RegisterDamageDealt(int dmg)
    {
        hitsLanded++;
        totalDamageDealt += dmg;

        Debug.Log($"[STATS] Player {playerNumber} DEALT {dmg} damage | " +
                  $"TotalDealt={totalDamageDealt}, HitsLanded={hitsLanded}");
    }

    // Called when this player TAKES damage
    public void RegisterDamageTaken(int dmg)
    {
        hitsReceived++;
        totalDamageTaken += dmg;

        Debug.Log($"[STATS] Player {playerNumber} TOOK {dmg} damage | " +
                  $"TotalTaken={totalDamageTaken}, HitsReceived={hitsReceived}");
    }

    // Called when this player attempts an attack
    public void RegisterAttackAttempt()
    {
        attackAttempts++;

        Debug.Log($"[STATS] Player {playerNumber} ATTEMPTED ATTACK | " +
                  $"Attempts={attackAttempts}");
    }

    // Called when this player KO’s someone
    public void RegisterKnockout()
    {
        knockouts++;

        Debug.Log($"[STATS] Player {playerNumber} SCORED A KO | " +
                  $"TotalKOs={knockouts}");
    }
}

